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
            this.renderer = options.renderer ?? new Renderer(options);

            this.links = links;
            this.rules = new NormalInlineRules();

            if (this.links == null)
            {
                throw new Exception("Tokens array requires a `links` property.");
            }

            if (this.options.gfm)
            {
                if (this.options.breaks)
                {
                    this.rules = new BreaksInlineRules();
                }
                else
                {
                    this.rules = new GfmInlineRules();
                }
            }
            else if (this.options.pedantic)
            {
                this.rules = new PedanticInlineRules();
            }
        }



        /// <summary>
        /// Static Lexing/Compiling Method
        /// </summary>
        public static string output(string src, IDictionary<string, LinkObj> links, Options options)
        {
            var inline = new InlineLexer(links, options);
            return inline.output(src);
        }

        /// <summary>
        /// Lexing/Compiling
        /// </summary>
        private bool inLink;
        public virtual string output(string src)
        {
            var @out = String.Empty;
            LinkObj link;
            string text;
            string href;
            IList<string> cap;

            while (!String.IsNullOrEmpty(src))
            {
                // escape
                cap = this.rules.escape.exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += cap[1];
                    continue;
                }

                // autolink
                cap = this.rules.autolink.exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    if (cap[2] == "@")
                    {
                        text = cap[1][6] == ':'
                          ? this.mangle(cap[1].Substring(7))
                          : this.mangle(cap[1]);
                        href = this.mangle("mailto:") + text;
                    }
                    else
                    {
                        text = StringHelper.escape(cap[1]);
                        href = text;
                    }
                    @out += this.renderer.link(href, null, text);
                    continue;
                }

                // url (gfm)
                if (!this.inLink && (cap = this.rules.url.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    text = StringHelper.escape(cap[1]);
                    href = text;
                    @out += this.renderer.link(href, null, text);
                    continue;
                }

                // tag
                cap = this.rules.tag.exec(src);
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
                    @out += this.options.sanitize
                      ? (this.options.sanitizer != null)
                        ? this.options.sanitizer(cap[0])
                        : StringHelper.escape(cap[0])
                      : cap[0];
                    continue;
                }

                // link
                cap = this.rules.link.exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    this.inLink = true;
                    @out += this.outputLink(cap, new LinkObj
                    {
                        href = cap[2],
                        title = cap[3]
                    });
                    this.inLink = false;
                    continue;
                }

                // reflink, nolink
                if ((cap = this.rules.reflink.exec(src)).Any() || (cap = this.rules.nolink.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    var linkStr = (StringHelper.NotEmpty(cap, 2, 1)).ReplaceRegex(@"\s+", " ");
                    
                    this.links.TryGetValue(linkStr.ToLower(), out link);
                    
                    if (link == null || String.IsNullOrEmpty(link.href))
                    {
                        @out += cap[0][0];
                        src = cap[0].Substring(1) + src;
                        continue;
                    }
                    this.inLink = true;
                    @out += this.outputLink(cap, link);
                    this.inLink = false;
                    continue;
                }

                // strong
                if ((cap = this.rules.strong.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.strong(this.output(StringHelper.NotEmpty(cap, 2, 1)));
                    continue;
                }

                // em
                cap = this.rules.em.exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.em(this.output(StringHelper.NotEmpty(cap, 2, 1)));
                    continue;
                }

                // code
                cap = this.rules.code.exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.codespan(StringHelper.escape(cap[2], true));
                    continue;
                }

                // br
                cap = this.rules.br.exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.br();
                    continue;
                }

                // del (gfm)
                cap = this.rules.del.exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.del(this.output(cap[1]));
                    continue;
                }

                // text
                cap = this.rules.text.exec(src);
                if (cap.Any())
                {
                    src = src.Substring(cap[0].Length);
                    @out += this.renderer.text(StringHelper.escape(this.smartypants(cap[0])));
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
        protected virtual string outputLink(IList<string> cap, LinkObj link)
        {
            string href = StringHelper.escape(link.href),
            title = !String.IsNullOrEmpty(link.title) ? StringHelper.escape(link.title) : null;

            return cap[0][0] != '!'
                ? this.renderer.link(href, title, this.output(cap[1]))
                : this.renderer.image(href, title, StringHelper.escape(cap[1]));
        }

        /// <summary>
        /// Mangle Links
        /// </summary>
        protected virtual string mangle(string text)
        {
            if (!this.options.mangle) return text;
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
        protected virtual string smartypants(string text)
        {
            if (!this.options.smartypants) return text;

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
