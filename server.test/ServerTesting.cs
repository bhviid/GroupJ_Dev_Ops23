namespace server.test;

using System.Net;
using MiniTwit.Shared;

/* 

    MiniTwit Tests
    ~~~~~~~~~~~~~~

    Tests the MiniTwit application.

    :copyright: (c) 2010 by Armin Ronacher.
    :license: BSD, see LICENSE for more details.

 */

public class server_testing : IClassFixture<WebTestFixture>
{
    private readonly HttpClient _client;
    private readonly WebTestFixture _factory;
    public server_testing(WebTestFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async void TestSimpleRegister()
    {
        var response = await Register("user0", "default", "default");
        response.Should().BeSuccessful();
    }

    [Fact]
    public async void TestRegister()
    {
        var response = await Register("user1", "default", "default");
        response.Should().HaveStatusCode(HttpStatusCode.Created);

        response = await Register("user1", "default", "default");
        response.Should().HaveClientError();

        response = await Register("", "default", "default");
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        response = await Register("meh", "", "");
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        response = await Register("meh", "x", "y");
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        response = await Register("meh", "foo", email: "broken");
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
    }
    /*     [Fact]
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
     */
    /*     [Fact]
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
     */
    /* [Fact]
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
    } */

    // helper functions
    private async Task<HttpResponseMessage> Register(string username, string password, string password2 = "", string? email = null)
    {
        if (email == null) email = username + "@example.com";
        var content = Utility_Methods.stringify_object(new UserCreateDTO(username, email, password, password2));
        var resp = await _client.PostAsync("minitwit/register", content);
        return resp;
    }

    private async Task<HttpResponseMessage> Login(string username, string password)
    {
        var content = Utility_Methods.stringify_object(new UserDTO(username, "", password));
        return await _client.PostAsync("minitwit/login", content);
    }

    private async Task<HttpResponseMessage> RegisterAndLogin(string username, string password)
    {
        await Register(username, password);
        return await Login(username, password);
    }

    private async Task<HttpResponseMessage> Logout()
    {
        return await _client.GetAsync("minitwit/logout");
    }

    private async Task<HttpResponseMessage> AddMessage(string text)
    {
        var content = Utility_Methods.stringify_object(new Message
        {
            AuthorId = 0,
            Text = text,
            PubDate = DateTime.Now,
            Flagged = 0
        });
        // Console.WriteLine(json);
        var response = await _client.PostAsync("minitwit/add_message", content);
        return response;
    }
}