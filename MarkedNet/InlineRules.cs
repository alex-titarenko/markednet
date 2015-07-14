using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace MarkedNet
{
    /// <summary>
    /// Inline-Level Grammar
    /// </summary>
    public class InlineRules
    {
          public virtual Regex escape { get { return new Regex(@"^\\([\\`*{}\[\]()#+\-.!_>])"); } }
          public virtual Regex autolink { get { return new Regex(@"^<([^ >]+(@|:\/)[^ >]+)>"); } }
          public virtual Regex url { get { return new Regex(""); } } // noop
          public virtual Regex tag { get { return new Regex(@"^<!--[\s\S]*?-->|^<\/?\w+(?:""[^""]*""|'[^']*'|[^'"">])*?>"); } }
          public virtual Regex link { get { return new Regex(@"^!?\[((?:\[[^\]]*\]|[^\[\]]|\](?=[^\[]*\]))*)\]\(\s*<?([\s\S]*?)>?(?:\s+['""]([\s\S]*?)['""])?\s*\)"); } }
          public virtual Regex reflink { get { return new Regex(@"^!?\[((?:\[[^\]]*\]|[^\[\]]|\](?=[^\[]*\]))*)\]\s*\[([^\]]*)\]"); } }
          public virtual Regex nolink { get { return new Regex(@"^!?\[((?:\[[^\]]*\]|[^\[\]])*)\]"); } }
          public virtual Regex strong { get { return new Regex(@"^__([\s\S]+?)__(?!_)|^\*\*([\s\S]+?)\*\*(?!\*)"); } }
          public virtual Regex em { get { return new Regex(@"^\b_((?:__|[\s\S])+?)_\b|^\*((?:\*\*|[\s\S])+?)\*(?!\*)"); } }
          public virtual Regex code { get { return new Regex(@"^(`+)\s*([\s\S]*?[^`])\s*\1(?!`)"); } }
          public virtual Regex br { get { return new Regex(@"^ {2,}\n(?!\s*$)"); } }
          public virtual Regex del { get { return new Regex(""); } } // noop
          public virtual Regex text { get { return new Regex(@"^[\s\S]+?(?=[\\<!\[_*`]| {2,}\n|$)"); } }
    }

    /// <summary>
    /// Normal Inline Grammar
    /// </summary>
    public class NormalInlineRules : InlineRules
    {
    }

    /// <summary>
    /// Pedantic Inline Grammar
    /// </summary>
    public class PedanticInlineRules : InlineRules
    {
        public override Regex strong { get { return new Regex(@"^__(?=\S)([\s\S]*?\S)__(?!_)|^\*\*(?=\S)([\s\S]*?\S)\*\*(?!\*)"); } }
        public override Regex em { get { return new Regex(@"^_(?=\S)([\s\S]*?\S)_(?!_)|^\*(?=\S)([\s\S]*?\S)\*(?!\*)"); } }
    }

    /// <summary>
    /// GFM Inline Grammar
    /// </summary>
    public class GfmInlineRules : InlineRules
    {
        public override Regex escape { get { return new Regex(@"^\\([\\`*{}\[\]()#+\-.!_>~|])"); } }
        public override Regex url { get { return new Regex(@"^(https?:\/\/[^\s<]+[^<.,:;""')\]\s])"); } }
        public override Regex del { get { return new Regex(@"^~~(?=\S)([\s\S]*?\S)~~"); } }
        public override Regex text { get { return new Regex(@"^[\s\S]+?(?=[\\<!\[_*`~]|https?:\/\/| {2,}\n|$)"); } }
    }

    /// <summary>
    /// GFM + Line Breaks Inline Grammar
    /// </summary>
    public class BreaksInlineRules : InlineRules
    {
        public override Regex br { get { return new Regex(@"^ *\n(?!\s*$)"); } }
        public override Regex text { get { return new Regex(@"^[\s\S]+?(?=[\\<!\[_*`~]|https?:\/\/| *\n|$)"); } }
    }
}
