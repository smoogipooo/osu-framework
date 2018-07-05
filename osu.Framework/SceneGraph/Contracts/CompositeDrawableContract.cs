using osu.Framework.Graphics.Containers;

namespace osu.Framework.SceneGraph.Contracts
{
    /// <summary>
    /// Produces the <see cref="Contract"/> for a <see cref="CompositeDrawable"/> component in the scene graph.
    /// </summary>
    public class CompositeDrawableContract : DrawableContract
    {
        private readonly CompositeDrawable composite;

        /// <summary>
        /// Constructs a new <see cref="CompositeDrawableContract"/>.
        /// </summary>
        /// <param name="composite">The <see cref="CompositeDrawable"/> to produce the contract for.</param>
        public CompositeDrawableContract(CompositeDrawable composite)
            : base(composite)
        {
            this.composite = composite;
        }

        public override void Build()
        {
            base.Build();

            // Add a dependency on every child in the composite
            foreach (var drawable in composite.InternalChildren)
                AddDependency(new DrawableContract(drawable));
        }
    }
}
