// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;

namespace osu.Framework.Tests.Visual
{
    public class TestCaseCachedModelContainer2 : TestCase
    {
        [Test]
        public void RunTest()
        {
            CachedContainer container = null;
            CacheResolver resolver = null;
            Model model = new Model { Bindable = { Value = 1 } };
            Model model2 = new Model { Bindable = { Value = 3 } };

            AddStep("setup", () =>
            {
                Child = container = new CachedContainer
                {
                    Model = model,
                    Child = resolver = new CacheResolver()
                };
            });


            AddAssert("resolver value == 1", () => resolver.Resolvable.Bindable.Value == 1);

            AddStep("set model value = 2", () => model.Bindable.Value = 2);
            AddAssert("resolver value == 2", () => resolver.Resolvable.Bindable.Value == 2);

            AddStep("change model", () => container.Model = model2);
            AddAssert("resolver value == 3", () => resolver.Resolvable.Bindable.Value == 3);

            AddStep("set old model value = 4", () => model.Bindable.Value = 4);
            AddAssert("resolver value == 3", () => resolver.Resolvable.Bindable.Value == 3);

            AddStep("set new model value = 4", () => model2.Bindable.Value = 4);
            AddAssert("resolver value == 4", () => resolver.Resolvable.Bindable.Value == 4);
        }

        private class CachedContainer : CachedModelContainer2<Model, Resolvable>
        {
        }

        private class CacheResolver : Drawable
        {
            [Resolved]
            public Resolvable Resolvable { get; private set; }
        }

        private class Model
        {
            public readonly Bindable<int> Bindable = new Bindable<int>();
        }

        private class Resolvable : IResolvable<Model>
        {
            public readonly Bindable<int> Bindable = new Bindable<int>();

            public void Unbind(Model model)
            {
                Bindable.UnbindFrom(model.Bindable);
            }

            public void Bind(Model model)
            {
                Bindable.BindTo(model.Bindable);
            }
        }
    }
}
