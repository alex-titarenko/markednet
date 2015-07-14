using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace MarkedNet
{
    /// <summary>
    /// Block-Level Grammar
    /// </summary>
    public class BlockRules
    {
        public virtual Regex newline { get { return new Regex(@"^\n+"); } }
        public virtual Regex code { get { return new Regex(@"^( {4}[^\n]+\n*)+"); } }
        public virtual Regex fences { get { return new Regex(""); } } // noop
        public virtual Regex hr { get { return new Regex(@"^( *[-*_]){3,} *(?:\n+|$)"); } }
        public virtual Regex heading { get { return new Regex(@"^ *(#{1,6}) *([^\n]+?) *#* *(?:\n+|$)"); } }
        public virtual Regex nptable { get { return new Regex(""); } } // noop
        public virtual Regex lheading { get { return new Regex(@"^([^\n]+)\n *(=|-){2,} *(?:\n+|$)"); } }
        public virtual Regex blockquote { get { return new Regex(@"^( *>[^\n]+(\n(?! *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$))[^\n]+)*\n*)+"); } }
        public virtual Regex list { get { return new Regex(@"^( *)((?:[*+-]|\d+\.)) [\s\S]+?(?:\n+(?=\1?(?:[-*_] *){3,}(?:\n+|$))|\n+(?= *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$))|\n{2,}(?! )(?!\1(?:[*+-]|\d+\.) )\n*|\s*$)"); } }
        public virtual Regex html { get { return new Regex(@"^ *(?:<!--[\s\S]*?-->|<((?!(?:a|em|strong|small|s|cite|q|dfn|abbr|data|time|code|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo|span|br|wbr|ins|del|img)\b)\w+(?!:\/|[^\w\s@]*@)\b)[\s\S]+?<\/\1>|<(?!(?:a|em|strong|small|s|cite|q|dfn|abbr|data|time|code|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo|span|br|wbr|ins|del|img)\b)\w+(?!:\/|[^\w\s@]*@)\b(?:""[^""]*""|'[^']*'|[^'"">])*?>) *(?:\n{2,}|\s*$)"); } }
        public virtual Regex def { get { return new Regex(@"^ *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$)"); } }
        public virtual Regex table { get { return new Regex(""); } } // noop
        public virtual Regex paragraph { get { return new Regex(@"^((?:[^\n]+\n?(?!( *[-*_]){3,} *(?:\n+|$)| *(#{1,6}) *([^\n]+?) *#* *(?:\n+|$)|([^\n]+)\n *(=|-){2,} *(?:\n+|$)|( *>[^\n]+(\n(?! *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$))[^\n]+)*\n*)+|<(?!(?:a|em|strong|small|s|cite|q|dfn|abbr|data|time|code|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo|span|br|wbr|ins|del|img)\b)\w+(?!:\/|[^\w\s@]*@)\b| *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$)))+)\n*"); } }
        public virtual Regex text { get { return new Regex(@"^[^\n]+"); } }
        public virtual Regex bullet { get { return new Regex(@"(?:[*+-]|\d+\.)"); } }
        public virtual Regex item { get { return new Regex(@"^( *)((?:[*+-]|\d+\.)) [^\n]*(?:\n(?!\1(?:[*+-]|\d+\.) )[^\n]*)*"); } }
    }

    /// <summary>
    /// Normal Block Grammar
    /// </summary>
    public class NormalBlockRules : BlockRules
    {
    }

    /// <summary>
    /// GFM Block Grammar
    /// </summary>
    public class GfmBlockRules : BlockRules
    {
        public override Regex fences { get { return new Regex(@"^ *(`{3,}|~{3,}) *(\S+)? *\n([\s\S]+?)\s*\1 *(?:\n+|$)"); } }
        public override Regex paragraph { get { return new Regex(@"^((?:[^\n]+\n?(?! *(`{3,}|~{3,}) *(\S+)? *\n([\s\S]+?)\s*\2 *(?:\n+|$)|( *)((?:[*+-]|\d+\.)) [\s\S]+?(?:\n+(?=\3?(?:[-*_] *){3,}(?:\n+|$))|\n+(?= *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$))|\n{2,}(?! )(?!\1(?:[*+-]|\d+\.) )\n*|\s*$)|( *[-*_]){3,} *(?:\n+|$)| *(#{1,6}) *([^\n]+?) *#* *(?:\n+|$)|([^\n]+)\n *(=|-){2,} *(?:\n+|$)|( *>[^\n]+(\n(?! *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$))[^\n]+)*\n*)+|<(?!(?:a|em|strong|small|s|cite|q|dfn|abbr|data|time|code|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo|span|br|wbr|ins|del|img)\b)\w+(?!:\/|[^\w\s@]*@)\b| *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$)))+)\n*"); } }
        public override Regex heading { get { return new Regex(@"^ *(#{1,6}) *([^\n]+?) *#* *(?:\n+|$)"); } }
    }

    /// <summary>
    /// GFM + Tables Block Grammar
    /// </summary>
    public class TablesBlockRules : GfmBlockRules
    {
        public override Regex nptable { get { return new Regex(@"^ *(\S.*\|.*)\n *([-:]+ *\|[-| :]*)\n((?:.*\|.*(?:\n|$))*)\n*"); } }
        public override Regex table { get { return new Regex(@"^ *\|(.+)\n *\|( *[-:]+[-| :]*)\n((?: *\|.*(?:\n|$))*)\n*"); } }
    }
}
