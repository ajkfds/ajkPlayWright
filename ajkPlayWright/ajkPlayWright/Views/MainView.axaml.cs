using Avalonia.Controls;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace ajkPlayWright.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        TestButton.Click += TestButton_Click;
    }

    private async void TestButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //        await browserTest(false);
        await testStealth();
    }

    private System.Random rand = new Random();


    private async Task testStealth()
    {
        using var playwright = await Playwright.CreateAsync();

        // Edge (Chromium) を起動
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = "msedge",
            Headless = false,
            Args = new[]
            {
                "--disable-blink-features=AutomationControlled",
                "--no-first-run",
                "--no-default-browser-check"
            }
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                        "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0",
            Locale = "ja-JP",
            TimezoneId = "Asia/Tokyo",
            ViewportSize = new ViewportSize { Width = 1366, Height = 768 }
        });

        var page = await context.NewPageAsync();

        //// 最後まで待つ or スクリーンショットを取る
        //await page.WaitForTimeoutAsync(3000);
        //await page.ScreenshotAsync(new PageScreenshotOptions { Path = "sannysoft.png", FullPage = true });
        await page.GotoAsync("https://www.google.com/");
        await Task.Delay(rand.Next(100, 200));

        await page.FillAsync("#APjFqb", "Playwright");
        await page.Keyboard.PressAsync("Enter");

        await Task.Delay(rand.Next(100, 200));
    }
    private async Task browserTest(bool headless)
    {
        var playwright = await Playwright.CreateAsync();
        //var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        //{
        //    ExecutablePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        //    Headless = headless
        //});

        //var context = await browser.NewContextAsync();
        //var page = await context.NewPageAsync();

        // Edgeを起動（Chromiumベース）
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = "msedge", // Edge指定
            Headless = false    // ステルス検証のため表示
        });

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // CDPセッションを開始
        var cdpSession = await context.NewCDPSessionAsync(page);

        // navigator.webdriver を undefined に偽装
        //await context.AddInitScriptAsync(@"Object.defineProperty(navigator, 'webdriver', {
        //    get: () => undefined
        //});");
        await context.AddInitScriptAsync(@"
// navigator.webdriver
Object.defineProperty(navigator, 'webdriver', {
    get: () => undefined
});

// navigator.plugins
Object.defineProperty(navigator, 'plugins', {
    get: () => [1, 2, 3] // 実在するプラグイン数を模倣
});

// navigator.languages
Object.defineProperty(navigator, 'languages', {
    get: () => ['ja-JP', 'en-US'] // 日本語＋英語の自然な構成
});

// WebGLRenderingContext偽装
const getParameter = WebGLRenderingContext.prototype.getParameter;
WebGLRenderingContext.prototype.getParameter = function(parameter) {
    // GPUベンダーとレンダラーを偽装
    if (parameter === 37445) return 'Intel Inc.';       // UNMASKED_VENDOR_WEBGL
    if (parameter === 37446) return 'Intel Iris OpenGL'; // UNMASKED_RENDERER_WEBGL
    return getParameter(parameter);
};

// AudioContext偽装（fingerprintノイズ除去）
const original = AudioBuffer.prototype.getChannelData;
AudioBuffer.prototype.getChannelData = function() {
    const data = original.apply(this, arguments);
    for (let i = 0; i < data.length; i += 100) {
        data[i] = data[i] + Math.random() * 0.00001; // 微小ノイズを除去
    }
    return data;
};
");
        // ページを開いて検証
        await page.GotoAsync("https://bot.sannysoft.com/");

        // navigator.webdriver の値を確認
        var result = await page.EvaluateAsync<string>("navigator.webdriver === undefined ? 'undefined' : navigator.webdriver.ToString()");
        Console.WriteLine($"navigator.webdriver: {result}");

        //        var page = await browser.NewPageAsync();


        //            # 自動化インジケーターを無効にするJavaScriptを注入
        //await page.AddInitScriptAsync("""
        //    Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
        //    Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3, 4, 5] }); // 一般的なプラグイン数を模倣
        //    Object.defineProperty(navigator, 'languages', { get: () => ['ja-JP', 'ja'] });
        //    Object.defineProperty(navigator, 'deviceMemory', { get: () => 8 }); // 一般的なデバイスメモリを模倣
        //""");

    //    await Task.Delay(100);
    //    await page.EvaluateAsync(@"() => {
    //Object.defineProperty(navigator, 'webdriver', {
    //    get: () => false
    //});
    //}");
    //    await page.GotoAsync("https://bot.sannysoft.com/");
    }
    private async Task copilotTest(bool headless)
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            ExecutablePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            Headless = headless
        });
        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://copilot.microsoft.com/");
        await Task.Delay(rand.Next(1000, 2000));

        await page.Mouse.MoveAsync(rand.Next(100, 300), rand.Next(100, 300));
        await Task.Delay(rand.Next(500, 1000));
        await page.Mouse.MoveAsync(rand.Next(400, 600), rand.Next(200, 400));
        await Task.Delay(rand.Next(500, 1000));

        await typeTextAsync(page, "#userInput", "hello");
        //        await page.FillAsync("#userInput", "こんにちは");
        await Task.Delay(rand.Next(500, 1000));
        await page.Keyboard.PressAsync("Enter");
    }
    private async Task searchDuckDuckGo(bool headless)
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            ExecutablePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            Headless = headless
        });
        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://duckduckgo.com/");
        await page.FillAsync("#searchbox_input", "Playwright");
        await page.Keyboard.PressAsync("Enter");
    }
    private async Task googleTest(bool headless)
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {
            ExecutablePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            Headless = headless
        });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36"
        });

        var page = await context.NewPageAsync();
        await page.EvaluateAsync(@"() => {
            Object.defineProperty(navigator, 'webdriver', { get: () => false });
            Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3] });
            Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'ja'] });
        }");


        await page.GotoAsync("https://www.google.com/");
        await Task.Delay(rand.Next(1000, 2000));

        await page.Mouse.MoveAsync(rand.Next(100, 300), rand.Next(100, 300));
        await Task.Delay(rand.Next(500, 1000));
        await page.Mouse.MoveAsync(rand.Next(400, 600), rand.Next(200, 400));
        await Task.Delay(rand.Next(500, 1000));

        //        await page.se .SetContentAsync("#APjFqb");
        //        await Task.Delay(rand.Next(100, 200));
        await typeTextAsync(page, "#APjFqb","Playwright");



        //        await page.FillAsync("#APjFqb", "Playwright");
        await Task.Delay(rand.Next(100, 200));
        await page.Keyboard.PressAsync("Enter");
        //        await page.GotoAsync("https://login.microsoftonline.com");
        //        await page.FillAsync("#i0116", "your-email@example.com");
        //        await page.ClickAsync("#idSIButton9");
    }

    private async Task typeTextAsync(IPage page,string selector, string text)
    {
        await page.Locator(selector).ClearAsync();

        for(int i=0; i<text.Length; i++)
        {
            await page.Keyboard.DownAsync(text.Substring(i, 1));
            await Task.Delay(rand.Next(10, 20));
            await page.Keyboard.UpAsync(text.Substring(i, 1));
//            await page.Keyboard.TypeAsync(text.Substring(i, 1));
            await Task.Delay(rand.Next(50, 200));
        }
    }



    // AvaloniaのViewModelなどから呼び出す
    public async Task LaunchBrowserAsync()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://example.com");

        // Avalonia側でステータス表示など
//        StatusMessage = "ブラウザ起動完了";
    }

}
