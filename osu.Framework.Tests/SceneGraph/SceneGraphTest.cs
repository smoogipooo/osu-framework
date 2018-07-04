// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.SceneGraph.Attributes;
using osu.Framework.SceneGraph.Builders;

namespace osu.Framework.Tests.SceneGraph
{
    [TestFixture]
    public class SceneGraphTest
    {
        [Test]
        public void Test2()
        {
            var result = new DrawableDependencyBuilder(new TestComposite()).Build();
        }

        private class TestDrawable : Drawable
        {
            public float Df1;
            public float Df2;
            public float Df3;
            public float Df4;
            public float Df5;
            public float Df6;

            [Updates(nameof(Df1))]
            public void Dm1()
            {
            }

            [DependsOn(nameof(Df1))]
            [Updates(nameof(Df2))]
            public void Dm2()
            {
            }

            [DependsOn(nameof(Df1))]
            [DependsOn(nameof(Df2))]
            [Updates(nameof(Df3))]
            public void Dm3()
            {
            }

            [DependsOn(nameof(Dm1))]
            [Updates(nameof(Df4))]
            public void Dm4()
            {
            }

            [DependsOn(nameof(Df1))]
            [DependsOn(nameof(Df4))]
            [Updates(nameof(Df5))]
            [Updates(nameof(Df6))]
            public void Dm5()
            {
            }
        }

        private class TestComposite : CompositeDrawable
        {
            public float Cf1;
            public float Cf2;

            public TestComposite()
            {
                InternalChild = new TestDrawable();
            }

            [DependsOnChild(typeof(TestDrawable), nameof(TestDrawable.Df1))]
            [Updates(nameof(Cf1))]
            public void Cm1()
            {
            }

            [DependsOnChild(typeof(TestDrawable), nameof(TestDrawable.Dm5))]
            [DependsOn(nameof(Cf1))]
            [Updates(nameof(Cf2))]
            public void Cm2()
            {
            }
        }
    }
}
