// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;

namespace osu.Framework.SceneGraph.Attributes
{
    /// <summary>
    /// Attribute for methods of an object which depend on other members of the same object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class DependsOnAttribute : LocalTargetAttribute
    {
        public DependsOnAttribute(string memberName)
            : base(memberName)
        {
        }
    }
}
