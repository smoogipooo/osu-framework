// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface.Markdown;
using osu.Framework.Testing;
using OpenTK.Graphics;

namespace osu.Framework.Tests.Visual
{
    public class TestCaseMarkdownRenderer : TestCase
    {
        public TestCaseMarkdownRenderer()
        {
            var markdown =
@"Testing test testa test `hello world I am c#` abcdef
New line


```
this

is
a

code
block

void main() {
    Console.WriteLine(""Hello World!"");
}
```

# H1

## H2

### H3

#### H4

##### H5

###### H6

[hello](https://upload.wikimedia.org/wikipedia/commons/9/9c/Peppy-hoodie.png)
";

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = new Color4(50, 50, 50, 255)
            });

            Add(new MarkdownTextContainer { Text = markdown });
        }
    }
}
