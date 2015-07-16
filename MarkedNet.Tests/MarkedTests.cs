using System;
using NUnit;
using NUnit.Framework;


namespace MarkedNet.Tests
{
    [TestFixture]
    public class MarkedTests
    {
        private Marked _marked;


        [SetUp]
        public virtual void SetUp()
        {
            _marked = new Marked();
        }

        [Test]
        public void Parse_Null_Null()
        {
            //action
            var actual = _marked.Parse(null);

            //assert
            Assert.IsNull(actual);
        }

        [Test]
        public void Parse_EmptyString_EmptyString()
        {
            //action
            var actual = _marked.Parse(String.Empty);

            //assert
            Assert.IsEmpty(actual);
        }
        
        [TestCase("# Hello World", "<h1 id=\"hello-world\">Hello World</h1>\n")]
        public void Parse_MarkdownText_HtmlText(string source, string expected)
        {
            //action
            var actual = _marked.Parse(source);

            //assert
            Assert.AreEqual(actual, expected);
        }

        [TestCase("Hot keys: <kbd>Ctrl+[</kbd> and <kbd>Ctrl+]</kbd>", "<p>Hot keys: <kbd>Ctrl+[</kbd> and <kbd>Ctrl+]</kbd></p>\n")]
        [TestCase("<div>Some text here</div>", "<div>Some text here</div>")]
        public void Parse_MarkdownWithHtmlTags_HtmlText(string source, string expected)
        {
            //action
            var actual = _marked.Parse(source);

            //assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Parse_MarkdownTableWithMissingAlign_HtmlText()
        {
            //arrange
            var table = @"|                  | Header1                        | Header2              |
 ----------------- | ---------------------------- | ------------------
| Single backticks | `'Isn't this fun?'`            | 'Isn't this fun?' |
| Quotes           | `""Isn't this fun?""`            | ""Isn't this fun?"" |
| Dashes           | `-- is en-dash, --- is em-dash` | -- is en-dash, --- is em-dash |";


            var expected = "<table>\n<thead>\n<tr>\n<th></th>\n<th></th>\n<th>Header1</th>\n<th>Header2</th>\n</tr>\n</thead>\n<tbody>\n<tr>\n<td></td>\n<td>Single backticks</td>\n<td><code>&#39;Isn&#39;t this fun?&#39;</code></td>\n<td>&#39;Isn&#39;t this fun?&#39;</td>\n<td></td>\n</tr>\n<tr>\n<td></td>\n<td>Quotes</td>\n<td><code>&quot;Isn&#39;t this fun?&quot;</code></td>\n<td>&quot;Isn&#39;t this fun?&quot;</td>\n<td></td>\n</tr>\n<tr>\n<td></td>\n<td>Dashes</td>\n<td><code>-- is en-dash, --- is em-dash</code></td>\n<td>-- is en-dash, --- is em-dash</td>\n<td></td>\n</tr>\n</tbody>\n</table>\n";

            //action
            var actual = _marked.Parse(table);

            //assert
            Assert.AreEqual(expected, actual);

        }
    }
}
