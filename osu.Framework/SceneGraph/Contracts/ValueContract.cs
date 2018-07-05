using System.Linq;
using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.SceneGraph.Attributes;

namespace osu.Framework.SceneGraph.Contracts
{
    /// <summary>
    /// Produces the <see cref="Contract"/> for a value (field or property) of a <see cref="Drawable"/> component in the scene graph.
    /// </summary>
    public class ValueContract : Contract
    {
        private readonly Drawable owner;
        private readonly MemberInfo member;
        private readonly Contract dependent;

        /// <summary>
        /// Constructs a new <see cref="ValueContract"/> for a field.
        /// </summary>
        /// <param name="owner">The owner of the field.</param>
        /// <param name="field">The field.</param>
        /// <param name="dependent">The <see cref="Contract"/> which depends on this one.</param>
        public ValueContract(Drawable owner, FieldInfo field, Contract dependent)
            : this(owner, (MemberInfo)field, dependent)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ValueContract"/> for a property.
        /// </summary>
        /// <param name="owner">The owner of the property.</param>
        /// <param name="property">The property.</param>
        /// <param name="dependent">The <see cref="Contract"/> which depends on this one.</param>
        public ValueContract(Drawable owner, PropertyInfo property, Contract dependent)
            : this(owner, (MemberInfo)property, dependent)
        {
        }

        private ValueContract(Drawable owner, MemberInfo member, Contract dependent)
            : base(owner)
        {
            this.owner = owner;
            this.member = member;
            this.dependent = dependent;
        }

        public override void Build()
        {
            base.Build();

            // Add a dependency on every parent method which depends on this value
            foreach (var method in owner.GetType().GetMethods(BINDING_FLAGS))
            {
                if (method.GetCustomAttributes(true).OfType<UpdatesAttribute>().Any(a => a.MemberName == member.Name))
                    AddDependency(owner, method.Name);
            }

            // We may have multiple parents (a parenting composite), such that the one that depends on this value is not the owner of this value
            if (dependent.Owner != owner)
            {
                // Add a dependency on every method of the composite which depends on this value
                foreach (var method in dependent.Owner.GetType().GetMethods(BINDING_FLAGS))
                {
                    if (method.GetCustomAttributes(true).OfType<UpdatesChildAttribute>().Any(a => a.MemberName == member.Name))
                        AddDependency(dependent.Owner, method.Name);
                }
            }
        }
    }
}
