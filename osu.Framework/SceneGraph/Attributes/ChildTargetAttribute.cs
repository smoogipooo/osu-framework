// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using JetBrains.Annotations;

namespace osu.Framework.SceneGraph.Attributes
{
    [MeansImplicitUse]
    public class ChildTargetAttribute : Attribute
    {
        public readonly Type ChildType;
        public readonly string MemberName;

        protected ChildTargetAttribute(Type childType, string memberName)
        {
            ChildType = childType;
            MemberName = memberName;
        }
    }
}
