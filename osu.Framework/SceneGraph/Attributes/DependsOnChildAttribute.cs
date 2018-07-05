// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;

namespace osu.Framework.SceneGraph.Attributes
{
    /// <summary>
    /// Attribute for methods of an object that depend on a member of a child of the object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class DependsOnChildAttribute : ChildTargetAttribute
    {
        public DependsOnChildAttribute(Type childType, string memberName)
            : base(childType, memberName)
        {
        }
    }
}
