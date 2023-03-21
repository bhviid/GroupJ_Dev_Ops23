using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    [Test]
    public async Task LogInTest()
    {
        await Page.GotoAsync("http://0.0.0.0:5235/public");

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
        await Page.GotoAsync("http://0.0.0.0:5235/public");

        await Page.GetByRole(AriaRole.Link, new() { Name = "sign in" }).ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.FillAsync("asdasd");

        await Page.GetByRole(AriaRole.Textbox).First.PressAsync("Tab");

        await Page.GetByRole(AriaRole.Textbox).Nth(1).FillAsync("asdasd");

        await Page.GetByRole(AriaRole.Textbox).Nth(1).PressAsync("Enter");

        await Page.GetByRole(AriaRole.Textbox).First.ClickAsync();

        await Page.GetByRole(AriaRole.Textbox).First.FillAsync("asdasd");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await Expect (Page.GetByText("You succesfully shared a Twit! 🐧")).ToBeEnabledAsync();
    }
}
