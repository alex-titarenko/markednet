using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace MarkedNet
{
    public class Lexer
    {
        private readonly Options options;
        private readonly BlockRules rules;


        public Lexer(Options options)
        {
            this.options = options ?? new Options();
            this.rules = new NormalBlockRules();

            if (this.options.Gfm)
            {
                if (this.options.Tables)
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
        public static TokensResult Lex(string src, Options options)
        {
            var lexer = new Lexer(options);
            return lexer.Lex(src);
        }

        /// <summary>
        /// Preprocessing
        /// </summary>
        protected virtual TokensResult Lex(string src)
        {
            src = src
                .ReplaceRegex(@"\r\n|\r", "\n")
                .Replace("\t", "    ")
                .Replace("\u00a0", " ")
                .Replace("\u2424", "\n");

            return Token(src, true);
        }

        /// <summary>
        /// Lexing
        /// </summary>
        protected virtual TokensResult Token(string srcOrig, bool top, TokensResult result = null)
        {
            var src = Regex.Replace(srcOrig, @"^ +$", "", RegexOptions.Multiline);
            IList<string> cap;
            var tokens = result ?? new TokensResult();

            while (!String.IsNullOrEmpty(src))
            {
                // newline
                if ((cap = this.rules.Newline.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    if (cap[0].Length > 1)
                    {
                        tokens.Add(new Token
                        {
                            Type = "space"
                        });
                    }
                }

                // code
                if ((cap = this.rules.Сode.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    var capStr = Regex.Replace(cap[0], @"^ {4}", "", RegexOptions.Multiline);
                    tokens.Add(new Token
                    {
                        Type = "code",
                        Text = !this.options.Pedantic
                          ? Regex.Replace(capStr, @"\n+$", "")
                          : capStr
                    });
                    continue;
                }

                // fences (gfm)
                if ((cap = this.rules.Fences.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    tokens.Add(new Token
                    {
                        Type = "code",
                        Lang = cap[2],
                        Text = cap[3]
                    });
                    continue;
                }

                // heading
                if ((cap = this.rules.Heading.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    tokens.Add(new Token
                    {
                        Type = "heading",
                        Depth = cap[1].Length,
                        Text = cap[2]
                    });
                    continue;
                }

                // table no leading pipe (gfm)
                if (top && (cap = this.rules.NpTable.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);

                    var item = new Token
                    {
                        Type = "table",
                        Header = cap[1].ReplaceRegex(@"^ *| *\| *$", "").SplitRegex(@" *\| *"),
                        Align = cap[2].ReplaceRegex(@"^ *|\| *$", "").SplitRegex(@" *\| *"),
                        Cells = cap[3].ReplaceRegex(@"\n$", "").Split('\n').Select(x => new string[] { x }).ToArray()
                    };

                    for (int i = 0; i < item.Align.Count; i++)
                    {
                        if (Regex.IsMatch(item.Align[i], @"^ *-+: *$"))
                        {
                            item.Align[i] = "right";
                        }
                        else if (Regex.IsMatch(item.Align[i], @"^ *:-+: *$"))
                        {
                            item.Align[i] = "center";
                        }
                        else if (Regex.IsMatch(item.Align[i], @"^ *:-+ *$"))
                        {
                            item.Align[i] = "left";
                        }
                        else
                        {
                            item.Align[i] = null;
                        }
                    }

                    for (int i = 0; i < item.Cells.Count; i++)
                    {
                        item.Cells[i] = item.Cells[i][0].SplitRegex(@" *\| *");
                    }

                    tokens.Add(item);

                    continue;
                }

                // lheading
                if ((cap = this.rules.LHeading.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    tokens.Add(new Token
                    {
                        Type = "heading",
                        Depth = cap[2] == "=" ? 1 : 2,
                        Text = cap[1]
                    });
                    continue;
                }

                // hr
                if ((cap = this.rules.Hr.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    tokens.Add(new Token
                    {
                        Type = "hr"
                    });
                    continue;
                }

                // blockquote
                if ((cap = this.rules.Blockquote.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);

                    tokens.Add(new Token
                    {
                        Type = "blockquote_start"
                    });

                    var capStr = Regex.Replace(cap[0], @"^ *> ?", "", RegexOptions.Multiline);

                    // Pass `top` to keep the current
                    // "toplevel" state. This is exactly
                    // how markdown.pl works.
                    Token(capStr, top, tokens); //, true);

                    tokens.Add(new Token
                    {
                        Type = "blockquote_end"
                    });

                    continue;
                }

                // list
                if ((cap = this.rules.List.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    var bull = cap[2];

                    tokens.Add(new Token
                    {
                        Type = "list_start",
                        Ordered = bull.Length > 1
                    });

                    // Get each top-level item.
                    cap = cap[0].Match(this.rules.Item);

                    var next = false;
                    var l = cap.Count;
                    int i = 0;

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
                            item = !this.options.Pedantic
                              ? Regex.Replace(item, "^ {1," + space + "}", "", RegexOptions.Multiline)
                              : Regex.Replace(item, @"/^ {1,4}", "", RegexOptions.Multiline);
                        }

                        // Determine whether the next list item belongs here.
                        // Backpedal if it does not belong in this list.
                        if (this.options.SmartLists && i != l - 1)
                        {
                            var b = this.rules.Bullet.Exec(cap[i + 1])[0]; // !!!!!!!!!!!
                            if (bull != b && !(bull.Length > 1 && b.Length > 1))
                            {
                                src = String.Join("\n", cap.Skip(i + 1)) + src;
                                i = l - 1;
                            }
                        }

                        // Determine whether item is loose or not.
                        // Use: /(^|\n)(?! )[^\n]+\n\n(?!\s*$)/
                        // for discount behavior.
                        var loose = next || Regex.IsMatch(item, @"\n\n(?!\s*$)");
                        if (i != l - 1)
                        {
                            next = item[item.Length - 1] == '\n';
                            if (!loose) loose = next;
                        }

                        tokens.Add(new Token
                        {
                            Type = loose
                              ? "loose_item_start"
                              : "list_item_start"
                        });

                        // Recurse.
                        Token(item, false, tokens);

                        tokens.Add(new Token
                        {
                            Type = "list_item_end"
                        });
                    }

                    tokens.Add(new Token
                    {
                        Type = "list_end"
                    });

                    continue;
                }

                // html
                if ((cap = this.rules.Html.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    tokens.Add(new Token
                    {
                        Type = this.options.Sanitize
                          ? "paragraph"
                          : "html",
                        Pre = (this.options.Sanitizer == null)
                          && (cap[1] == "pre" || cap[1] == "script" || cap[1] == "style"),
                        Text = cap[0]
                    });
                    continue;
                }

                // def
                if ((top) && (cap = this.rules.Def.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    tokens.Links[cap[1].ToLower()] = new LinkObj
                    {
                        Href = cap[2],
                        Title = cap[3]
                    };
                    continue;
                }

                // table (gfm)
                if (top && (cap = this.rules.Table.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);

                    var item = new Token
                    {
                        Type = "table",
                        Header = cap[1].ReplaceRegex(@"^ *| *\| *$", "").SplitRegex(@" *\| *"),
                        Align = cap[2].ReplaceRegex(@"^ *|\| *$", "").SplitRegex(@" *\| *"),
                        Cells = cap[3].ReplaceRegex(@"(?: *\| *)?\n$", "").Split('\n').Select(x => new string[] { x }).ToArray()
                    };

                    for (int i = 0; i < item.Align.Count; i++)
                    {
                        if (Regex.IsMatch(item.Align[i], @"^ *-+: *$"))
                        {
                            item.Align[i] = "right";
                        }
                        else if (Regex.IsMatch(item.Align[i], @"^ *:-+: *$"))
                        {
                            item.Align[i] = "center";
                        }
                        else if (Regex.IsMatch(item.Align[i], @"^ *:-+ *$"))
                        {
                            item.Align[i] = "left";
                        }
                        else
                        {
                            item.Align[i] = null;
                        }
                    }

                    for (int i = 0; i < item.Cells.Count; i++)
                    {
                        item.Cells[i] = item.Cells[i][0]
                          .ReplaceRegex(@"^ *\| *| *\| *$", "")
                          .SplitRegex(@" *\| *");
                    }

                    tokens.Add(item);

                    continue;
                }

                // top-level paragraph
                if (top && (cap = this.rules.Paragraph.Exec(src)).Any())
                {
                    src = src.Substring(cap[0].Length);
                    tokens.Add(new Token
                    {
                        Type = "paragraph",
                        Text = cap[1][cap[1].Length - 1] == '\n'
                          ? cap[1].Substring(0, cap[1].Length - 1)
                          : cap[1]
                    });
                    continue;
                }

                // text
                if ((cap = this.rules.Text.Exec(src)).Any())
                {
                    // Top-level should never reach here.
                    src = src.Substring(cap[0].Length);
                    tokens.Add(new Token
                    {
                        Type = "text",
                        Text = cap[0]
                    });
                    continue;
                }

                if (!String.IsNullOrEmpty(src))
                {
                    throw new Exception("Infinite loop on byte: " + (int)src[0]);
                }
            }

            return tokens;
        }
    }
}
