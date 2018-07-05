// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;

namespace osu.Framework.SceneGraph.Attributes
{
    /// <summary>
    /// Attribute for methods of an object which may change the value of members within the object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UpdatesAttribute : LocalTargetAttribute
    {
        public UpdatesAttribute(string memberName)
            : base(memberName)
        {
        }
    }
}
