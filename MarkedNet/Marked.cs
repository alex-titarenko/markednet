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

            TokensResult tokens;

            try
            {
              tokens = BlockLexer.lex(src, Options);
            }
            catch (Exception)
            {
              throw;
            }

            string @out = Parser.parse(tokens, Options);
            return @out;


  //  for (; i < tokens.length; i++) {
  //    (function(token) {
  //      if (token.type !== 'code') {
  //        return --pending || done();
  //      }
  //      return highlight(token.text, token.lang, function(err, code) {
  //        if (err) return done(err);
  //        if (code == null || code === token.text) {
  //          return --pending || done();
  //        }
  //        token.text = code;
  //        token.escaped = true;
  //        --pending || done();
  //      });
  //    })(tokens[i]);
  //  }

  //  return;
  //}
  //try {
  //  if (opt) opt = merge({}, marked.defaults, opt);
  //  return Parser.parse(Lexer.lex(src, opt), opt);
  //} catch (e) {
  //  e.message += '\nPlease report this to https://github.com/chjj/marked.';
  //  if ((opt || marked.defaults).silent) {
  //    return '<p>An error occured:</p><pre>'
  //      + escape(e.message + '', true)
  //      + '</pre>';
  //  }
  //  throw e;
  //}
        }
    }
}
