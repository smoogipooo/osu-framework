// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.SceneGraph.Attributes;

namespace osu.Framework.SceneGraph.Builders
{
    public class DrawableDependencyBuilder : IDependencyBuilder
    {
        private readonly HashSet<Type> types = new HashSet<Type>();

        public DrawableDependencyBuilder(CompositeDrawable composite)
        {
            traverseTypes(composite);

            void traverseTypes(Drawable d)
            {
                types.Add(d.GetType());

                if (d is CompositeDrawable c)
                {
                    foreach (var n in c.InternalChildren)
                        traverseTypes(n);
                }
            }
        }

        public List<UpdateInfo> Build()
        {
            var nodes = new List<Node>();

            // Build nodes
            foreach (var m in generateUpdateInfos())
                nodes.Add(new Node(m));

            // Build dependency graph
            foreach (var node in nodes)
            foreach (var dependency in node.Info.Dependencies)
                node.Dependencies.AddRange(nodes.Where(n => n.Info.Targets.Contains(dependency) || n.Info.Method == dependency));

            // Generate the update chain
            return generateUpdateChain(nodes);
        }

        private IEnumerable<UpdateInfo> generateUpdateInfos()
        {
            foreach (var t in types)
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.GetCustomAttributes(true).OfType<IUpdatesAttribute>().Any()))
                yield return new UpdateInfo(t, m);
        }

        private List<UpdateInfo> generateUpdateChain(List<Node> nodes)
        {
            var updateChain = new List<UpdateInfo>();

            foreach (var n in nodes)
            {
                if (n.Added)
                    continue;

                updateChain.AddRange(visit(n));
            }

            return updateChain;

            // Visits a node and its children in topological order
            IEnumerable<UpdateInfo> visit(Node node)
            {
                if (node.Added) // Common dependency
                    yield break;

                if (node.Visited) // Cycle
                {
                    // Todo: Print more descriptive info
                    throw new InvalidOperationException($"Graph has a cyclic dependency: {node.Info.Method.Name}.");
                }

                node.Visited = true;

                foreach (var dependency in node.Dependencies)
                foreach (var dependentInfo in visit(dependency))
                    yield return dependentInfo;

                yield return node.Info;

                node.Added = true;
            }
        }

        private class Node
        {
            public bool Visited;
            public bool Added;

            public readonly List<Node> Dependencies = new List<Node>();
            public readonly UpdateInfo Info;

            public Node(UpdateInfo info)
            {
                Info = info;
            }
        }
    }
}
