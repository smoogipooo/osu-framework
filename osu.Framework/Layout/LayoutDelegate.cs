// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Framework.Layout
{
    public sealed class LayoutDelegate : LayoutMember
    {
        public LayoutDelegate(InvalidationConditionDelegate invalidationCondition = null)
            : base(Invalidation.All, invalidationCondition)
        {
        }
    }
}
