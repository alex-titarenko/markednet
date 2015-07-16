using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MarkedNet
{
    public class Parser
    {
        private Options options; 
        private Renderer renderer;
        private InlineLexer inline;
        private Stack<Token> tokens;
        private Token token;


        public Parser(Options options)
        {
            this.tokens = new Stack<Token>();
            this.token = null;
            this.options = options ?? new Options();
            this.options.renderer = this.options.renderer ?? new Renderer(options);
            this.renderer = this.options.renderer;
        }


        /// <summary>
        /// Static Parse Method
        /// </summary>
        public static string parse(TokensResult src, Options options)
        {
            var parser = new Parser(options);
            return parser.parse(src);
        }

        /// <summary>
        /// Parse Loop
        /// </summary>
        public virtual string parse(TokensResult src)
        {
            this.inline = new InlineLexer(src.links, this.options);
            this.tokens = new Stack<Token>(src.Reverse());

            var @out = String.Empty;
            while (this.next() != null)
            {
                @out += this.tok();
            }

            return @out;
        }


        /// <summary>
        /// Next Token
        /// </summary>
        protected virtual Token next()
        {
            return this.token = (this.tokens.Any()) ? this.tokens.Pop() : null;
        }


        /// <summary>
        /// Preview Next Token
        /// </summary>
        protected virtual Token peek()
        {
            return this.tokens.Peek() ?? new Token();
        }


        /// <summary>
        /// Parse Text Tokens
        /// </summary>    
        protected virtual string parseText()
        {
            var body = this.token.text;

            while (this.peek().type == "text")
            {
                body += '\n' + this.next().text;
            }

            return this.inline.output(body);
        }

        /// <summary>
        /// Parse Current Token
        /// </summary>    
        protected virtual string tok()
        {
            switch (this.token.type)
            {
                case "space":
                    {
                        return String.Empty;
                    }
                case "hr":
                    {
                        return this.renderer.hr();
                    }
                case "heading":
                    {
                        return this.renderer.heading(this.inline.output(this.token.text), this.token.depth, this.token.text);
                    }
                case "code":
                    {
                        return this.renderer.code(this.token.text, this.token.lang, this.token.escaped);
                    }
                case "table":
                    {
                        string header = String.Empty, body = String.Empty;

                        // header
                        var cell = String.Empty;
                        for (int i = 0; i < this.token.header.Count; i++)
                        {
                            cell += this.renderer.tablecell(
                              this.inline.output(this.token.header[i]),
                              new TableCellFlags { header = true, align = i < this.token.align.Count ? this.token.align[i] : null }
                            );
                        }
                        header += this.renderer.tablerow(cell);

                        for (int i = 0; i < this.token.cells.Count; i++)
                        {
                            var row = this.token.cells[i];

                            cell = String.Empty;
                            for (int j = 0; j < row.Count; j++)
                            {
                                cell += this.renderer.tablecell(
                                  this.inline.output(row[j]),
                                  new TableCellFlags { header = false, align = j < this.token.align.Count ? this.token.align[j] : null }
                                );
                            }

                            body += this.renderer.tablerow(cell);
                        }
                        return this.renderer.table(header, body);
                    }
                case "blockquote_start":
                    {
                        var body = String.Empty;

                        while (this.next().type != "blockquote_end")
                        {
                            body += this.tok();
                        }

                        return this.renderer.blockquote(body);
                    }
                case "list_start":
                    {
                        var body = String.Empty;
                        var ordered = this.token.ordered;

                        while (this.next().type != "list_end")
                        {
                            body += this.tok();
                        }

                        return this.renderer.list(body, ordered);
                    }
                case "list_item_start":
                    {
                        var body = String.Empty;

                        while (this.next().type != "list_item_end")
                        {
                            body += this.token.type == "text"
                              ? this.parseText()
                              : this.tok();
                        }

                        return this.renderer.listitem(body);
                    }
                case "loose_item_start":
                    {
                        var body = String.Empty;

                        while (this.next().type != "list_item_end")
                        {
                            body += this.tok();
                        }

                        return this.renderer.listitem(body);
                    }
                case "html":
                    {
                        var html = !this.token.pre && !this.options.pedantic
                          ? this.inline.output(this.token.text)
                          : this.token.text;
                        return this.renderer.html(html);
                    }
                case "paragraph":
                    {
                        return this.renderer.paragraph(this.inline.output(this.token.text));
                    }
                case "text":
                    {
                        return this.renderer.paragraph(this.parseText());
                    }
            }

            throw new Exception();
        }
    }
}
