using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkedNet
{
    public static class StringHelper
    {
        public static string decodeURIComponent(string str)
        {
            return Uri.UnescapeDataString(str);
        }

        public static string escape(string html, bool encode)
        {
            return Regex.Replace(html, !encode ? @"&(?!#?\w+;)" : @"&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        public static string unescape(string html)
        {
            return Regex.Replace(html, @"&([#\w]+);", (Match match) =>
            {
                var n = match.Value;

                n = n.ToLower();
                if (n == "colon") return ":";
                if (n[0] == '#')
                {
                    return n[1] == 'x'
                        ? ((char)Convert.ToInt32(n.Substring(2), 16)).ToString()
                        : ((char)Convert.ToInt32(n.Substring(1))).ToString();
                }
                return String.Empty;
            });
        }
    }
}
