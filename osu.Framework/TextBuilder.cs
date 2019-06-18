// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osuTK;

namespace osu.Framework
{
    /// <summary>
    /// A text builder for <see cref="SpriteText"/> and other text-based display components.
    /// </summary>
    public class TextBuilder
    {
        public readonly List<SpriteText.CharacterPart> Characters = new List<SpriteText.CharacterPart>();

        private FontStore.CharacterGlyph? lastGlyph => Characters.Count == 0 ? null : (FontStore.CharacterGlyph?)Characters[Characters.Count - 1].Glyph;

        private readonly float fontSize;
        private readonly bool useFullGlyphHeight;
        private readonly Vector2 startOffset;
        private readonly Vector2 spacing;
        private readonly float maxWidth;

        private bool multiline;
        private bool truncate;
        private float ellipsisLength;
        private FontStore.CharacterGlyph[] ellipsisGlyphs;

        private Vector2 currentPos;
        private float currentRowHeight;
        private bool canAddCharacters = true;

        /// <summary>
        /// Creates a new <see cref="TextBuilder"/>.
        /// </summary>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="useFullGlyphHeight">True to use <paramref name="fontSize"/> as the height for each line. False if the height of each individual glyph should be used.</param>
        /// <param name="startOffset">The offset at which characters should begin being added at.</param>
        /// <param name="spacing">The spacing between characters.</param>
        /// <param name="maxWidth">The maximum width of the resulting text bounds.</param>
        public TextBuilder(float fontSize, bool useFullGlyphHeight = true, Vector2 startOffset = default, Vector2 spacing = default, float maxWidth = float.MaxValue)
        {
            this.fontSize = fontSize;
            this.useFullGlyphHeight = useFullGlyphHeight;
            this.startOffset = startOffset;
            this.spacing = spacing;
            this.maxWidth = maxWidth;

            currentPos = startOffset;
        }

        /// <summary>
        /// Disables multi-lining and causes an ellipsis string to be displayed when characters exceed the text bounds.
        /// </summary>
        /// <param name="ellipsisGlyphs">The glyphs that make up the ellipsis.</param>
        public void SetEllipsis(FontStore.CharacterGlyph[] ellipsisGlyphs)
        {
            this.ellipsisGlyphs = ellipsisGlyphs;

            multiline = false;
            truncate = true;

            var builder = new TextBuilder(fontSize);
            foreach (var glyph in ellipsisGlyphs)
                builder.AddCharacter(glyph);

            // Todo: Is this correct? Shouldn't it be the texture width?
            ellipsisLength = builder.currentPos.X;
        }

        /// <summary>
        /// Disables truncation and enables multi-lining when characters exceed the text bounds.
        /// </summary>
        public void SetMultiline()
        {
            truncate = false;
            multiline = true;
        }

        /// <summary>
        /// Adds a character to this <see cref="TextBuilder"/>.
        /// </summary>
        /// <param name="glyph">The glyph of the character to add.</param>
        public void AddCharacter(FontStore.CharacterGlyph glyph) => addCharacter(glyph, false);

        private void addCharacter(FontStore.CharacterGlyph glyph, bool bypassAreaChecks)
        {
            if (!canAddCharacters)
                return;

            glyph.ApplyScaleAdjust(fontSize);

            // Add the kerning. This is _not_ considered a draw-time offset - it affects the position of all further drawn characters.
            if (lastGlyph != null)
                currentPos.X += glyph.GetKerning(lastGlyph.Value);

            // Check for multiline/truncation
            if (!bypassAreaChecks)
            {
                ensureAreaAvailable(glyph);

                if (!canAddCharacters)
                    return;
            }

            // Add the character
            if (!glyph.IsWhiteSpace)
            {
                Characters.Add(new SpriteText.CharacterPart
                {
                    Glyph = glyph,
                    Texture = glyph.Texture,
                    DrawRectangle = new RectangleF(new Vector2(currentPos.X + glyph.XOffset, currentPos.Y + glyph.YOffset),
                        new Vector2(glyph.Width * fontSize, glyph.Height * fontSize)),
                });
            }

            // Move the current position
            currentPos.X += glyph.XAdvance;
        }

        /// <summary>
        /// Removes the last character from this <see cref="TextBuilder"/>.
        /// </summary>
        private void removeLastCharacter()
        {
            if (Characters.Count == 0)
                return;

            FontStore.CharacterGlyph glyph = Characters[Characters.Count - 1].Glyph;
            Characters.RemoveAt(Characters.Count - 1);

            currentPos.X -= glyph.XAdvance;
            if (lastGlyph != null)
                currentPos.X -= glyph.GetKerning(lastGlyph.Value);

            currentRowHeight = Characters.Count == 0 ? 0 : Characters.Max(c => getGlyphHeight(c.Glyph));
        }

        /// <summary>
        /// Checks if a glyph will exceed the available text bounds and either adds a new line or truncates the text.
        /// </summary>
        /// <param name="glyph">The glyph being added.</param>
        private void ensureAreaAvailable(FontStore.CharacterGlyph glyph)
        {
            if (!multiline && !truncate)
                return;

            try
            {
                // Todo: Is this correct? Shouldn't it be the texture width?
                if (currentPos.X + glyph.XAdvance <= maxWidth)
                    return;

                if (multiline)
                    addNewLine();

                if (truncate)
                    addEllipsis();
            }
            finally
            {
                // Keep track of the current row height for any future invocations
                currentRowHeight = Math.Max(currentRowHeight, getGlyphHeight(glyph));
            }
        }

        /// <summary>
        /// Adjusts the current position to a new line.
        /// </summary>
        private void addNewLine()
        {
            // Reset + vertically offset the current position
            currentPos.X = startOffset.X;
            currentPos.Y += currentRowHeight + spacing.Y;

            // Immediately after moving to a new line, the line is empty
            currentRowHeight = 0;
        }

        /// <summary>
        /// Adds the ellipsis glyphs to the string, removing any characters until the builder is able to do so without exceeding the text bounds.
        /// </summary>
        private void addEllipsis()
        {
            // Remove characters by backtracking until both of the following conditions are met:
            // 1. The ellipsis glyphs can be added without exceeding the text bounds
            // 2. The last character in the builder is not a whitespace (for visual niceness)
            // Or until there are no more non-ellipsis characters in the builder (if the ellipsis string is very long)

            do
            {
                removeLastCharacter();

                if (Characters.Count == 0)
                    break;
            } while (Characters[Characters.Count - 1].Glyph.IsWhiteSpace || currentPos.X + ellipsisLength > maxWidth);

            // Add the ellipsis characters
            foreach (var g in ellipsisGlyphs)
                addCharacter(g, true);

            // Stop the addition of more characters after the ellipsis
            canAddCharacters = false;
        }

        /// <summary>
        /// Retrieves the height of a glyph.
        /// </summary>
        /// <param name="glyph">The glyph to retrieve the height of.</param>
        /// <returns>The height of the glyph.</returns>
        private float getGlyphHeight(FontStore.CharacterGlyph glyph)
        {
            if (useFullGlyphHeight)
                return fontSize;

            // Space characters typically have heights that exceed the height of all other characters in the font
            // Thus, the height is forced to 0 such that only non-whitespace character heights are considered
            if (glyph.IsWhiteSpace)
                return 0;

            return glyph.Height;
        }
    }
}
