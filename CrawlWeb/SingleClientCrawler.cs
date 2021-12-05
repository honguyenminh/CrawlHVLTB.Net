using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlWeb
{
    public class SingleClientCrawler : Crawler
    {
        private readonly HttpClient client;
        private readonly HttpClientHandler handler;
        public SingleClientCrawler()
        {
            handler = new() { UseCookies = false, AllowAutoRedirect = false };
            client = new(handler) { BaseAddress = BaseUri };
        }
        ~SingleClientCrawler()
        {
            client.Dispose();
            handler.Dispose();
        }

        public override void RunLogin(IEnumerable<List<Account>> accountChunks)
        {
            Parallel.ForEach(accountChunks, accountChunk =>
            {
                HtmlDocument document = new();
                foreach (var account in accountChunk)
                {
                    Login(account, document).GetAwaiter().GetResult();
                }
            });
        }
        private async Task Login(Account account, HtmlDocument document)
        {
            // Get session token and cookies first, so make a GET request to /login
            // Make request
            var response = await client.GetAsync("/login");
            // Get cookies
            var cookiesHeaders = response.Headers.GetValues("Set-Cookie");
            var cookies = from header in cookiesHeaders select GetRequestHeader(header);
            // Get token
            var result = await response.Content.ReadAsStringAsync();
            document.LoadHtml(result);
            var tokenField = document.DocumentNode.SelectSingleNode("/html/body/div/div/div/div[2]/div/form/input");
            if (tokenField is null)
                throw new NodeNotFoundException($"Cannot find token input in login page for user " + account.Username);
            string token = tokenField.GetAttributeValue("value", "owo");
            if (token == "owo") throw new NodeAttributeNotFoundException("Cannot find 'value' attribute in token input");

            // Make POST request to /login with previous cookies and token to actually log in
            var message = new HttpRequestMessage(HttpMethod.Post, "/login");
            message.Headers.Add("Cookie", string.Join(";", cookies));
            message.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("_token", token),
                new KeyValuePair<string, string>("username", account.Username),
                new KeyValuePair<string, string>("password", account.Password),
            });
            response = await client.SendAsync(message);
            result = await response.Content.ReadAsStringAsync();
            // Check if credentials are correct
            document.LoadHtml(result);
            string title = document.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
            if (title.Contains("Đăng nhập")) throw new ArgumentException("Wrong credentials at username " + account.Username);
            // Get cookies again
            cookiesHeaders = response.Headers.GetValues("Set-Cookie");
            cookies = from header in cookiesHeaders select GetRequestHeader(header);

            // Make GET request to /ket-qua-thi-sinh to crawl data
            message = new HttpRequestMessage(HttpMethod.Get, "/ket-qua-thi-sinh");
            message.Headers.Add("Cookie", string.Join("; ", cookies));
            response = await client.SendAsync(message);
            // Get cookies again
            cookiesHeaders = response.Headers.GetValues("Set-Cookie");
            cookies = from header in cookiesHeaders select GetRequestHeader(header);
            result = await response.Content.ReadAsStringAsync();
            document.LoadHtml(result);
        }

        // Better regex: ((\w+)=([^;\s]+))(?:.*;.*)*
        private static readonly Regex cookieParseRegex = new(@"((\w+)=([^;\s]+))", RegexOptions.Compiled);
        public static string GetRequestHeader(string responseHeader)
        {
            var match = cookieParseRegex.Match(responseHeader);
            return match.Groups[1].Value;
        }
    }
}
