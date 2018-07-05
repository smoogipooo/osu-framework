// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.SceneGraph.Attributes;
using osu.Framework.SceneGraph.Contracts;

namespace osu.Framework.Tests.SceneGraph
{
    [TestFixture]
    public class SceneGraphTest
    {
        [Test]
        public void Test2()
        {
            var contract = new CompositeDrawableContract(new TestComposite());
            contract.Build();

            var result = contract.Flatten().ToList();
        }

        private class TestDrawable : Drawable
        {
            public float Df1;
            public float Df2;
            public float Df3;
            public float Df4;
            public float Df5;
            public float Df6;
            public float Df7;

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
            public float Cf3;

            public TestComposite()
            {
                InternalChildren = new[]
                {
                    new TestDrawable(),
                    new TestDrawable(),
                };
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

            [UpdatesChild(typeof(TestDrawable), nameof(TestDrawable.Df7))]
            public void Cm3()
            {
            }

            [DependsOnChild(typeof(TestDrawable), nameof(TestDrawable.Df7))]
            [DependsOnChild(typeof(TestDrawable), nameof(TestDrawable.Df6))]
            [Updates(nameof(Cf3))]
            public void Cm4()
            {
            }
        }
    }
}
