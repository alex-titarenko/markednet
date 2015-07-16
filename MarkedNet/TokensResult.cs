using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkedNet
{
    public class TokensResult
    {
        public IList<Token> tokens { get; set; }
        public IDictionary<string, LinkObj> links { get; set; }
        public int Length { get { return tokens.Count; } }

        public IEnumerable<Token> Reverse()
        {
            return tokens.Reverse();
        }

        public TokensResult()
        {
            tokens = new List<Token>();
            links = new Dictionary<string, LinkObj>();
        }


        public void Add(Token token)
        {
            tokens.Add(token);
        }
    }
}
