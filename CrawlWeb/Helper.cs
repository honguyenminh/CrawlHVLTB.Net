using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrawlWeb
{
    public static class Helper
    {
        // Better regex: ((\w+)=([^;\s]+))(?:.*;.*)* if options are needed
        private static readonly Regex cookieParser = new(@"((\w+)=([^;\s]+))", RegexOptions.Compiled);
        public static string GetRequestHeader(string responseHeader)
        {
            var match = cookieParser.Match(responseHeader);
            return match.Groups[1].Value;
        }
        public static string ToTitleCase(string str) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        private static readonly Regex minuteToMsTimeParser = new(@"^(\d+):((?:[0-5]?\d)|(?:60)).(\d{3})$");
        public static TimeSpan SiteTimeToTimeSpan(string siteTime)
        {
            var match = minuteToMsTimeParser.Match(siteTime);
            if (!match.Success) throw new ArgumentException("Invalid time/format");

            TimeSpan timeSpan = new(0, 0, int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
            return timeSpan;
        }
    }
}
