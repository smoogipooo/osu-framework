// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Graphics;

namespace osu.Framework.SceneGraph.Contracts
{
    /// <summary>
    /// Represents a contract between the updates of <see cref="Drawable"/>s in the scene graph.
    /// </summary>
    public abstract class Contract
    {
        protected const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public readonly Drawable Owner;

        private readonly List<Contract> dependencies = new List<Contract>();
        private Dictionary<(Drawable, string), Contract> dependencyCache = new Dictionary<(Drawable, string), Contract>();

        private bool visited;
        private bool flattened;

        protected Contract(Drawable owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// Builds the <see cref="Contract"/>s.
        /// </summary>
        public virtual void Build()
        {
        }

        /// <summary>
        /// Produces a topologically-sorted collection of <see cref="MethodContract"/>s by their dependencies.
        /// </summary>
        /// <returns>The <see cref="Contract"/>.</returns>
        public IEnumerable<MethodContract> Flatten()
        {
            if (flattened) // Common dependency
                yield break;

            if (visited) // Cycle
            {
                // Todo: Print more descriptive info
                throw new InvalidOperationException("Cyclic contract dependency.");
            }

            visited = true;

            foreach (var d in dependencies)
            {
                foreach (var f in d.Flatten())
                    yield return f;
            }

            flattened = true;

            if (this is MethodContract methodContract)
                yield return methodContract;
        }

        /// <summary>
        /// Registers a <see cref="Contract"/> dependency which is required for this <see cref="Contract"/> to be fulfilled.
        /// </summary>
        /// <param name="owner">The owner of the member which the contract should target.</param>
        /// <param name="memberName">The member which the contract should target.</param>
        protected void AddDependency(Drawable owner, string memberName) => AddDependency(getDependency(owner, memberName));

        /// <summary>
        /// Registers a <see cref="Contract"/> dependency which is required for this <see cref="Contract"/> to be fulfilled.
        /// </summary>
        /// <param name="contract">The contract to be dependent on.</param>
        protected void AddDependency(Contract contract)
        {
            contract.dependencyCache = dependencyCache;
            contract.Build();

            dependencies.Add(contract);
        }

        private Contract getDependency(Drawable owner, string memberName)
        {
            if (dependencyCache.TryGetValue((owner, memberName), out var existing))
                return existing;

            var memberInfo = owner.GetType().GetMember(memberName, BINDING_FLAGS).Single();

            Contract newContract;

            switch (memberInfo)
            {
                case MethodInfo m:
                    newContract = new MethodContract(owner, m);
                    break;
                case PropertyInfo p:
                    newContract = new ValueContract(owner, p, this);
                    break;
                case FieldInfo f:
                    newContract = new ValueContract(owner, f, this);
                    break;
                default:
                    throw new InvalidOperationException($"Contract could not be created for member {memberName} on type {owner.GetType()}.");
            }

            return dependencyCache[(owner, memberName)] = newContract;
        }
    }
}
