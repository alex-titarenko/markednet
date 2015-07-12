using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace MarkedNet
{
    public class Renderer
    {
        #region Fields

        private Options options;

        #endregion

        #region Constructors

        public Renderer(Options options)
        {
            this.options = options ?? new Options();
        }

        #endregion

        #region Methods

        #region Block Level Renderer

        public virtual string code(string code, string lang, bool escaped)
        {
            if (this.options.highlight != null)
            {
                var @out = this.options.highlight(code, lang);
                if (@out != null && @out != code)
                {
                    escaped = true;
                    code = @out;
                }
            }

            if (String.IsNullOrEmpty(lang))
            {
                return "<pre><code>" + (escaped ? code : StringHelper.escape(code, true)) + "\n</code></pre>";
            }

            return "<pre><code class=\""
                + this.options.langPrefix
                + StringHelper.escape(lang, true)
                + "\">"
                + (escaped ? code : StringHelper.escape(code, true))
                + "\n</code></pre>\n";
        }

        public virtual string blockquote(string quote)
        {
            return "<blockquote>\n" + quote + "</blockquote>\n";
        }

        public virtual string html(string html)
        {
            return html;
        }

        public virtual string heading(string text, int level, string raw)
        {
            return "<h"
                + level
                + " id=\""
                + this.options.headerPrefix
                + Regex.Replace(raw.ToLower(), @"[^\w]+", "-")
                + "\">"
                + text
                + "</h"
                + level
                + ">\n";
        }

        public virtual string hr()
        {
            return this.options.xhtml ? "<hr/>\n" : "<hr>\n";
        }

        public virtual string list(string body, bool ordered)
        {
            var type = ordered ? "ol" : "ul";
            return "<" + type + ">\n" + body + "</" + type + ">\n";
        }

        public virtual string listitem(string text)
        {
            return "<li>" + text + "</li>\n";
        }

        public virtual string paragraph(string text)
        {
            return "<p>" + text + "</p>\n";
        }

        public virtual string table(string header, string body)
        {
            return "<table>\n"
                + "<thead>\n"
                + header
                + "</thead>\n"
                + "<tbody>\n"
                + body
                + "</tbody>\n"
                + "</table>\n";
        }

        public virtual string tablerow(string content)
        {
            return "<tr>\n" + content + "</tr>\n";
        }

        public virtual string tablecell(string content, TableCellFlags flags)
        {
            var type = flags.header ? "th" : "td";
            var tag = !String.IsNullOrEmpty(flags.align)
                ? "<" + type + " style=\"text-align:" + flags.align + "\">"
                : "<" + type + ">";

            return tag + content + "</" + type + ">\n";
        }

        #endregion

        #region Span Level Renderer

        public virtual string strong(string text)
        {
            return "<strong>" + text + "</strong>";
        }

        public virtual string em(string text)
        {
            return "<em>" + text + "</em>";
        }

        public virtual string codespan(string text)
        {
            return "<code>" + text + "</code>";
        }

        public virtual string br()
        {
            return this.options.xhtml ? "<br/>" : "<br>";
        }

        public virtual string del(string text)
        {
            return "<del>" + text + "</del>";
        }

        public virtual string link(string href, string title, string text)
        {
            if (this.options.sanitize)
            {
                string prot = null;
                
                try
                {
                    prot = Regex.Replace(StringHelper.decodeURIComponent(StringHelper.unescape(href)), @"[^\w:]", String.Empty).ToLower();
                }
                catch (Exception)
                {
                    return String.Empty;
                }

                if (prot.IndexOf("javascript:") == 0 || prot.IndexOf("vbscript:") == 0)
                {
                    return String.Empty;
                }
            }

            var @out = "<a href=\"" + href + "\"";
            if (!String.IsNullOrEmpty(title))
            {
                @out += " title=\"" + title + "\"";
            }

            @out += ">" + text + "</a>";
            return @out;
        }

        public virtual string image(string href, string title, string text)
        {
            var @out = "<img src=\"" + href + "\" alt=\"" + text + "\"";
            if (!String.IsNullOrEmpty(title))
            {
                @out += " title=\"" + title + "\"";
            }

            @out += this.options.xhtml ? "/>" : ">";
            return @out;
        }

        public virtual string text(string text)
        {
          return text;
        }
    
        #endregion

        #endregion
    }
}
