// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.IO.Stores;
using osuTK;

namespace osu.Framework
{
    public class TruncatingTextBuilder : TextBuilder
    {
        private readonly TextBuilder ellipsisBuilder;
        private readonly List<EllipsisGlyph> ellipsisGlyphs;

        public TruncatingTextBuilder(float fontSize, float maxWidth, bool useFullGlyphHeight = true, Vector2 startOffset = default, Vector2 spacing = default)
            : base(fontSize, maxWidth, useFullGlyphHeight, startOffset, spacing)
        {
            ellipsisBuilder = new TextBuilder(fontSize, float.MaxValue, useFullGlyphHeight, startOffset, spacing);
            ellipsisGlyphs = new List<EllipsisGlyph>();
        }

        public void AddEllipsisCharacter(FontStore.CharacterGlyph glyph, float? widthOverride)
        {
            ellipsisBuilder.AddCharacter(glyph, widthOverride);
            ellipsisGlyphs.Add(new EllipsisGlyph(glyph, widthOverride));
        }

        private bool widthExceededOnce;
        private bool addingEllipsis;

        protected override bool CanAddCharacters => base.CanAddCharacters && (!widthExceededOnce || addingEllipsis);

        protected override void OnWidthExceeded()
        {
            if (widthExceededOnce)
                return;

            widthExceededOnce = true;
            addingEllipsis = true;

            // Remove characters by backtracking until both of the following conditions are met:
            // 1. The ellipsis glyphs can be added without exceeding the text bounds
            // 2. The last character in the builder is not a whitespace (for visual niceness)
            // Or until there are no more non-ellipsis characters in the builder (if the ellipsis string is very long)

            do
            {
                RemoveLastCharacter();

                if (Characters.Count == 0)
                    break;
            } while (Characters[Characters.Count - 1].Glyph.IsWhiteSpace || !HasAvailableSpace(ellipsisBuilder.CurrentPos.X));

            // Add the ellipsis characters
            foreach (var g in ellipsisGlyphs)
                AddCharacter(g.Glyph, g.WidthOverride);

            addingEllipsis = false;
        }

        private readonly struct EllipsisGlyph
        {
            public readonly FontStore.CharacterGlyph Glyph;
            public readonly float? WidthOverride;

            public EllipsisGlyph(FontStore.CharacterGlyph glyph, float? widthOverride)
            {
                Glyph = glyph;
                WidthOverride = widthOverride;
            }
        }
    }
}
