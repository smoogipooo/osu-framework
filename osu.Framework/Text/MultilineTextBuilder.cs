// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osuTK;

namespace osu.Framework.Text
{
    public class MultilineTextBuilder : TextBuilder
    {
        public MultilineTextBuilder(IGlyphLookupStore store, FontUsage font, float maxWidth, bool useFontSizeAsHeight = true, Vector2 startOffset = default, Vector2 spacing = default)
            : base(store, font, maxWidth, useFontSizeAsHeight, startOffset, spacing)
        {
        }

        protected override void OnWidthExceeded() => AddNewLine();
    }
}
