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


        public string Parse(string src)
        {
            if (String.IsNullOrEmpty(src))
            {
                return src;
            }

            var tokens = BlockLexer.Lex(src, Options);
            var result = Parser.parse(tokens, Options);
            return result;
        }
    }
}
