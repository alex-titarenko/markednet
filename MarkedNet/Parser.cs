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
            this.options.Renderer = this.options.Renderer ?? new Renderer(options);
            this.renderer = this.options.Renderer;
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
            this.inline = new InlineLexer(src.Links, this.options);
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
            var body = this.token.Text;

            while (this.peek().Type == "text")
            {
                body += '\n' + this.next().Text;
            }

            return this.inline.Output(body);
        }

        /// <summary>
        /// Parse Current Token
        /// </summary>    
        protected virtual string tok()
        {
            switch (this.token.Type)
            {
                case "space":
                    {
                        return String.Empty;
                    }
                case "hr":
                    {
                        return this.renderer.Hr();
                    }
                case "heading":
                    {
                        return this.renderer.Heading(this.inline.Output(this.token.Text), this.token.Depth, this.token.Text);
                    }
                case "code":
                    {
                        return this.renderer.Code(this.token.Text, this.token.Lang, this.token.Escaped);
                    }
                case "table":
                    {
                        string header = String.Empty, body = String.Empty;

                        // header
                        var cell = String.Empty;
                        for (int i = 0; i < this.token.Header.Count; i++)
                        {
                            cell += this.renderer.TableCell(
                              this.inline.Output(this.token.Header[i]),
                              new TableCellFlags { Header = true, Align = i < this.token.Align.Count ? this.token.Align[i] : null }
                            );
                        }
                        header += this.renderer.TableRow(cell);

                        for (int i = 0; i < this.token.Cells.Count; i++)
                        {
                            var row = this.token.Cells[i];

                            cell = String.Empty;
                            for (int j = 0; j < row.Count; j++)
                            {
                                cell += this.renderer.TableCell(
                                  this.inline.Output(row[j]),
                                  new TableCellFlags { Header = false, Align = j < this.token.Align.Count ? this.token.Align[j] : null }
                                );
                            }

                            body += this.renderer.TableRow(cell);
                        }
                        return this.renderer.Table(header, body);
                    }
                case "blockquote_start":
                    {
                        var body = String.Empty;

                        while (this.next().Type != "blockquote_end")
                        {
                            body += this.tok();
                        }

                        return this.renderer.Blockquote(body);
                    }
                case "list_start":
                    {
                        var body = String.Empty;
                        var ordered = this.token.Ordered;

                        while (this.next().Type != "list_end")
                        {
                            body += this.tok();
                        }

                        return this.renderer.List(body, ordered);
                    }
                case "list_item_start":
                    {
                        var body = String.Empty;

                        while (this.next().Type != "list_item_end")
                        {
                            body += this.token.Type == "text"
                              ? this.parseText()
                              : this.tok();
                        }

                        return this.renderer.ListItem(body);
                    }
                case "loose_item_start":
                    {
                        var body = String.Empty;

                        while (this.next().Type != "list_item_end")
                        {
                            body += this.tok();
                        }

                        return this.renderer.ListItem(body);
                    }
                case "html":
                    {
                        var html = !this.token.Pre && !this.options.Pedantic
                          ? this.inline.Output(this.token.Text)
                          : this.token.Text;
                        return this.renderer.Html(html);
                    }
                case "paragraph":
                    {
                        return this.renderer.Paragraph(this.inline.Output(this.token.Text));
                    }
                case "text":
                    {
                        return this.renderer.Paragraph(this.parseText());
                    }
            }

            throw new Exception();
        }
    }
}
