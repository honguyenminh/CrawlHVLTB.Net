using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Threading.Thread;

namespace CrawlWeb
{
    /// <summary>
    /// A crawler that uses only one client for all operations
    /// </summary>
    public class SingleClientCrawler : Crawler, IDisposable
    {
        private readonly HttpClient _client;
        private readonly HttpClientHandler _handler;
        public SingleClientCrawler()
        {
            _handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false };
            _client = new HttpClient(_handler) { BaseAddress = BaseUri };
        }

        public override void RunLogin(IEnumerable<List<Account>> accountChunks)
        {
            Parallel.ForEach(accountChunks, accountChunk =>
            {
                HtmlDocument document = new();
                foreach (var account in accountChunk)
                {
                    try
                    {
                        Login(account, document).GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        int threadId = CurrentThread.ManagedThreadId;
                        Console.WriteLine($"[ERROR] Thread #{threadId}: {e.Message}");
                    }
                }
            });
        }
        private async Task Login(Account account, HtmlDocument document)
        {
            // Get session token and cookies first, so make a GET request to /login
            // Make request
            var response = await _client.GetAsync("/login");
            // Get cookies
            var cookiesHeaders = response.Headers.GetValues("Set-Cookie");
            var cookies = from header in cookiesHeaders select Helper.GetRequestHeader(header);
            // Get token
            var result = await response.Content.ReadAsStringAsync();
            document.LoadHtml(result);
            var tokenField = document.DocumentNode.SelectSingleNode("/html/body/div/div/div/div[2]/div/form/input");
            if (tokenField is null)
                throw new NodeNotFoundException($"Cannot find token input in login page for user " + account.Username);
            string token = tokenField.GetAttributeValue("value", "owo");
            if (token == "owo") throw new NodeAttributeNotFoundException("Cannot find 'value' attribute in token input at username " + account.Username);

            // Make POST request to /login with previous cookies and token to actually log in
            var message = new HttpRequestMessage(HttpMethod.Post, "/login");
            message.Headers.Add("Cookie", string.Join(";", cookies));
            message.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("_token", token),
                new KeyValuePair<string, string>("username", account.Username),
                new KeyValuePair<string, string>("password", account.Password),
            });
            response = await _client.SendAsync(message);
            result = await response.Content.ReadAsStringAsync();
            // Check if credentials are correct
            if (result.Contains("http://hocvalamtheobac.vn/login"))
                throw new ArgumentException("Wrong credentials at username " + account.Username);
            // Get cookies again
            cookiesHeaders = response.Headers.GetValues("Set-Cookie");
            cookies = from header in cookiesHeaders select Helper.GetRequestHeader(header);

            // Make GET request to /ket-qua-thi-sinh to crawl data
            message = new HttpRequestMessage(HttpMethod.Get, "/ket-qua-thi-sinh");
            message.Headers.Add("Cookie", string.Join("; ", cookies));
            response = await _client.SendAsync(message);

            // Parse the HTML
            result = await response.Content.ReadAsStringAsync();
            document.LoadHtml(result);

            // Get full name of account
            string fullName = document.DocumentNode.SelectSingleNode("/html/body/div[1]/main/section[1]/div/div/div/div[1]/p/strong")?.InnerText ?? throw new InvalidOperationException();
            if (fullName is null) throw new InvalidOperationException("New account, no info found at username " + account.Username);
            fullName = Helper.ToTitleCase(fullName.Trim());

            // Get body
            var rows = document.DocumentNode.SelectNodes("//tbody/tr");
            Dictionary<string, ScoreInfo> bestScoreOfWeek = new();
            foreach (var row in rows)
            {
                // TODO: decide what to do with non-qualification rounds
                var cells = row.ChildNodes;
                string week = cells[2].InnerText;
                int score = int.Parse(cells[4].InnerText);
                TimeSpan time = Helper.SiteTimeToTimeSpan(cells[5].InnerText);
            }
        }

        public void Dispose()
        {
            _client.Dispose();
            _handler.Dispose();
            GC.SuppressFinalize(this);
        }

        ~SingleClientCrawler()
        {
            Dispose();
        }
    }
}
