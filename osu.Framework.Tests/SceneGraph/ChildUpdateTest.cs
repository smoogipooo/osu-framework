// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.SceneGraph.Attributes;
using osu.Framework.SceneGraph.Contracts;

namespace osu.Framework.Tests.SceneGraph
{
    /// <summary>
    /// A suite of tests for <see cref="UpdatesChildAttribute"/>.
    /// </summary>
    [TestFixture]
    public class ContractChildUpdateTest
    {
        /// <summary>
        /// Tests that an update child method fires.
        /// </summary>
        [Test]
        public void TestSingleChildUpdate()
        {
            var target = new SingleChildUpdateComposite();
            executeContract(new CompositeDrawableContract(target));

            Assert.AreEqual(1, target.Child.Field1);
        }

        /// <summary>
        /// Tests that an update child method fires even if the attribute references an incorrect child type.
        /// This is allowed because _all_ update methods must fire, and it is to the user's discretion to make sure only the specified type is updated.
        /// </summary>
        [Test]
        public void TestSingleChildUpdateViaBaseType()
        {
            var target = new SingleChildUpdateViaBaseTypeComposite();
            executeContract(new CompositeDrawableContract(target));

            Assert.AreEqual(1, target.Child.Field1);
        }

        /// <summary>
        /// Tests that an update child method fires even if the attribute references an incorrect type.
        /// This is allowed because _all_ update methods must fire, and it is to the user's discretion to make sure only the specified type is updated.
        /// </summary>
        [Test]
        public void TestSingleChildUpdateViaDerivedType()
        {
            var target = new SingleChildUpdateViaDerivedTypeComposite();
            executeContract(new CompositeDrawableContract(target));

            Assert.AreEqual(1, target.Child.Field1);
        }

        /// <summary>
        /// Tests that a child's update method is invoked.
        /// </summary>
        [Test]
        public void TestSingleChildNestedUpdate()
        {
            var target = new SingleChildNestedUpdateComposite();
            executeContract(new CompositeDrawableContract(target));

            Assert.AreEqual(1, target.Child.Field1);
        }

        /// <summary>
        /// Tests that every child's update method is invoked.
        /// </summary>
        [Test]
        public void TestMultipleChildNestedUpdate()
        {
            var target = new MultipleChildUpdateComposite();
            executeContract(new CompositeDrawableContract(target));

            foreach (var child in target.Children)
                Assert.AreEqual(1, child.Field1);
        }

        /// <summary>
        /// Tests that the update method of a child nested within two composites is invoked.
        /// </summary>
        [Test]
        public void TestDoublyNestedChildUpdate()
        {
            var target = new DoubleNestedChildUpdateComposite();
            executeContract(new CompositeDrawableContract(target));

            Assert.AreEqual(1, target.Child.Field1);
        }

        private void executeContract(Contract contract)
        {
            contract.Build();
            foreach (var m in contract.Flatten())
                m.Invoke();
        }

        private class SingleChildUpdateComposite : CompositeDrawable
        {
            public readonly SingleFieldChild Child;

            private int invocation;

            [UpdatesChild(typeof(SingleFieldChild), nameof(SingleFieldChild.Field1))]
            private void update() => Child.Field1 = ++invocation;

            public SingleChildUpdateComposite() => InternalChild = Child = new SingleFieldChild();
        }

        private class SingleChildUpdateViaBaseTypeComposite : CompositeDrawable
        {
            public readonly SingleFieldChild Child;

            private int invocation;

            [UpdatesChild(typeof(Drawable), nameof(SingleFieldChild.Field1))]
            private void update() => Child.Field1 = ++invocation;

            public SingleChildUpdateViaBaseTypeComposite() => InternalChild = Child = new SingleFieldChild();
        }

        private class SingleChildUpdateViaDerivedTypeComposite : CompositeDrawable
        {
            public readonly SingleFieldChild Child;

            private int invocation;

            [UpdatesChild(typeof(LocalDerivedChild), nameof(SingleFieldChild.Field1))]
            private void update() => Child.Field1 = ++invocation;

            public SingleChildUpdateViaDerivedTypeComposite() => InternalChild = Child = new SingleFieldChild();

            private class LocalDerivedChild : SingleFieldChild
            {
            }
        }

        private class SingleChildNestedUpdateComposite : CompositeDrawable
        {
            public readonly SingleFieldUpdatingChild Child;

            public SingleChildNestedUpdateComposite() => InternalChild = Child = new SingleFieldUpdatingChild();
        }

        private class MultipleChildUpdateComposite : CompositeDrawable
        {
            public IEnumerable<SingleFieldUpdatingChild> Children => InternalChildren.Select(c => (SingleFieldUpdatingChild)c);

            public MultipleChildUpdateComposite()
            {
                for (int i = 0; i < 10; i++)
                    AddInternal(new SingleFieldUpdatingChild());
            }
        }

        private class DoubleNestedChildUpdateComposite : CompositeDrawable
        {
            public readonly SingleFieldUpdatingChild Child;

            public DoubleNestedChildUpdateComposite() => InternalChild = new NestedComposite(Child = new SingleFieldUpdatingChild());

            private class NestedComposite : CompositeDrawable
            {
                public NestedComposite(SingleFieldUpdatingChild child) => InternalChild = child;
            }
        }

        public class SingleFieldChild : Drawable
        {
            public int Field1 { get; set; }
        }

        public class SingleFieldUpdatingChild : Drawable
        {
            public int Field1 { get; set; }

            private int invocation;

            [Updates(nameof(Field1))]
            private void update() => Field1 = ++invocation;
        }
    }
}
