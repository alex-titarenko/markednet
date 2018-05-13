using System;
using System.Collections.Generic;

namespace MarkedNet
{
    public class Options
    {
        #region Fields

        private Renderer _renderer;

        #endregion

        #region Properties

        public Func<string, string, string> Highlight { get; set; }

        public Func<string, string> Sanitizer { get; set; }

        public Renderer Renderer
        {
            get { return _renderer; }
            set { _renderer = value; if (_renderer != null) _renderer.Options = this; }
        }

        public string LangPrefix { get; set; }

        public string HeaderPrefix { get; set; }

        public bool XHtml { get; set; }

        public bool Sanitize { get; set; }

        public bool Pedantic { get; set; }

        public bool Mangle { get; set; }

        public bool Smartypants { get; set; }

        public bool Breaks { get; set; }

        public bool Gfm { get; set; }

        public bool Tables { get; set; }

        public bool SmartLists { get; set; }

        public bool ExternalLinks { get; set; }

        public IDictionary<string, string> TableAttributes { get; set; }

        public IDictionary<string, string> ImageAttributes { get; set; }

        public IDictionary <string, string> PreformattedTextAttributes { get; set; }

        #endregion

        #region Constructors

        public Options()
        {
            Highlight = null;
            Sanitizer = null;
            Renderer = new Renderer(this);

            LangPrefix = "lang-";
            HeaderPrefix = "";
            XHtml = false;
            Sanitize = false;
            Pedantic = false;
            Mangle = true;
            Smartypants = false;
            Breaks = false;
            Gfm = true;
            Tables = true;
            SmartLists = false;
            ExternalLinks = true;
        }

        #endregion
    }
}
