using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
namespace E2E_test;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class E2E_test : PageTest
{
    string _baseUrl = "http://127.0.0.1:5235/public";

    [Test]
    public async Task SignUpandLogInTest()
    {
        await Page.GotoAsync($"{_baseUrl}");

        await Page.GetByRole(AriaRole.Link, new() { Name = "sign up" }).ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.FillAsync("adsadsa");

        await Page.GetByRole(AriaRole.Textbox).First.PressAsync("Tab");

        await Page.GetByRole(AriaRole.Textbox).Nth(1).FillAsync("adsadsa@adsadsa.dk");

        await Page.GetByRole(AriaRole.Textbox).Nth(1).PressAsync("Tab");

        await Page.GetByRole(AriaRole.Textbox).Nth(2).FillAsync("adsadsa");

        await Page.GetByRole(AriaRole.Textbox).Nth(2).PressAsync("Tab");

        await Page.GetByRole(AriaRole.Textbox).Nth(3).FillAsync("adsadsa");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign Up" }).ClickAsync();

        await Page.GotoAsync($"{_baseUrl}");

        await Page.GetByRole(AriaRole.Link, new() { Name = "sign in" }).ClickAsync();

        await Page.Locator("input[name=\"username\"]").ClickAsync();

        await Page.Locator("input[name=\"username\"]").FillAsync("adsadsa");

        await Page.Locator("input[name=\"password\"]").ClickAsync();

        await Page.Locator("input[name=\"password\"]").FillAsync("adsadsa");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

        await Expect(Page.GetByText("What's on your mind adsadsa?")).ToBeEnabledAsync();
    }
    [Test]
    public async Task MakeATweetTest()
    {
        await CreateUser("asdasd", "asdasd");

        await Page.GotoAsync($"{_baseUrl}");

        await Page.GetByRole(AriaRole.Link, new() { Name = "sign in" }).ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.FillAsync("asdasd");

        await Page.GetByRole(AriaRole.Textbox).First.PressAsync("Tab");

        await Page.GetByRole(AriaRole.Textbox).Nth(1).FillAsync("asdasd");

        await Page.GetByRole(AriaRole.Textbox).Nth(1).PressAsync("Enter");

        await Page.GetByRole(AriaRole.Textbox).First.ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.FillAsync("asdasd");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await Expect(Page.GetByText("You succesfully shared a Twit! 🐧")).ToBeEnabledAsync();
    }
    public async Task CreateUser(string username, string password)
    {

        await Page.GotoAsync($"{_baseUrl}");

        await Page.GetByRole(AriaRole.Link, new() { Name = "sign up" }).ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.FillAsync(username);

        await Page.GetByRole(AriaRole.Textbox).Nth(1).ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).Nth(1).FillAsync($"{username}@email.com");

        await Page.GetByRole(AriaRole.Textbox).Nth(1).PressAsync("Tab");

        await Page.GetByRole(AriaRole.Textbox).Nth(2).FillAsync(password);

        await Page.GetByRole(AriaRole.Textbox).Nth(2).PressAsync("Tab");

        await Page.GetByRole(AriaRole.Textbox).Nth(3).FillAsync(password);

        await Page.Locator("html").ClickAsync();

        await Page.GetByText("Username: E-Mail: Password: Password (repeat): Sign Up").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign Up" }).ClickAsync();




    }
}
