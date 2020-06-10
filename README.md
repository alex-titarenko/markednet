# Marked .NET
[![Build status](https://ci.appveyor.com/api/projects/status/2kmxq916vw9ytjik?svg=true)](https://ci.appveyor.com/project/alex-titarenko/markednet) !
[![NuGet Version](http://img.shields.io/nuget/v/MarkedNet.svg?style=flat)](https://www.nuget.org/packages/MarkedNet/) [![NuGet Downloads](http://img.shields.io/nuget/dt/MarkedNet.svg?style=flat)](https://www.nuget.org/packages/MarkedNet/)

A full-featured markdown parser and compiler, written in C#. Port of original [marked](https://github.com/chjj/marked) javascript project.

## Example of usage
Here is an example of typical using of library:

```C#
var markdown = @"
Heading
=======

Sub-heading
-----------

### Another deeper heading

Paragraphs are separated
by a blank line.

Leave 2 spaces at the end of a line to do a  
line break

Text attributes *italic*, **bold**,
`monospace`, ~~strikethrough~~ .

A [link](http://example.com).

Shopping list:

* apples
* oranges
* pears

Numbered list:

1. apples
2. oranges
3. pears
";

var marked = new Marked();
var html = marked.Parse(markdown);
```

Output:
```html
<h1 id="heading">Heading</h1>
<h2 id="sub-heading">Sub-heading</h2>
<h3 id="another-deeper-heading">Another deeper heading</h3>
<p>Paragraphs are separated
by a blank line.</p>
<p>Leave 2 spaces at the end of a line to do a<br>line break</p>
<p>Text attributes <em>italic</em>, <strong>bold</strong>,
<code>monospace</code>, <del>strikethrough</del> .</p>
<p>A <a href="http://example.com">link</a>.
</p>
<p>Shopping list:</p>
<ul>
<li>apples</li>
<li>oranges</li>
<li>pears</li>
</ul>
<p>Numbered list:</p>
<ol>
<li>apples</li>
<li>oranges</li>
<li>pears</li>
</ol>
```


## Get it on NuGet!

    Install-Package MarkedNet

## License
MarkedNet is under the [MIT license](LICENSE.md).
