using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Threading;
using System.Linq;

namespace CrawlWeb
{
    class Program
    {
        static readonly Uri baseUri = new("https://hocvalamtheobac.vn");

        static async Task Main(string[] args)
        {
            string[] input = await File.ReadAllLinesAsync("accounts.txt");
            // TODO: add account splitter
            List<Account> accounts = new();
            for (int i = 0; i + 1 < input.Length; i+=2)
            {
                accounts.Add(new() { Username = input[i], Password = input[i + 1] });
            }

            var tasks = accounts.Select(async account =>
            {
                CookieContainer cookieContainer = new();
                using HttpClientHandler handler = new() { CookieContainer = cookieContainer };
                using HttpClient client = new(handler) { BaseAddress = baseUri };
                await MultipleClient.Login(account.Username, account.Password, client);
            });
            await Task.WhenAll(tasks);
        }
    }
}
