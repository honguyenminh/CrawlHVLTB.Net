using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlWeb
{
    static class MultipleClient
    {
        public static async Task Login(Account account, HttpClient client)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            HtmlDocument document = new();
            var result = await client.GetStringAsync("/login");
            document.LoadHtml(result);

            var tokenField = document.DocumentNode.SelectSingleNode("/html/body/div/div/div/div[2]/div/form/input");
            if (tokenField is null) throw new NodeNotFoundException($"Thread #{threadId}: Cannot find token input in page for user " + account.Username);
            string token = tokenField.GetAttributeValue("value", "owo");
            if (token == "owo") throw new NodeAttributeNotFoundException("Cannot find 'value' attribute in token input");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("_token", token),
                new KeyValuePair<string, string>("username", account.Username),
                new KeyValuePair<string, string>("password", account.Password),
            });
            await client.PostAsync("/login", content);

            // TODO: add parsing and split this out
            result = await client.GetStringAsync("/ket-qua-thi-sinh");
            // TODO: check wrong password

            _ = await client.GetAsync("/logout");
            // Clear cookies
            //var cookies = cookieContainer.GetCookies(baseUri);
            //foreach (Cookie cookie in cookies) cookie.Expired = true;
        }
    }
}
