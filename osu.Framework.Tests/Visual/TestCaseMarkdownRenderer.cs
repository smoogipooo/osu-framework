﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
@"
Seven days, sixty-five incredible entries, and only six votes to spend. Help us decide how we decorate our main menu for this year's festive season - vote for your favourite fanart entry now!

[![](https://assets.ppy.sh/contests/58/header.jpg?20171127)](https://osu.ppy.sh/community/contests/58)

As usual, our absurdly talented community artists have made the unbelievable happen and created a winter wonderland of entries for you to agonize over. With only **six** votes at your disposal, which of these incredible creations is your favourite?

A selection of the top-voted entries will make it to the main menu as winter seasonal backgrounds, remaining to usher in holiday cheer until the new year. The artists responsible for such magnificent work will also receive **2 months of osu!supporter** for themselves or a friend, or two friends!

<video width=""640"" height=""360"" controls>
<source src=""https://assets.ppy.sh/contests/58/socmedia/720p.mp4"" type=""video/mp4"">
</video>

*(Enjoy a little tease of a brand new, osu! original winter-themed track coming soon to a client near you!)*

[Head on over now and vote!](https://osu.ppy.sh/community/contests/58)

The voting will conclude in **7 days time from the date of this post** - there's also a handy little timer on the page itself if you're unsure.

—Ephemeral

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
