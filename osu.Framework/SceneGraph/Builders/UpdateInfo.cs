// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.SceneGraph.Attributes;

namespace osu.Framework.SceneGraph.Builders
{
    public class UpdateInfo
    {
        public readonly List<MemberInfo> Targets = new List<MemberInfo>();
        public readonly List<MemberInfo> Dependencies = new List<MemberInfo>();

        public readonly MethodInfo Method;

        public UpdateInfo(MethodInfo method)
        {
            Method = method;

            // Add all update targets from the target type
            foreach (var updateTarget in method.GetCustomAttributes(true).OfType<UpdatesAttribute>())
                Targets.Add(updateTarget.GetMember(method.DeclaringType));

            // Add all update dependencies from the target type
            foreach (var dependency in method.GetCustomAttributes(true).OfType<DependsOnAttribute>())
                Dependencies.Add(dependency.GetMember(method.DeclaringType));

            // Add all update targets from child types
            foreach (var childUpdateTarget in method.GetCustomAttributes(true).OfType<UpdatesChildAttribute>())
                Targets.Add(childUpdateTarget.GetMember());

            // Add all update dependencies from child types
            foreach (var childDependency in method.GetCustomAttributes(true).OfType<DependsOnChildAttribute>())
                Dependencies.Add(childDependency.GetMember());
        }
    }
}
