using Microsoft.Playwright;

namespace Telegram.Bot.Examples.WebHook.Services
{
    public class BrowserHandlers
    {
        private readonly ILogger<BrowserHandlers> _logger;
        public IPlaywright? _playwright;
        public IBrowser? _browser;
        public IPage? _page;

        public BrowserHandlers(ILogger<BrowserHandlers> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// 啟動套件
        /// </summary>
        /// <returns></returns>
        public async Task LunchesPlaywright()
        {
            _playwright = await Playwright.CreateAsync();
        }

        /// <summary>
        /// 啟動瀏覽器流程
        /// </summary>
        /// <returns></returns>
        public async Task CreateBrowser()
        {
            await SettingBrowser();
            await SettingPage();
        }

        /// <summary>
        /// 釋放瀏覽器流程
        /// </summary>
        /// <returns></returns>
        public async Task ReleaseBrowser()
        {
            await ClosePage();
            await CloseBrowser();
        }

        /// <summary>
        /// 設定瀏覽器
        /// </summary>
        /// <returns></returns>
        public async Task SettingBrowser()
        {
            try
            {
                _logger.LogInformation($"設定瀏覽器");

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
                _logger.LogInformation($"瀏覽器設定完成");
            }
            catch(Exception ex)
            {
                _logger.LogInformation("SettingBrowser：" + ex.Message);
            }
        }

        /// <summary>
        /// 設定頁面
        /// </summary>
        /// <returns></returns>
        public async Task SettingPage()
        {
            try
            {
                _logger.LogInformation($"設定頁面中");

                //新增頁面
                _page = await _browser.NewPageAsync();
                //設定頁面大小
                await _page.SetViewportSizeAsync(1920, 1080);

                _logger.LogInformation($"設定頁面完成");
            }
            catch(Exception ex)
            {
                _logger.LogInformation("SettingPage：" + ex.Message);
            }
        }

        /// <summary>
        /// 關閉頁面
        /// </summary>
        /// <returns></returns>
        public async Task ClosePage()
        {
            await _page.CloseAsync();
        }

        /// <summary>
        /// 關閉瀏覽器
        /// </summary>
        /// <returns></returns>
        public async Task CloseBrowser()
        {
            await _browser.CloseAsync();
        }


    }
}
