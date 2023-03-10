namespace e2e;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class SignInSignUpSignOutTests : PageTest
{
    [SetUp]
    public void Setup()
    {
    }


    [Test]
    public async Task TestSignIn()
    {
        await Page.GotoAsync("http://localhost:5235/public");

        await Page.GetByRole(AriaRole.Link, new() { Name = "sign in" }).ClickAsync();

        await Page.Locator("input[name=\"username\"]").ClickAsync();

        await Page.Locator("input[name=\"username\"]").FillAsync("asdasd");

        await Page.Locator("input[name=\"username\"]").PressAsync("Tab");

        await Page.Locator("input[name=\"password\"]").FillAsync("asdasd");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

        Page.GetByRole(AriaRole.Link, new() { Name = "sign out [asdasd]" }).Should().NotBeNull();

        await Page.GetByRole(AriaRole.Link, new() { Name = "sign out [asdasd]" }).ClickAsync();

        Page.GetByRole(AriaRole.Link, new() { Name = "sign in" }).Should().NotBeNull();
    }
}
