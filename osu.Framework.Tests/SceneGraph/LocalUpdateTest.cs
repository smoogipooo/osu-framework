// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.SceneGraph.Attributes;
using osu.Framework.SceneGraph.Contracts;

namespace osu.Framework.Tests.SceneGraph
{
    /// <summary>
    /// A suite of tests for <see cref="UpdatesAttribute"/>.
    /// </summary>
    [TestFixture]
    public class ContractLocalUpdateTest
    {
        /// <summary>
        /// Tests that an update method fires.
        /// </summary>
        [Test]
        public void TestSingleFieldUpdate()
        {
            var target = new SingleUpdateDrawable();
            executeContract(new DrawableContract(target));

            Assert.AreEqual(1, target.Field1);
        }

        /// <summary>
        /// Tests that multiple disjoint update methods fire.
        /// </summary>
        [Test]
        public void TestMultipleFieldUpdate()
        {
            var target = new MultipleUpdateDrawable();
            executeContract(new DrawableContract(target));

            Assert.AreEqual(1, target.Field1);
            Assert.AreEqual(2, target.Field2);
        }

        /// <summary>
        /// Tests that multiple joint updates fire only once.
        /// </summary>
        [Test]
        public void TestJoinedMultipleFieldUpdate()
        {
            var target = new JoinedMultipleUpdateDrawable();
            executeContract(new DrawableContract(target));

            Assert.AreEqual(1, target.Field1);
            Assert.AreEqual(1, target.Field2);
        }

        private void executeContract(Contract contract)
        {
            contract.Build();
            foreach (var m in contract.Flatten())
                m.Invoke();
        }

        private class SingleUpdateDrawable : Drawable
        {
            public int Field1 { get; private set; }

            private int invocation;

            [Updates(nameof(Field1))]
            private void update() => Field1 = ++invocation;
        }

        private class MultipleUpdateDrawable : Drawable
        {
            public int Field1 { get; private set; }
            public int Field2 { get; private set; }

            private int invocation;

            [Updates(nameof(Field1))]
            private void update1() => Field1 = ++invocation;

            [Updates(nameof(Field2))]
            private void update2() => Field2 = ++invocation;
        }

        private class JoinedMultipleUpdateDrawable : Drawable
        {
            public int Field1 { get; private set; }
            public int Field2 { get; private set; }

            private int invocation;

            [Updates(nameof(Field1))]
            [Updates(nameof(Field2))]
            private void update()
            {
                Field1 = Field2 = ++invocation;
            }
        }
    }
}
