using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlWeb
{
    public abstract class Crawler
    {
        public static Uri BaseUri { get; } = new("https://hocvalamtheobac.vn");
        public int ConcurrentThreads { get; set; } = 200;
        public abstract void RunLogin(IEnumerable<List<Account>> accountChunks);
    }
}
