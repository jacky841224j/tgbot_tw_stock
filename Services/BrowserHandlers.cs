using Microsoft.Playwright;

namespace Telegram.Bot.Examples.WebHook.Services
{
    public class BrowserHandlers
    {
        private readonly ILogger<BrowserHandlers> _logger;
        private IPlaywright? _playwright;
        public IBrowser? _browser;
        public IPage? _page;

        public BrowserHandlers(ILogger<BrowserHandlers> logger)
        {
            _logger = logger;
        }

        public async Task CreateBrowserAsync()
        {
            #region 建立瀏覽器
            _logger.LogInformation($"設定瀏覽器");
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                //路徑會依瀏覽器版本不同有差異，若有錯時請修正路徑
                //使用docker執行時須使用下面參數，本機直接執行則不用
                // ExecutablePath = "/root/.cache/ms-playwright/chromium-1055/chrome-linux/chrome",
                Args = new[] {
                    "--disable-dev-shm-usage",
                    "--disable-setuid-sandbox",
                    "--no-sandbox",
                    "--disable-gpu"
                },
                Headless = true,
                Timeout = 0,
            });
            _page = await _browser.NewPageAsync();
            await _page.SetViewportSizeAsync(1920, 1080);
            _logger.LogInformation($"瀏覽器設定完成");
            #endregion
        }
    }
}
