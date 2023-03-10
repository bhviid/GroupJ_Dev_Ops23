namespace e2e;
public class SignUpSignInE2E
{
    private readonly PlaywrightFixture playwrightFixture;
    private string url = "http://localhost:5235/public";
    public SignUpSignInE2E(PlaywrightFixture playwrightFixture)
    {
        this.playwrightFixture = playwrightFixture;
    }

    [Fact]
    public async Task SignUpSignInTest()
    {
        await playwrightFixture.GotoPageAsync(url, async (page) =>
        {
            await page.GetByRole(AriaRole.Link, new() { Name = "sign up" }).ClickAsync();

            await page.GetByRole(AriaRole.Textbox).First.ClickAsync();

            await page.GetByRole(AriaRole.Textbox).First.FillAsync("asdasd");

            await page.GetByRole(AriaRole.Textbox).First.PressAsync("Tab");

            await page.GetByRole(AriaRole.Textbox).Nth(1).FillAsync("asd@asd.com");

            await page.GetByRole(AriaRole.Textbox).Nth(1).PressAsync("Tab");

            await page.GetByRole(AriaRole.Textbox).Nth(2).FillAsync("asdasd");

            await page.GetByRole(AriaRole.Textbox).Nth(2).PressAsync("Tab");

            await page.GetByRole(AriaRole.Textbox).Nth(3).FillAsync("asdasd");

            await page.GetByRole(AriaRole.Button, new() { Name = "Sign Up" }).ClickAsync();

            await page.GetByRole(AriaRole.Link, new() { Name = "sign out [asdasd]" }).ClickAsync();

            await page.GetByRole(AriaRole.Link, new() { Name = "sign in" }).ClickAsync();

            await page.Locator("input[name=\"username\"]").ClickAsync();

            await page.Locator("input[name=\"username\"]").FillAsync("asdasd");

            await page.Locator("input[name=\"username\"]").PressAsync("Tab");

            await page.Locator("input[name=\"password\"]").FillAsync("asdasd");

            await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

            page.GetByRole(AriaRole.Link, new() { Name = "sign out [asdasd]" }).Should().NotBeNull();
        }, PlaywrightFixture.Browser.Chromium);
    }
}