using System;
using System.Linq;
using System.Reflection;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.SceneGraph.Attributes;

namespace osu.Framework.SceneGraph.Contracts
{
    /// <summary>
    /// Produces the <see cref="Contract"/> for an update method of a <see cref="Drawable"/> component in the scene graph.
    /// </summary>
    public class MethodContract : Contract
    {
        private readonly Drawable owner;
        private readonly MethodInfo method;

        /// <summary>
        /// Constructs a new <see cref="MethodContract"/>.
        /// </summary>
        /// <param name="owner">The owner of the method.</param>
        /// <param name="method">The method.</param>
        public MethodContract(Drawable owner, MethodInfo method)
            : base(owner)
        {
            this.owner = owner;
            this.method = method;
        }

        public override void Build()
        {
            base.Build();

            // Add a dependency on every local member which the method depends on
            foreach (var attribute in method.GetCustomAttributes(true).OfType<DependsOnAttribute>())
                AddDependency(owner, attribute.MemberName);

            if (!(owner is CompositeDrawable composite)) return;

            // Add a dependency on every child member which the method depends on
            foreach (var attribute in method.GetCustomAttributes(true).OfType<DependsOnChildAttribute>())
            {
                foreach (var child in composite.InternalChildren.Where(c => c.GetType().IsSubclassOrTypeOf(attribute.ChildType)))
                    AddDependency(child, attribute.MemberName);
            }
        }

        /// <summary>
        /// Invokes the method.
        /// </summary>
        public void Invoke() => method.Invoke(owner, Array.Empty<object>());
    }
}
