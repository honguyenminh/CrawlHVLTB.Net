using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace CrawlWeb
{
    partial class Program
    {
        static readonly Uri baseUri = new("https://hocvalamtheobac.vn");
        private const int concurrentThreads = 1000;

        static void Main(string[] args)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            if (args.Contains("-h") || args.Contains("--help")) { ShowHelp(); return; }
            if (args.Length != 1) { InvalidArgs(); return; }
            if (!File.Exists(args[0])) { Console.WriteLine("File not found"); return; }

            string[] input = File.ReadAllLines(args[0]);
            List<Account> accounts = new();
            for (int i = 0; i + 1 < input.Length; i+=2)
            {
                accounts.Add(new() { Username = input[i], Password = input[i + 1] });
            }
            var accountChunks = SplitList(accounts, (int)Math.Ceiling((decimal)accounts.Count / concurrentThreads));

            Console.WriteLine("Total " + accounts.Count + " accounts");
            Parallel.ForEach(accountChunks, accountChunk =>
            {
                CookieContainer cookieContainer = new();
                using HttpClientHandler handler = new() { CookieContainer = cookieContainer };
                using HttpClient client = new(handler) { BaseAddress = baseUri };
                foreach (var account in accountChunk)
                {
                    MultipleClient.Login(account, client).GetAwaiter().GetResult();
                }
            });
            stopwatch.Stop();
            Console.WriteLine("Exec time: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        private static void InvalidArgs()
        {
            throw new NotImplementedException();
        }

        private static void ShowHelp()
        {
            throw new NotImplementedException();
        }

        //public static List<List<T>> SplitList<T>(List<T> original, int subListSize)
        //{
        //    var list = new List<List<T>>();

        //    for (int i = 0; i < original.Count; i += subListSize)
        //    {
        //        list.Add(original.GetRange(i, Math.Min(subListSize, original.Count - i)));
        //    }

        //    return list;
        //}
        public static IEnumerable<List<T>> SplitList<T>(List<T> original, int subListSize = 30)
        {
            for (int i = 0; i < original.Count; i += subListSize)
            {
                yield return original.GetRange(i, Math.Min(subListSize, original.Count - i));
            }
        }
    }
}
