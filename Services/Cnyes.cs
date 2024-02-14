using Microsoft.Playwright;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Examples.WebHook.Services
{
    /// <summary>
    /// 鉅亨網
    /// </summary>
    public class Cnyes
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<Cnyes> _logger;
        private readonly BrowserHandlers _browserHandlers;

        public Cnyes(ITelegramBotClient botClient, ILogger<Cnyes> logger, BrowserHandlers browserHandlers)
        {
            _botClient = botClient;
            _logger = logger;
            _browserHandlers = browserHandlers;
        }

        /// <summary>
        /// 取得K線
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="chatID">使用者ID</param>
        /// <param name="input">使用者輸入參數</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetKlineAsync(int stockNumber, long chatID, string? input, CancellationToken cancellationToken)
        {
            await _browserHandlers._page.GotoAsync($"https://www.cnyes.com/twstock/{stockNumber}",
                        new PageGotoOptions { WaitUntil = WaitUntilState.Load }).WaitAsync(new TimeSpan(0, 1, 0));

            _logger.LogInformation("等待元素載入...");

            //等待圖表載入
            await _browserHandlers._page.WaitForSelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[2]//div//div[2]//div[1]//div//div//div//div[2]//table").WaitAsync(new TimeSpan(0, 1, 0));

            //拆解元素
            var element = await _browserHandlers._page.QuerySelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//h2");
            var textContent = await element.EvaluateAsync<string>("node => node.innerText");

            //股票名稱
            var stockName = textContent.Split("\n").ToList()[0];

            await _browserHandlers._page.GetByRole(AriaRole.Button, new() { Name = input, Exact = true, }).ClickAsync();
            await _browserHandlers._page.WaitForTimeoutAsync(1500);

            //圖表
            _logger.LogInformation("擷取網站中...");
            Stream stream = new MemoryStream(await _browserHandlers._page.Locator("//div[@class= 'jsx-3625047685 tradingview-chart']").ScreenshotAsync());
            await _botClient.SendPhotoAsync(
                caption: $"{stockName}：{input}線圖　💹",
                chatId: chatID,
                photo: InputFile.FromStream(stream),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
            _logger.LogInformation("已傳送資訊");

        }

        /// <summary>
        /// 取得詳細報價
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="chatID">使用者ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetDetialPriceAsync(int stockNumber, long chatID, CancellationToken cancellationToken)
        {
            //股價資訊
            var InfoDic = new Dictionary<int, string>()
            {
                { 1, "開盤價"},{ 2, "最高價"},{ 3, "成交量"},
                { 4, "昨日收盤價"},{ 5, "最低價"},{ 6, "成交額"},
                { 7, "均價"},{ 8, "本益比"},{ 9, "市值"},
                { 10, "振幅"},{ 11, "迴轉率"},{ 12, "發行股"},
                { 13, "漲停"},{ 14, "52W高"},{ 15, "內盤量"},
                { 16, "跌停"},{ 17, "52W低"},{ 18, "外盤量"},
                { 19, "近四季EPS"},{ 20, "當季EPS"},{ 21, "毛利率"},
                { 22, "每股淨值"},{ 23, "本淨比"},{ 24, "營利率"},
                { 25, "年股利"},{ 26, "殖利率"},{ 27, "淨利率"},
            };

            await _browserHandlers._page.GotoAsync($"https://www.cnyes.com/twstock/{stockNumber}",
                            new PageGotoOptions { WaitUntil = WaitUntilState.Load }).WaitAsync(new TimeSpan(0, 1, 0));

            _logger.LogInformation("等待元素載入...");
            //await _browserHandlers._page.GetByRole(AriaRole.Button, new() { Name = "日K" }).ClickAsync();

            //等待圖表載入
            await _browserHandlers._page.WaitForSelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[2]//div//div[2]//div[1]//div//div//div//div[2]//table").WaitAsync(new TimeSpan(0, 1, 0));
            await _browserHandlers._page.WaitForTimeoutAsync(1500);

            _logger.LogInformation("處理相關資料...");
            //拆解元素
            var element = await _browserHandlers._page.QuerySelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//h2");
            var textContent = await element.EvaluateAsync<string>("node => node.innerText");

            //股票名稱
            var stockName = textContent.Split("\n").ToList()[0];

            //詳細報價
            var temp_returnStockUD = await _browserHandlers._page.QuerySelectorAllAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[3]//div[2]");
            var returnStockUD = await temp_returnStockUD[0].InnerTextAsync();
            var StockUD_List = returnStockUD.Split("\n");

            //股價
            var stock_price = await _browserHandlers._page.TextContentAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//div//h3");
            //漲跌幅
            var stock_change_price = await _browserHandlers._page.TextContentAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//div//div//div[1]//span[1]");
            //漲跌%
            var stock_amplitude = await _browserHandlers._page.TextContentAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//div//div//div[1]//span[2]");

            //選擇輸出欄位
            var output = new int[] { 1, 2, 5 };

            StringBuilder chart = new StringBuilder();
            int line = 0;

            chart.Append(@$"<b>-{stockName}-📝</b>");
            chart.AppendLine();
            chart.Append(@$"<code>收盤價：{stock_price}</code>");
            chart.AppendLine();
            chart.Append(@$"<code>漲跌幅：{stock_change_price}</code>");
            chart.AppendLine();
            chart.Append(@$"<code>漲跌%：{stock_amplitude}</code>");
            chart.AppendLine();

            foreach (var i in output)
            {
                line++;
                chart.Append(@$"<code>{InfoDic[i]}：{StockUD_List[i * 2 - 1]}</code>");
                chart.AppendLine();
            }

            //圖表
            _logger.LogInformation("擷取網站中...");
            Stream stream = new MemoryStream(
                await _browserHandlers._page.Locator("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]").First.ScreenshotAsync());
             await _botClient.SendPhotoAsync(
                caption: chart.ToString(),
                chatId: chatID,
                photo: InputFile.FromStream(stream),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
            _logger.LogInformation("已傳送資訊");
        }

        /// <summary>
        /// 取得績效
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="chatID">使用者ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetPerformanceAsync(int stockNumber, long chatID, CancellationToken cancellationToken)
        {
            await _browserHandlers._page.GotoAsync($"https://www.cnyes.com/twstock/{stockNumber}",
                           new PageGotoOptions { WaitUntil = WaitUntilState.Load }).WaitAsync(new TimeSpan(0, 1, 0));

            _logger.LogInformation("等待元素載入...");

            //等待圖表載入
            await _browserHandlers._page.WaitForSelectorAsync("//html//body//div[1]//div[1]//div[4]//div[3]//section//div[2]//section//div[2]//div[1]//div//div[2]//div").WaitAsync(new TimeSpan(0, 1, 0));

            //等待數據載入
            //await _browserHandlers._page.WaitForSelectorAsync("//html//body//div[1]//div[1]//div[4]//div[3]//section//div[2]//section//div[2]//div[2]//div//div//table").WaitAsync(new TimeSpan(0, 1, 0));

            await _browserHandlers._page.WaitForTimeoutAsync(1500);

            //拆解元素
            var element = await _browserHandlers._page.QuerySelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//h2");
            var textContent = await element.EvaluateAsync<string>("node => node.innerText");

            //股票名稱
            var stockName = textContent.Split("\n").ToList()[0];

            //股價
            var price = await _browserHandlers._page.Locator("//*[@id=\"tw-stock-tabs\"]//div[2]//section").First.ScreenshotAsync();

            _logger.LogInformation("擷取網站中...");
            var stream = new MemoryStream(price);
            await _botClient.SendPhotoAsync(
                caption: $"{stockName} 績效表現　✨",
                chatId: chatID,
                photo: InputFile.FromStream(stream),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
            _logger.LogInformation("已傳送資訊");
        }
       
        /// <summary>
        /// 取得新聞
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="chatID">使用者ID</param>
        /// <param name="input">使用者輸入參數</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetNewsAsync(int stockNumber, long chatID, CancellationToken cancellationToken)
        {
            await _browserHandlers._page.GotoAsync($"https://www.cnyes.com/twstock/{stockNumber}",
                           new PageGotoOptions { WaitUntil = WaitUntilState.Load }).WaitAsync(new TimeSpan(0, 1, 0));
            _logger.LogInformation("定位網站中...");

            //拆解元素
            var element = await _browserHandlers._page.QuerySelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//h2");
            var textContent = await element.EvaluateAsync<string>("node => node.innerText");

            //股票名稱
            var stockName = textContent.Split("\n").ToList()[0];
            //定位新聞版塊
            var newsList = await _browserHandlers._page.QuerySelectorAsync("//div[contains(@class, 'news-notice-container-summary')]");
            var newsContent = await newsList.QuerySelectorAllAsync("//a[contains(@class, 'container shadow')]");

            var InlineList = new List<IEnumerable<InlineKeyboardButton>>();
            for (int i = 0; i < 5; i++)
            {
                InlineList.Add(new[] { InlineKeyboardButton.WithUrl(await newsContent[i].TextContentAsync(), await newsContent[i].GetAttributeAsync("href")) });
            }

            InlineKeyboardMarkup inlineKeyboard = new(InlineList);
            var s = inlineKeyboard.InlineKeyboard;
            await _botClient.SendTextMessageAsync(
                chatId: chatID,
                text: @$"⚡️{stockName}-即時新聞",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
            _logger.LogInformation("已傳送資訊");
        }
    }
}
