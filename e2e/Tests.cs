using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    [Test]
    public async Task MyTest()
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
}
