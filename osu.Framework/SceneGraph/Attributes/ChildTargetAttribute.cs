// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Linq;
using System.Reflection;

namespace osu.Framework.SceneGraph.Attributes
{
    public class ChildTargetAttribute : Attribute
    {
        public readonly Type ChildType;
        public readonly string MemberName;

        protected ChildTargetAttribute(Type childType, string memberName)
        {
            ChildType = childType;
            MemberName = memberName;
        }

        public MemberInfo GetMember() => ChildType.GetMember(MemberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
    }
}
