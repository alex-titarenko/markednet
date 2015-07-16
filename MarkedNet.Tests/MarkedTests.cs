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


        [TestCase("# Hello World", "<h1 id=\"hello-world\">Hello World</h1>\n")]
        public void Parse_MarkdownText_HtmlText(string source, string expected)
        {
            //action
            var actual = _marked.Parse(source);

            //assert
            Assert.AreEqual(actual, expected);
        }
    }
}
