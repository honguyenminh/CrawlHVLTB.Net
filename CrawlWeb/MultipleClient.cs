using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CrawlWeb
{
    static class MultipleClient
    {
        public static async Task Login(string username, string password, HttpClient client)
        {
            HtmlDocument document = new();
            var result = await client.GetStringAsync("/login");
            await File.WriteAllTextAsync(username + "-login.html", result);
            document.LoadHtml(result);

            var tokenField = document.DocumentNode.SelectSingleNode("/html/body/div/div/div/div[2]/div/form/input");
            if (tokenField is null) throw new NodeNotFoundException("Cannot find token input in page");
            string token = tokenField.GetAttributeValue("value", "owo");
            if (token == "owo") throw new NodeAttributeNotFoundException("Cannot find 'value' attribute in token input");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("_token", token),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
            });
            var response = await client.PostAsync("/login", content);
            try { result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync(); }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.Found) Console.WriteLine("Wrong password b*tch");
                return;
            }
            await File.WriteAllTextAsync(username + "-after-login.html", result);

            result = await client.GetStringAsync("/ket-qua-thi-sinh");
            await File.WriteAllTextAsync(username + "-ketqua.html", result);

            result = await client.GetStringAsync("/logout");
            await File.WriteAllTextAsync(username + "-logout.html", result);

            result = await client.GetStringAsync("/login");
            await File.WriteAllTextAsync(username + "-login2.html", result);
            Console.WriteLine("Done user " + username);
            // Clear cookies
            //var cookies = cookieContainer.GetCookies(baseUri);
            //foreach (Cookie cookie in cookies) cookie.Expired = true;
        }
    }
}
