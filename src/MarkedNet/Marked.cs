using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MarkedNet
{
    public class Marked
    {
        public Options Options { get; set; }


        public Marked()
            : this(null)
        {
        }

        public Marked(Options options)
        {
            Options = options ?? new Options();
        }


        public static string Parse(string src, Options options)
        {
            var marked = new Marked(options);
            return marked.Parse(src);
        }

        public virtual string Parse(string src)
        {
            if (String.IsNullOrEmpty(src))
            {
                return src;
            }

            var tokens = Lexer.Lex(src, Options);
            var result = Parser.Parse(tokens, Options);
            return result;
        }
    }
}
