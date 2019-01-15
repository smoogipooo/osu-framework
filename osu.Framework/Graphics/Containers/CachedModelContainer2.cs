// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Lists;

namespace osu.Framework.Graphics.Containers
{
    public class CachedModelContainer2<TModel, TResolvable> : Container
        where TResolvable : class, IResolvable<TModel>, new()
    {
        private TModel model;

        public TModel Model
        {
            get => model;
            set
            {
                if (EqualityComparer<TModel>.Default.Equals(model))
                    return;

                if (model != null)
                    OurResolvable.Unbind(model);

                model = value;

                if (model != null)
                    OurResolvable.Bind(model);

                if (dependencies != null)
                    dependencies.Model = model;
            }
        }

        public TResolvable OurResolvable { get; } = new TResolvable();

        private ResolvingDependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new ResolvingDependencyContainer(base.CreateChildDependencies(parent)) { Model = Model };

        private class ResolvingDependencyContainer : IReadOnlyDependencyContainer
        {
            private TModel model;

            public TModel Model
            {
                get => model;
                set
                {
                    if (model != null)
                        boundResolvables.ForEachAlive(b => b.Unbind(model));

                    model = value;

                    if (model != null)
                        boundResolvables.ForEachAlive(b => b.Bind(model));
                }
            }

            private readonly WeakList<TResolvable> boundResolvables = new WeakList<TResolvable>();

            private readonly IReadOnlyDependencyContainer parent;

            public ResolvingDependencyContainer(IReadOnlyDependencyContainer parent)
            {
                this.parent = parent;
            }

            public object Get(Type type)
            {
                if (type == typeof(TResolvable))
                    return createResolvable();

                return parent.Get(type);
            }

            public object Get(Type type, CacheInfo info)
            {
                if (type == typeof(TResolvable))
                    return createResolvable();

                return parent.Get(type, info);
            }

            private TResolvable createResolvable()
            {
                var resolvable = new TResolvable();

                if (Model != null)
                    resolvable.Bind(Model);

                boundResolvables.Add(resolvable);

                return resolvable;
            }

            public void Inject<T>(T instance) where T : class
                => DependencyActivator.Activate(instance, this);
        }
    }
}
