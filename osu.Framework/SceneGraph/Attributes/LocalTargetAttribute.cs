// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using JetBrains.Annotations;

namespace osu.Framework.SceneGraph.Attributes
{
    [MeansImplicitUse]
    public abstract class LocalTargetAttribute : Attribute
    {
        public readonly string MemberName;

        protected LocalTargetAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
}
