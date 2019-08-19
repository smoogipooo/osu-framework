// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Caching;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osuTK;

namespace osu.Framework.Text
{
    public sealed class TruncatingTextBuilder : TextBuilder
    {
        private readonly char[] neverFixedWidthCharacters;
        private readonly char fallbackCharacter;
        private readonly ITexturedGlyphLookupStore store;
        private readonly FontUsage font;
        private readonly string ellipsisString;
        private readonly bool useFontSizeAsHeight;
        private readonly Vector2 spacing;

        /// <summary>
        /// Creates a new <see cref="TextBuilder"/>.
        /// </summary>
        /// <param name="store">The store from which glyphs are to be retrieved from.</param>
        /// <param name="font">The font to use for glyph lookups from <paramref name="store"/>.</param>
        /// <param name="ellipsisString">The string to be displayed if the text exceeds the allowable text area.</param>
        /// <param name="useFontSizeAsHeight">True to use the provided <see cref="font"/> size as the height for each line. False if the height of each individual glyph should be used.</param>
        /// <param name="startOffset">The offset at which characters should begin being added at.</param>
        /// <param name="spacing">The spacing between characters.</param>
        /// <param name="maxWidth">The maximum width of the resulting text bounds.</param>
        /// <param name="characterList">That list to contain all resulting <see cref="TextBuilderGlyph"/>s.</param>
        /// <param name="neverFixedWidthCharacters">The characters for which fixed width should never be applied.</param>
        /// <param name="fallbackCharacter">The character to use if a glyph lookup fails.</param>
        public TruncatingTextBuilder(ITexturedGlyphLookupStore store, FontUsage font, float maxWidth, string ellipsisString = null, bool useFontSizeAsHeight = true, Vector2 startOffset = default,
                                     Vector2 spacing = default, List<TextBuilderGlyph> characterList = null, char[] neverFixedWidthCharacters = null, char fallbackCharacter = '?')
            : base(store, font, maxWidth, useFontSizeAsHeight, startOffset, spacing, characterList, neverFixedWidthCharacters, fallbackCharacter)
        {
            this.store = store;
            this.font = font;
            this.ellipsisString = ellipsisString;
            this.useFontSizeAsHeight = useFontSizeAsHeight;
            this.spacing = spacing;
            this.neverFixedWidthCharacters = neverFixedWidthCharacters;
            this.fallbackCharacter = fallbackCharacter;
        }

        private bool widthExceededOnce;
        private bool addingEllipsis;

        protected override bool CanAddCharacters => base.CanAddCharacters && (!widthExceededOnce || addingEllipsis);

        protected override void OnWidthExceeded()
        {
            if (widthExceededOnce)
                return;

            widthExceededOnce = true;

            if (string.IsNullOrEmpty(ellipsisString))
                return;

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
            } while (Characters[Characters.Count - 1].IsWhiteSpace() || !HasAvailableSpace(getEllipsisSize().X));

            AddText(ellipsisString);

            addingEllipsis = false;
        }

        private readonly Cached<Vector2> ellipsisSizeCache = new Cached<Vector2>();

        private Vector2 getEllipsisSize()
        {
            if (ellipsisSizeCache.IsValid)
                return ellipsisSizeCache.Value;

            var builder = new TextBuilder(store, font, float.MaxValue, useFontSizeAsHeight, Vector2.Zero, spacing, null, neverFixedWidthCharacters, fallbackCharacter);

            builder.AddText(ellipsisString);

            return ellipsisSizeCache.Value = builder.TextSize;
        }
    }
}
