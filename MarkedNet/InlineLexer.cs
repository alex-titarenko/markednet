using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace MarkedNet
{
    /// <summary>
    /// Inline Lexer & Compiler
    /// </summary>
    public class InlineLexer
    {
        private Random random = new Random();

        private Options options;
        private Renderer renderer;
        private IDictionary<string, LinkObj> links;
        private InlineRules rules;


        public InlineLexer(IDictionary<string, LinkObj> links, Options options)
        {
            this.options = options ?? new Options();
            this.renderer = options.Renderer ?? new Renderer(options);

            this.links = links;
            this.rules = new NormalInlineRules();

            if (this.links == null)
            {
                throw new Exception("Tokens array requires a `links` property.");
            }

            if (this.options.Gfm)
            {
                if (this.options.Breaks)
                {
                    this.rules = new BreaksInlineRules();
                }
                else
                {
                    this.rules = new GfmInlineRules();
                }
            }
            else if (this.options.Pedantic)
            {
                this.rules = new PedanticInlineRules();
            }
        }



        /// <summary>
        /// Static Lexing/Compiling Method
        /// </summary>
        public static string Output(string src, IDictionary<string, LinkObj> links, Options options)
        {
            var inline = new InlineLexer(links, options);
            return inline.Output(src);
        }

        /// <summary>
        /// Lexing/Compiling
        /// </summary>
        private bool inLink;
        public virtual string Output(string src)
        {
            var @out = String.Empty;
            LinkObj link;
            string text;
            string href;
            IList<string> cap;

            while (!String.IsNullOrEmpty(src))
            {
                // escape
                cap = this.rules.Escape.Exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += cap[1];
                    continue;
                }

                // autolink
                cap = this.rules.AutoLink.Exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    if (cap[2] == "@")
                    {
                        text = cap[1][6] == ':'
                          ? this.Mangle(cap[1].Substring(7))
                          : this.Mangle(cap[1]);
                        href = this.Mangle("mailto:") + text;
                    }
                    else
                    {
                        text = StringHelper.Escape(cap[1]);
                        href = text;
                    }
                    @out += this.renderer.Link(href, null, text);
                    continue;
                }

                // url (gfm)
                if (!this.inLink && (cap = this.rules.Url.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    text = StringHelper.Escape(cap[1]);
                    href = text;
                    @out += this.renderer.Link(href, null, text);
                    continue;
                }

                // tag
                cap = this.rules.Tag.Exec(src);
                if (cap.Any())
                {
                    if (!this.inLink && Regex.IsMatch(cap[0], "^<a ", RegexOptions.IgnoreCase))
                    {
                        this.inLink = true;
                    }
                    else if (this.inLink && Regex.IsMatch(cap[0], @"^<\/a>", RegexOptions.IgnoreCase))
                    {
                        this.inLink = false;
                    }
                    src = src.Substring(cap[0].Length);
                    @out += this.options.Sanitize
                      ? (this.options.Sanitizer != null)
                        ? this.options.Sanitizer(cap[0])
                        : StringHelper.Escape(cap[0])
                      : cap[0];
                    continue;
                }

                // link
                cap = this.rules.Link.Exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    this.inLink = true;
                    @out += this.OutputLink(cap, new LinkObj
                    {
                        Href = cap[2],
                        Title = cap[3]
                    });
                    this.inLink = false;
                    continue;
                }

                // reflink, nolink
                if ((cap = this.rules.RefLink.Exec(src)).Any() || (cap = this.rules.NoLink.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    var linkStr = (StringHelper.NotEmpty(cap, 2, 1)).ReplaceRegex(@"\s+", " ");
                    
                    this.links.TryGetValue(linkStr.ToLower(), out link);
                    
                    if (link == null || String.IsNullOrEmpty(link.Href))
                    {
                        @out += cap[0][0];
                        src = cap[0].Substring(1) + src;
                        continue;
                    }
                    this.inLink = true;
                    @out += this.OutputLink(cap, link);
                    this.inLink = false;
                    continue;
                }

                // strong
                if ((cap = this.rules.Strong.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.Strong(this.Output(StringHelper.NotEmpty(cap, 2, 1)));
                    continue;
                }

                // em
                cap = this.rules.Em.Exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.Em(this.Output(StringHelper.NotEmpty(cap, 2, 1)));
                    continue;
                }

                // code
                cap = this.rules.Code.Exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.Codespan(StringHelper.Escape(cap[2], true));
                    continue;
                }

                // br
                cap = this.rules.Br.Exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.Br();
                    continue;
                }

                // del (gfm)
                cap = this.rules.Del.Exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.Del(this.Output(cap[1]));
                    continue;
                }

                // text
                cap = this.rules.Text.Exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.Text(StringHelper.Escape(this.Smartypants(cap[0])));
                    continue;
                }

                if (!String.IsNullOrEmpty(src))
                {
                    throw new Exception("Infinite loop on byte: " + (int)src[0]);
                }
            }

            return @out;
        }

        /// <summary>
        /// Compile Link
        /// </summary>
        protected virtual string OutputLink(IList<string> cap, LinkObj link)
        {
            string href = StringHelper.Escape(link.Href),
            title = !String.IsNullOrEmpty(link.Title) ? StringHelper.Escape(link.Title) : null;

            return cap[0][0] != '!'
                ? this.renderer.Link(href, title, this.Output(cap[1]))
                : this.renderer.Image(href, title, StringHelper.Escape(cap[1]));
        }

        /// <summary>
        /// Mangle Links
        /// </summary>
        protected virtual string Mangle(string text)
        {
            if (!this.options.Mangle) return text;
            var @out = String.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i].ToString();
                if (random.NextDouble() > 0.5)
                {
                    ch = 'x' + Convert.ToString((int)ch[0], 16);
                }
                @out += "&#" + ch + ";";
            }

            return @out;
        }

        /// <summary>
        /// Smartypants Transformations
        /// </summary>
        protected virtual string Smartypants(string text)
        {
            if (!this.options.Smartypants) return text;

            return text
                // em-dashes
                .Replace("---", "\u2014")
                // en-dashes
                .Replace("--", "\u2013")
                // opening singles
                .ReplaceRegex(@"(^|[-\u2014/(\[{""\s])'", "$1\u2018")
                // closing singles & apostrophes
                .Replace("'", "\u2019")
                // opening doubles
                .ReplaceRegex(@"(^|[-\u2014/(\[{\u2018\s])""", "$1\u201c")
                // closing doubles
                .Replace("\"", "\u201d")
                // ellipses
                .Replace("...", "\u2026");
        }
    }
}
