using Microsoft.Playwright;

namespace VehicleBookingSystem.Tests.UI;

[Trait("Category", "E2E")]
public class UiSmokeTests
{
    private static bool Enabled =>
        string.Equals(Environment.GetEnvironmentVariable("RUN_UI_SMOKE"), "true", StringComparison.OrdinalIgnoreCase);

    private static string? BaseUrl => Environment.GetEnvironmentVariable("UI_BASE_URL");

    [SkippableFact]
    public async Task ListingFilter_ShouldRenderResultsAfterApply()
    {
        Skip.If(!Enabled || string.IsNullOrWhiteSpace(BaseUrl), "UI smoke tests are disabled. Set RUN_UI_SMOKE=true and UI_BASE_URL to run.");

        await using var app = await LaunchAsync();
        var page = await app.Context.NewPageAsync();

        await page.GotoAsync($"{BaseUrl}/Customer/Index");
        await page.WaitForSelectorAsync("#vehicleFilterForm");

        await page.FillAsync("#vehicleSearchInput", "Toyota");
        await page.ClickAsync("#vehicleFilterForm button[type='submit']");

        await page.WaitForSelectorAsync("#vehicleListContainer");
        var content = await page.TextContentAsync("#vehicleListContainer");
        Assert.False(string.IsNullOrWhiteSpace(content));
    }

    [SkippableFact]
    public async Task BookingCreate_ShouldReachLoginWhenAnonymousSubmit()
    {
        Skip.If(!Enabled || string.IsNullOrWhiteSpace(BaseUrl), "UI smoke tests are disabled. Set RUN_UI_SMOKE=true and UI_BASE_URL to run.");

        await using var app = await LaunchAsync();
        var page = await app.Context.NewPageAsync();

        await page.GotoAsync($"{BaseUrl}/Customer/Index");
        await page.WaitForSelectorAsync("#vehicleListContainer");

        var bookLinks = await page.QuerySelectorAllAsync("a:has-text('Book now')");
        Skip.If(bookLinks.Count == 0, "No 'Book now' link found in current fixture data.");

        await bookLinks[0].ClickAsync();
        await page.WaitForSelectorAsync("#bookingForm");

        var tomorrow = DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-dd");
        var dayAfter = DateTime.UtcNow.Date.AddDays(2).ToString("yyyy-MM-dd");

        await page.FillAsync("#PickupLocation", "District 1");
        await page.FillAsync("#DropoffLocation", "District 7");
        await page.FillAsync("#PickupDateTime", tomorrow);
        await page.FillAsync("#ReturnDateTime", dayAfter);

        await page.ClickAsync("#bookingForm button[type='submit']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Contains("/Account/Login", page.Url, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task AdminDashboard_ShouldRenderChartsWhenLoggedInAsAdmin()
    {
        Skip.If(!Enabled || string.IsNullOrWhiteSpace(BaseUrl), "UI smoke tests are disabled. Set RUN_UI_SMOKE=true and UI_BASE_URL to run.");

        var adminEmail = Environment.GetEnvironmentVariable("UI_ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("UI_ADMIN_PASSWORD");
        Skip.If(string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword), "Admin credentials are missing. Set UI_ADMIN_EMAIL and UI_ADMIN_PASSWORD.");

        await using var app = await LaunchAsync();
        var page = await app.Context.NewPageAsync();

        await page.GotoAsync($"{BaseUrl}/Account/Login");
        await page.FillAsync("#Email", adminEmail);
        await page.FillAsync("#Password", adminPassword);
        await page.ClickAsync("button[type='submit']");

        await page.GotoAsync($"{BaseUrl}/Admin/Dashboard/Index");
        await page.WaitForSelectorAsync("#revenueChart");
        await page.WaitForSelectorAsync("#categoryChart");

        var chartJsLoaded = await page.EvaluateAsync<bool>("() => typeof Chart !== 'undefined'");
        Assert.True(chartJsLoaded);
    }

    private static async Task<BrowserContextHolder> LaunchAsync()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        return new BrowserContextHolder(playwright, browser, context);
    }

    private sealed class BrowserContextHolder : IAsyncDisposable
    {
        public BrowserContextHolder(IPlaywright playwright, IBrowser browser, IBrowserContext context)
        {
            Playwright = playwright;
            Browser = browser;
            Context = context;
        }

        public IPlaywright Playwright { get; }
        public IBrowser Browser { get; }
        public IBrowserContext Context { get; }

        public async ValueTask DisposeAsync()
        {
            await Context.CloseAsync();
            await Browser.CloseAsync();
            Playwright.Dispose();
        }
    }
}
