// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using OpenTK;

namespace osu.Framework.Tests.Visual
{
    public class TestCaseOccluder : TestCase
    {
        [BackgroundDependencyLoader]
        private void load(FrameworkDebugConfigManager config)
        {
            Checkbox checkBox;
            Child = checkBox = new BasicCheckbox { LabelText = "Depth testing" };

            config.BindWith(DebugSetting.DepthTesting, checkBox.Current);

            Add(new Occluder
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500)
            });

            for (int i = 0; i < 1024; i++)
            {
                Add(new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500)
                });
            }

            Add(new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500)
            });
        }
    }
}
