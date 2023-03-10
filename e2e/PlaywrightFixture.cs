namespace e2e;
public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; }
    public Lazy<Task<IBrowser>> ChromiumBrowser { get; private set; }
    public Lazy<Task<IBrowser>> FirefoxBrowser { get; private set; }
    public Lazy<Task<IBrowser>> WebkitBrowser { get; private set; }
    public enum Browser
    {
        Chromium,
        Firefox,
        Webkit,
    }
    public async Task InitializeAsync()
    {
        // Install Playwright and its dependencies.
        InstallPlaywright();
        // Create Playwright module.
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        // Setup Browser lazy initializers.
        ChromiumBrowser = new Lazy<Task<IBrowser>>(Playwright.Chromium.LaunchAsync());
        FirefoxBrowser = new Lazy<Task<IBrowser>>(Playwright.Firefox.LaunchAsync());
        WebkitBrowser = new Lazy<Task<IBrowser>>(Playwright.Webkit.LaunchAsync());
    }

    private static void InstallPlaywright()
    {
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install-deps" });
        if (exitCode != 0)
        {
            throw new Exception(
              $"Playwright exited with code {exitCode} on install-deps");
        }
        exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
        if (exitCode != 0)
        {
            throw new Exception(
              $"Playwright exited with code {exitCode} on install");
        }
    }

    public const string PlaywrightCollection = nameof(PlaywrightCollection);
    [CollectionDefinition(PlaywrightCollection)]
    public class PlaywrightCollectionDefinition : ICollectionFixture<PlaywrightFixture>
    {
        // This class is just xUnit plumbing code to apply
        // [CollectionDefinition] and the ICollectionFixture<>
        // interfaces. Witch in our case is parametrized
        // with the PlaywrightFixture.
    }

    public async Task GotoPageAsync(string url, Func<IPage, Task> testHandler, Browser browserType)
    {
        // select and launch the browser.
        var browser = await SelectBrowserAsync(browserType);
        // Create a new context with an option to ignore HTTPS errors.
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
        // Open a new page.
        var page = await context.NewPageAsync();
        page.Should().NotBeNull();
        try
        {
            // Navigate to the given URL and wait until loading
            // network activity is done.
            var gotoResult = await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            gotoResult.Should().NotBeNull();
            await gotoResult.FinishedAsync();
            gotoResult.Ok.Should().BeTrue();
            // Run the actual test logic.
            await testHandler(page);
        }
        finally
        {
            // Make sure the page is closed 
            await page.CloseAsync();
        }
    }

    private Task<IBrowser> SelectBrowserAsync(Browser browser)
    {
        return browser switch
        {
            Browser.Chromium => ChromiumBrowser.Value,
            Browser.Firefox => FirefoxBrowser.Value,
            Browser.Webkit => WebkitBrowser.Value,
            _ => throw new NotImplementedException(),
        };
    }

    public async Task DisposeAsync()
    {
        if (Playwright != null)
        {
            if (ChromiumBrowser != null && ChromiumBrowser.IsValueCreated)
            {
                var browser = await ChromiumBrowser.Value;
                await browser.DisposeAsync();
            }
            if (FirefoxBrowser != null && FirefoxBrowser.IsValueCreated)
            {
                var browser = await FirefoxBrowser.Value;
                await browser.DisposeAsync();
            }
            if (WebkitBrowser != null && WebkitBrowser.IsValueCreated)
            {
                var browser = await WebkitBrowser.Value;
                await browser.DisposeAsync();
            }
            Playwright.Dispose();
            Playwright = null;
        }
    }
}