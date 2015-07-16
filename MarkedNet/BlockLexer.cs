using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace MarkedNet
{
    public class BlockLexer
    {
        private Options options;
        private BlockRules rules;
        private TokensResult tokens;


        public BlockLexer(Options options)
        {
            this.tokens = new TokensResult();
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


        /// <summary>
        /// Static Lex Method
        /// </summary>
        public static TokensResult lex(string src, Options options)
        {
            var lexer = new BlockLexer(options);
            return lexer.lex(src);
        }

        /// <summary>
        /// Preprocessing
        /// </summary>
        protected virtual TokensResult lex(string src)
        {
            src = src
                .ReplaceRegex(@"\r\n|\r", "\n")
                .Replace("\t", "    ")
                .Replace("\u00a0", " ")
                .Replace("\u2424", "\n");

            return this.token(src, true);
        }

        /// <summary>
        /// Lexing
        /// </summary>
        protected virtual TokensResult token(string srcOrig, bool top)
        {
            var src = Regex.Replace(srcOrig, @"^ +$", "", RegexOptions.Multiline);
            bool loose;
            IList<string> cap;
            string bull;
            string b;
            int i;
            int l;

            while (!String.IsNullOrEmpty(src))
            {
                // newline
                if ((cap = this.rules.newline.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    if (cap[0].Length > 1)
                    {
                        this.tokens.Add(new Token
                        {
                            type = "space"
                        });
                    }
                }

                // code
                if ((cap = this.rules.code.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    var capStr = Regex.Replace(cap[0], @"^ {4}", "", RegexOptions.Multiline);
                    this.tokens.Add(new Token
                    {
                        type = "code",
                        text = !this.options.pedantic
                          ? Regex.Replace(capStr, @"\n+$", "")
                          : capStr
                    });
                    continue;
                }

                // fences (gfm)
                if ((cap = this.rules.fences.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    this.tokens.Add(new Token
                    {
                        type = "code",
                        lang = cap[2],
                        text = cap[3]
                    });
                    continue;
                }

                // heading
                if ((cap = this.rules.heading.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    this.tokens.Add(new Token
                    {
                        type = "heading",
                        depth = cap[1].Length,
                        text = cap[2]
                    });
                    continue;
                }

                // table no leading pipe (gfm)
                if (top && (cap = this.rules.nptable.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);

                    var item = new Token
                    {
                        type = "table",
                        header = cap[1].ReplaceRegex(@"^ *| *\| *$", "").SplitRegex(@" *\| *"),
                        align = cap[2].ReplaceRegex(@"^ *|\| *$", "").SplitRegex(@" *\| *"),
                        cells = cap[3].ReplaceRegex(@"\n$", "").Split('\n').Select(x => new string[] { x }).ToArray()
                    };

                    for (i = 0; i < item.align.Count; i++)
                    {
                        if (Regex.IsMatch(item.align[i], @"^ *-+: *$"))
                        {
                            item.align[i] = "right";
                        }
                        else if (Regex.IsMatch(item.align[i], @"^ *:-+: *$"))
                        {
                            item.align[i] = "center";
                        }
                        else if (Regex.IsMatch(item.align[i], @"^ *:-+ *$"))
                        {
                            item.align[i] = "left";
                        }
                        else
                        {
                            item.align[i] = null;
                        }
                    }

                    for (i = 0; i < item.cells.Count; i++)
                    {
                        item.cells[i] = item.cells[i][0].SplitRegex(@" *\| *");
                    }

                    this.tokens.Add(item);

                    continue;
                }

                // lheading
                if ((cap = this.rules.lheading.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    this.tokens.Add(new Token
                    {
                        type = "heading",
                        depth = cap[2] == "=" ? 1 : 2,
                        text = cap[1]
                    });
                    continue;
                }

                // hr
                if ((cap = this.rules.hr.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    this.tokens.Add(new Token
                    {
                        type = "hr"
                    });
                    continue;
                }

                // blockquote
                if ((cap = this.rules.blockquote.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);

                    this.tokens.Add(new Token
                    {
                        type = "blockquote_start"
                    });

                    var capStr = Regex.Replace(cap[0], @"^ *> ?", "", RegexOptions.Multiline);

                    // Pass `top` to keep the current
                    // "toplevel" state. This is exactly
                    // how markdown.pl works.
                    this.token(capStr, top); //, true);

                    this.tokens.Add(new Token
                    {
                        type = "blockquote_end"
                    });

                    continue;
                }

                // list
                if ((cap = this.rules.list.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    bull = cap[2];

                    this.tokens.Add(new Token
                    {
                        type = "list_start",
                        ordered = bull.Length > 1
                    });

                    // Get each top-level item.
                    cap = cap[0].match(this.rules.item);

                    var next = false;
                    l = cap.Count;
                    i = 0;

                    for (; i < l; i++)
                    {
                        var item = cap[i];

                        // Remove the list item's bullet
                        // so it is seen as the next token.
                        var space = item.Length;
                        item = item.ReplaceRegex(@"^ *([*+-]|\d+\.) +", "");

                        // Outdent whatever the
                        // list item contains. Hacky.
                        if (item.IndexOf("\n ") > -1)
                        {
                            space -= item.Length;
                            item = !this.options.pedantic
                              ? Regex.Replace(item, "^ {1," + space + "}", "", RegexOptions.Multiline)
                              : Regex.Replace(item, @"/^ {1,4}", "", RegexOptions.Multiline);
                        }

                        // Determine whether the next list item belongs here.
                        // Backpedal if it does not belong in this list.
                        if (this.options.smartLists && i != l - 1)
                        {
                            b = this.rules.bullet.exec(cap[i + 1])[0]; // !!!!!!!!!!!
                            if (bull != b && !(bull.Length > 1 && b.Length > 1))
                            {
                                src = String.Join("\n", cap.Skip(i + 1)) + src;
                                i = l - 1;
                            }
                        }

                        // Determine whether item is loose or not.
                        // Use: /(^|\n)(?! )[^\n]+\n\n(?!\s*$)/
                        // for discount behavior.
                        loose = next || Regex.IsMatch(item, @"\n\n(?!\s*$)");
                        if (i != l - 1)
                        {
                            next = item[item.Length - 1] == '\n';
                            if (!loose) loose = next;
                        }

                        this.tokens.Add(new Token
                        {
                            type = loose
                              ? "loose_item_start"
                              : "list_item_start"
                        });

                        // Recurse.
                        this.token(item, false);

                        this.tokens.Add(new Token
                        {
                            type = "list_item_end"
                        });
                    }

                    this.tokens.Add(new Token
                    {
                        type = "list_end"
                    });

                    continue;
                }

                // html
                if ((cap = this.rules.html.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    this.tokens.Add(new Token
                    {
                        type = this.options.sanitize
                          ? "paragraph"
                          : "html",
                        pre = (this.options.sanitizer == null)
                          && (cap[1] == "pre" || cap[1] == "script" || cap[1] == "style"),
                        text = cap[0]
                    });
                    continue;
                }

                // def
                if ((top) && (cap = this.rules.def.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    this.tokens.links[cap[1].ToLower()] = new LinkObj
                    {
                        href = cap[2],
                        title = cap[3]
                    };
                    continue;
                }

                // table (gfm)
                if (top && (cap = this.rules.table.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);

                    var item = new Token
                    {
                        type = "table",
                        header = cap[1].ReplaceRegex(@"^ *| *\| *$", "").SplitRegex(@" *\| *"),
                        align = cap[2].ReplaceRegex(@"^ *|\| *$", "").SplitRegex(@" *\| *"),
                        cells = cap[3].ReplaceRegex(@"(?: *\| *)?\n$", "").Split('\n').Select(x => new string[] { x }).ToArray()
                    };

                    for (i = 0; i < item.align.Count; i++)
                    {
                        if (Regex.IsMatch(item.align[i], @"^ *-+: *$"))
                        {
                            item.align[i] = "right";
                        }
                        else if (Regex.IsMatch(item.align[i], @"^ *:-+: *$"))
                        {
                            item.align[i] = "center";
                        }
                        else if (Regex.IsMatch(item.align[i], @"^ *:-+ *$"))
                        {
                            item.align[i] = "left";
                        }
                        else
                        {
                            item.align[i] = null;
                        }
                    }

                    for (i = 0; i < item.cells.Count; i++)
                    {
                        item.cells[i] = item.cells[i][0]
                          .ReplaceRegex(@"^ *\| *| *\| *$", "")
                          .SplitRegex(@" *\| *");
                    }

                    this.tokens.Add(item);

                    continue;
                }

                // top-level paragraph
                if (top && (cap = this.rules.paragraph.exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    this.tokens.Add(new Token
                    {
                        type = "paragraph",
                        text = cap[1][cap[1].Length - 1] == '\n'
                          ? cap[1].Substring(0, cap[1].Length - 1)
                          : cap[1]
                    });
                    continue;
                }

                // text
                if ((cap = this.rules.text.exec(src)).Any())
                {
                    // Top-level should never reach here.
                    src = src.Substring(cap[0].Length);
                    this.tokens.Add(new Token
                    {
                        type = "text",
                        text = cap[0]
                    });
                    continue;
                }

                if (!String.IsNullOrEmpty(src))
                {
                    throw new Exception("Infinite loop on byte: " + (int)src[0]);
                }
            }

            return this.tokens;
        }
    }
}
