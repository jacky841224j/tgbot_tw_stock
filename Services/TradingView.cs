using Microsoft.Playwright;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace Telegram.Bot.Examples.WebHook.Services
{
    /// <summary>
    /// TradingView
    /// </summary>
    public class TradingView
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TradingView> _logger;
        private readonly BrowserHandlers _browserHandlers;

        public TradingView(ITelegramBotClient botClient, ILogger<TradingView> logger, BrowserHandlers browserHandlers)
        {
            _botClient = botClient;
            _logger = logger;
            _browserHandlers = browserHandlers;
        }

        /// <summary>
        /// 取得圖表
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="chatID">使用者ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetChartAsync(string stockNumber, long chatID, CancellationToken cancellationToken)
        {
            try
            {
                await _browserHandlers.CreateBrowser();
                await _browserHandlers._page.GotoAsync($"https://tradingview.com/chart/?symbol=TWSE%3A{stockNumber}",
                                                        new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 60000 });

                _logger.LogInformation("等待元素載入...");
                //等待元素載入
                await _browserHandlers._page.WaitForSelectorAsync("//div[@class= 'chart-markup-table']");

                _logger.LogInformation("擷取網站中...");

                Stream stream = new MemoryStream(await _browserHandlers._page.Locator("//div[@class= 'chart-markup-table']").ScreenshotAsync());

                await _botClient.SendPhotoAsync(
                   chatId: chatID,
                   photo: InputFile.FromStream(stream),
                   parseMode: ParseMode.Html,
                   cancellationToken: cancellationToken);
                _logger.LogInformation("已傳送資訊");
            }
            catch(Exception ex)
            {
                _logger.LogInformation("GetChartAsync：" + ex.Message);
            }
            finally
            {
                await _browserHandlers.ReleaseBrowser();
            }
        }

        /// <summary>
        /// 取得K線
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="chatID">使用者ID</param>
        /// <param name="input">使用者輸入參數</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetRangeAsync(string stockNumber, long chatID, string? input, CancellationToken cancellationToken)
        {
            try
            {
                await _browserHandlers.CreateBrowser();

                await _browserHandlers._page.GotoAsync($"https://tradingview.com/chart/?symbol=TWSE%3A{stockNumber}",
                                            new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 60000 });

                string range;

                #region
                switch (input)
                {
                    case "1d":
                        range = "1D";
                        break;
                    case "5d":
                        range = "5D";
                        break;
                    case "1m":
                        range = "1M";
                        break;
                    case "3m":
                        range = "3M";
                        break;
                    case "6m":
                        range = "6M";
                        break;
                    case "ytd":
                        range = "YTD";
                        break;
                    case "1y":
                        range = "12M";
                        break;
                    case "5y":
                        range = "60M";
                        break;
                    case "all":
                        range = "ALL";
                        break;
                    default:
                        range = "YTD";
                        break;
                }
                await _browserHandlers._page.Locator($"//button[@value = '{range}']").ClickAsync().WaitAsync(new TimeSpan(0, 1, 0));

                _logger.LogInformation("等待元素載入...");
                //等待元素載入
                await _browserHandlers._page.WaitForSelectorAsync("//div[@class= 'chart-markup-table']");

                _logger.LogInformation("擷取網站中...");
                Stream stream = new MemoryStream(await _browserHandlers._page.Locator("//div[@class= 'chart-markup-table']").ScreenshotAsync());
                await _botClient.SendPhotoAsync(
                   chatId: chatID,
                   photo: InputFile.FromStream(stream),
                   parseMode: ParseMode.Html,
                   cancellationToken: cancellationToken);
                _logger.LogInformation("已傳送資訊");
                #endregion
            }
            catch(Exception ex)
            {
                _logger.LogInformation("GetRangeAsync：" + ex.Message);
            }
            finally
            {
                await _browserHandlers.ReleaseBrowser();
            }
        }
    }
}
