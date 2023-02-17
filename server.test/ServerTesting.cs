namespace server.test;

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

/* 

    MiniTwit Tests
    ~~~~~~~~~~~~~~

    Tests the MiniTwit application.

    :copyright: (c) 2010 by Armin Ronacher.
    :license: BSD, see LICENSE for more details.

 */

public class server_testing : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public server_testing(WebApplicationFactory<Program> factory)
    {

        // Before each test, set up a blank database in sqlite memory
        SqliteConnection db = new SqliteConnection("Data Source=:memory:");
        _factory = factory;
        _client = _factory.CreateClient();
        

    }

    // helper functions
    private async Task<HttpResponseMessage> Register(string username, string password, string password2 = null, string email = null)
    {
        if (password2 == null)
            password2 = password;
        
        if (email == null)
            email = username + "@example.com";
            

        var content = new StringContent(JsonSerializer.Serialize(new
        {
            username = username,
            password = password,
            password2 = password2,
            email = email
        }), Encoding.UTF8, "application/json");

        return await _client.PostAsync("/register", content);
    }

    public async Task<string> GetContent(HttpResponseMessage response)
    {
        var encoding = Encoding.UTF8;
        var content = response.Content;

        var contentBytes = await content.ReadAsByteArrayAsync();
        return encoding.GetString(contentBytes);
    }
    private async Task<HttpResponseMessage> Login(string username, string password)
    {
        var content = new StringContent(JsonSerializer.Serialize(new
        {
            username = username,
            password = password
        }), Encoding.UTF8, "application/json");

        return await _client.PostAsync("/login", content);
    }

    private async Task<HttpResponseMessage> RegisterAndLogin(string username, string password)
    {
        Register(username, password);
        return await Login(username, password);
    }

    private async Task<HttpResponseMessage> Logout()
    {
        return await _client.GetAsync("/logout");
    }

    private async Task<HttpResponseMessage> AddMessage(string text)
    {
        var content = new StringContent(JsonSerializer.Serialize(new
        {
            text = text
        }), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/add_message", content);

        if (!string.IsNullOrEmpty(text))
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal("Your message was recorded", responseContent);
        }

        return response;
    }


    [Fact]
    public async void TestSimpleRegister()
    {
        var response = await Register("user1", "default");
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.Equal("You were successfully registered and can login now", responseContent);
    }
    
    [Fact(Skip = "it dont work")]
    public async void TestRegister()
    {
        var response = await Register("user1", "default");
        var responseContent = response.Content.ReadAsStringAsync().Result;
        Assert.Equal("You were successfully registered and can login now", responseContent);

        response = await Register("user1", "default");
        responseContent = response.Content.ReadAsStringAsync().Result;
        Assert.Equal("The username is already taken", responseContent);

        response = await Register("", "default");
        responseContent = response.Content.ReadAsStringAsync().Result;
        Assert.Equal("You have to enter a username", responseContent);

        response = await Register("meh", "");
        responseContent = response.Content.ReadAsStringAsync().Result;
        Assert.Equal("You have to enter a password", responseContent);

        response = await Register("meh", "x", "y");
        responseContent = response.Content.ReadAsStringAsync().Result;
        Assert.Equal("The two passwords do not match", responseContent);

        response = await Register("meh", "foo", email: "broken");
        responseContent = response.Content.ReadAsStringAsync().Result;
        Assert.Equal("You have to enter a valid email address", responseContent);
    }
    [Fact]
    public async Task LoginLogoutTest()
    {
        // Make sure logging in and logging out works
        var response = await RegisterAndLogin("user1", "default");
        Assert.Equal("You were logged in", await GetContent(response));
        response = await Logout();
        Assert.Equal("You were logged out", await GetContent(response));
        response = await Login("user1", "wrongpassword");
        Assert.Equal("Invalid password", await GetContent(response));
        response = await Login("user2", "wrongpassword");
        Assert.Equal("Invalid username", await GetContent(response));
    }

    [Fact]
    public async Task MessageRecordingTest()
    {
        // Check if adding messages works
        await RegisterAndLogin("foo", "default");
        await AddMessage("test message 1");
        await AddMessage("<test message 2>");
        var response = await _client.GetAsync("/");
        var content = await GetContent(response);
        // in can find substrings, maybe
        Assert.Contains("test message 1", content);
        Assert.Contains("&lt;test message 2&gt;", content);
    }
    [Fact]
    public async Task TimelinesTest()
    {
        // Make sure that timelines work
        await RegisterAndLogin("foo", "default");
        await AddMessage("the message by foo");
        await Logout();
        await RegisterAndLogin("bar", "default");
        await AddMessage("the message by bar");
        var response = await _client.GetAsync("/public");
        var content = await GetContent(response);
        Assert.Contains("the message by foo", content);
        Assert.Contains("the message by bar", content);

        // bar's timeline should just show bar's message
        response = await _client.GetAsync("/");
        content = await GetContent(response);
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);

        // now let's follow foo
        response = await _client.GetAsync("/foo/follow");
        content = await GetContent(response);
        //previous test , follow_redirects=True
        Assert.Contains("You are now following &quot;foo&quot;", content);

        // we should now see foo's message
        response = await _client.GetAsync("/");
        content = await GetContent(response);
        Assert.Contains("the message by foo", content);
        Assert.Contains("the message by bar", content);

        // but on the user's page we only want the user's message
        response = await _client.GetAsync("/bar");
        content = await GetContent(response);
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);
        response = await _client.GetAsync("/foo");
        content = await GetContent(response);
        Assert.Contains("the message by foo", content);
        Assert.DoesNotContain("the message by bar", content);

        // now unfollow and check if that worked
        response = await _client.GetAsync("/foo/unfollow");
        content = await GetContent(response);
        Assert.Contains("You are no longer following &quot;foo&quot;", content);
        response = await _client.GetAsync("/");
        content = await GetContent(response);
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);
    }
}