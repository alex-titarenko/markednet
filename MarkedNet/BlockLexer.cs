using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkedNet
{
    public class BlockLexer
    {
        private Options options;
        private BlockRules rules;
        private IList<Token> tokens;


        public BlockLexer(Options options)
        {
            this.tokens = new List<Token>();
            //this.tokens.links = {}; // !!!!!!!!!!!!!!!!!!!!

            this.options = options ?? new Options();

            this.rules = new NormalBlockRules();

            if (this.options.gfm)
            {
                if (this.options.tables)
                {
                    this.rules = new TablesBlockRules();
                }
                else
                {
                    this.rules = new GfmBlockRules();
                }
            }
        }
    }
}
