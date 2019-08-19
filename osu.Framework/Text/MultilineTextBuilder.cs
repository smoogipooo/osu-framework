// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osuTK;

namespace osu.Framework.Text
{
    public sealed class MultilineTextBuilder : TextBuilder
    {
        public MultilineTextBuilder(ITexturedGlyphLookupStore store, FontUsage font, float maxWidth, bool useFontSizeAsHeight = true, Vector2 startOffset = default, Vector2 spacing = default,
                                    List<TextBuilderGlyph> characterList = null)
            : base(store, font, maxWidth, useFontSizeAsHeight, startOffset, spacing, characterList)
        {
        }

        protected override void OnWidthExceeded() => AddNewLine();
    }
}
