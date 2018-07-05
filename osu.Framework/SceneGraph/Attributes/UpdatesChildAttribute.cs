// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;

namespace osu.Framework.SceneGraph.Attributes
{
    /// <summary>
    /// Attribute for members of an object which may change the value of members of child objects.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UpdatesChildAttribute : ChildTargetAttribute
    {
        public UpdatesChildAttribute(Type childType, string memberName)
            : base(childType, memberName)
        {
        }
    }
}
