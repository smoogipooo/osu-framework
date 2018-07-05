using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.SceneGraph.Attributes;

namespace osu.Framework.SceneGraph.Contracts
{
    /// <summary>
    /// Produces the <see cref="Contract"/> for a <see cref="Drawable"/> component in the scene graph.
    /// </summary>
    public class DrawableContract : Contract
    {
        private readonly Drawable drawable;

        /// <summary>
        /// Constructs a new <see cref="DrawableContract"/>.
        /// </summary>
        /// <param name="drawable">The <see cref="Drawable"/> to produce the contract for.</param>
        public DrawableContract(Drawable drawable)
            : base(drawable)
        {
            this.drawable = drawable;
        }

        public override void Build()
        {
            base.Build();

            // Add a dependency on every update method in the drawable
            foreach (var method in drawable.GetType().GetMethods(BINDING_FLAGS))
            {
                if (method.GetCustomAttributes(true).Any(a => a is UpdatesAttribute || a is UpdatesChildAttribute))
                    AddDependency(drawable, method.Name);
            }
        }
    }
}
