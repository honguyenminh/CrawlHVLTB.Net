using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CrawlWeb
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Crawler crawler = new SingleClientCrawler();
            Console.WriteLine("CrawlHVLTB.Net v0.1pre - Author: honguyenminh");
            Console.WriteLine("Distributed under GPLv3");
            Console.WriteLine("GitHub: https://github.com/honguyenminh/CrawlHVLTB.Net");

            Stopwatch stopwatch = new();
            stopwatch.Start();
            if (args.Contains("-h") || args.Contains("--help")) { ShowHelp(); return; }
            if (args.Length != 1) { InvalidArgs(args.Length); return; }
            if (!File.Exists(args[0])) { Console.WriteLine("File not found"); return; }

            string[] input = File.ReadAllLines(args[0]);
            List<Account> accounts = new();
            for (int i = 0; i + 1 < input.Length; i += 2)
            {
                accounts.Add(new() { Username = input[i], Password = input[i + 1] });
            }
            var accountChunks = SplitList(accounts, (int)Math.Ceiling((decimal)accounts.Count / crawler.ConcurrentThreads));

            Console.WriteLine("Total " + accounts.Count + " accounts");
            Console.WriteLine("Using " + crawler.ConcurrentThreads + " threads");

            crawler.RunLogin(accountChunks);
            stopwatch.Stop();
            Console.WriteLine("Exec time: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        private static void InvalidArgs(int argsCount)
        {
            Console.WriteLine("Invalid arguments. Found " + argsCount + " arguments.");
            Console.WriteLine("Try --help or -h for help");
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Syntax: <exec path> <account list file path>" + Environment.NewLine);
            Console.WriteLine("<exec path>: path to this executable, but you already knew this eh?");
            Console.WriteLine("<account list file path>: path to account list file.");
            Console.WriteLine("Account list file contains list of account and its password, each on their own line");
            Console.WriteLine("Example:");
            Console.WriteLine("username1");
            Console.WriteLine("password1");
            Console.WriteLine("username2");
            Console.WriteLine("password2");
            Console.WriteLine("...");
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> original, int subListSize = 30)
        {
            for (int i = 0; i < original.Count; i += subListSize)
            {
                yield return original.GetRange(i, Math.Min(subListSize, original.Count - i));
            }
        }
    }
}
