using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkedNet
{
    public class Options
    {
        public Func<string, string, string> highlight { get; set; }

        public Func<string, string> sanitizer { get; set; }

        public Renderer renderer { get; set; }

        public string langPrefix { get; set; }

        public string headerPrefix { get; set; }

        public bool xhtml { get; set; }

        public bool sanitize { get; set; }

        public bool pedantic { get; set; }

        public bool mangle { get; set; }

        public bool smartypants { get; set; }

        public bool breaks { get; set; }

        public bool gfm { get; set; }

        public bool tables { get; set; }


        public Options()
        {
            highlight = null;
            sanitizer = null;
            renderer = new Renderer(this);

            langPrefix = "lang-";
            headerPrefix = "";
            xhtml = false;
            sanitize = false;
            pedantic = false;
            mangle = true;
            smartypants = false;
            breaks = false;
            gfm = true;
            tables = true;
        }
    }
}
