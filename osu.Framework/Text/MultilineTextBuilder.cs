// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Framework.Text
{
    public class MultilineTextBuilder : TextBuilder
    {
        public MultilineTextBuilder(float fontSize, float maxWidth, bool useFullGlyphHeight = true, Vector2 startOffset = default, Vector2 spacing = default)
            : base(fontSize, maxWidth, useFullGlyphHeight, startOffset, spacing)
        {
        }

        protected override void OnWidthExceeded() => AddNewLine();
    }
}
