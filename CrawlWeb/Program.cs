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
            // Make a new ULTIMATE crawler
            Crawler crawler = new SingleClientCrawler() { ConcurrentThreads = 200 };
            // Credit stuff
            Console.WriteLine("CrawlHVLTB.Net v0.1pre - Author: honguyenminh");
            Console.WriteLine("Distributed under GPLv3");
            Console.WriteLine("GitHub: https://github.com/honguyenminh/CrawlHVLTB.Net");

            // To measure performance
            Stopwatch stopwatch = new();
            stopwatch.Start();

            // Rudimentary args handler, because I don't care
            if (args.Contains("-h") || args.Contains("--help")) { ShowHelp(); return; }
            if (args.Length != 1) { InvalidArgs(args.Length); return; }
            if (!File.Exists(args[0])) { Console.WriteLine("File not found"); return; }

            // Read account list from file
            string[] input = File.ReadAllLines(args[0]);
            List<Account> accounts = new();
            for (int i = 0; i + 1 < input.Length; i += 2)
            {
                accounts.Add(new() { Username = input[i], Password = input[i + 1] });
            }

            // Split input into chunks for multithreading
            var accountChunks = SplitList(accounts, (int)Math.Ceiling((decimal)accounts.Count / crawler.ConcurrentThreads));

            Console.WriteLine("Total " + accounts.Count + " accounts");
            Console.WriteLine("Using " + crawler.ConcurrentThreads + " threads");

            crawler.RunLogin(accountChunks); // Let the show begin!
            
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

        /// <summary>
        /// Split <see cref="List{T}"/> into smaller <see cref="List{T}"/> with the given max size
        /// </summary>
        /// <param name="target">The target list to split</param>
        /// <param name="subListSize">Max number of elements that a sub list can contains</param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> target, int subListSize)
        {
            for (int i = 0; i < target.Count; i += subListSize)
            {
                yield return target.GetRange(i, Math.Min(subListSize, target.Count - i));
            }
        }
    }
}
