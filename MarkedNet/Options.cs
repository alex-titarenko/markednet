using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkedNet
{
    public class Options
    {
        public Func<string, string, string> highlight { get; set; }

        public string langPrefix { get; set; }

        public string headerPrefix { get; set; }

        public bool xhtml { get; set; }

        public bool sanitize { get; set; }


        public Options()
        {
            langPrefix = "lang-";
            headerPrefix = "";
            xhtml = false;
            sanitize = false;
        }
    }
}
