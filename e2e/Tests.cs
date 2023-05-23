using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
namespace E2E_test;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class E2E_test : PageTest
{
    string _baseUrl = "http://127.0.0.1:5235/public";

    [Test]
    public async Task LogInTest()
    {
        await CreateUser();

        await Page.GotoAsync($"{_baseUrl}");

        await Page.GetByRole(AriaRole.Link, new() { Name = "sign in" }).ClickAsync();

        await Page.Locator("input[name=\"username\"]").ClickAsync();

        await Page.Locator("input[name=\"username\"]").FillAsync("asdasd");

        await Page.Locator("input[name=\"username\"]").PressAsync("Tab");

        await Page.Locator("input[name=\"password\"]").FillAsync("asdasd");

        await Page.Locator("input[name=\"password\"]").PressAsync("Enter");

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "sign out [asdasd]" })).ToBeEnabledAsync();
    }
    [Test]
    public async Task MakeATweetTest()
    {
        await CreateUser();

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
    public async Task CreateUser()
    {

        await Page.GotoAsync($"{_baseUrl}");

        await Page.GetByRole(AriaRole.Link, new() { Name = "sign up" }).ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.FillAsync("asdasd");

        await Page.GetByRole(AriaRole.Textbox).Nth(1).ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).Nth(1).FillAsync("asdasd@asdasd.dk");

        await Page.GetByRole(AriaRole.Textbox).Nth(1).PressAsync("Tab");

        await Page.GetByRole(AriaRole.Textbox).Nth(2).FillAsync("asdasd");

        await Page.GetByRole(AriaRole.Textbox).Nth(2).PressAsync("Tab");

        await Page.GetByRole(AriaRole.Textbox).Nth(3).FillAsync("asdasd");

        await Page.Locator("html").ClickAsync();

        await Page.GetByText("Username: E-Mail: Password: Password (repeat): Sign Up").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign Up" }).ClickAsync();


    }
}
