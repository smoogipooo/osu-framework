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

        private Vector2 currentPos;
        internal Vector2 CurrentPos => currentPos;

        private float currentLineHeight;

        /// <summary>
        /// Creates a new <see cref="TextBuilder"/>.
        /// </summary>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="useFullGlyphHeight">True to use <paramref name="fontSize"/> as the height for each line. False if the height of each individual glyph should be used.</param>
        /// <param name="startOffset">The offset at which characters should begin being added at.</param>
        /// <param name="spacing">The spacing between characters.</param>
        /// <param name="maxWidth">The maximum width of the resulting text bounds.</param>
        public TextBuilder(float fontSize, float maxWidth = float.MaxValue, bool useFullGlyphHeight = true, Vector2 startOffset = default, Vector2 spacing = default)
        {
            this.fontSize = fontSize;
            this.useFullGlyphHeight = useFullGlyphHeight;
            this.startOffset = startOffset;
            this.spacing = spacing;
            this.maxWidth = maxWidth;

            currentPos = startOffset;
        }

        protected virtual bool CanAddCharacters => true;

        /// <summary>
        /// Adds a character to this <see cref="TextBuilder"/>.
        /// </summary>
        /// <param name="glyph">The glyph of the character to add.</param>
        public void AddCharacter(FontStore.CharacterGlyph glyph, float? widthOverride = null)
        {
            if (!CanAddCharacters)
                return;

            // Apply the font size scale
            glyph.ApplyScaleAdjust(fontSize);

            if (widthOverride != null)
            {
                widthOverride *= fontSize;

                glyph.XAdvance = widthOverride.Value;
                glyph.XOffset = (widthOverride.Value - glyph.Width) / 2;
            }

            // Kerning is not applied if the user provided a custom width
            float kerning = lastGlyph == null || widthOverride != null ? 0 : glyph.GetKerning(lastGlyph.Value);

            // Test if there's enough space for the character to be added
            // Derived text builders may implement custom functionality if not, such as truncation or multi-lining
            if (!HasAvailableSpace(kerning + glyph.XAdvance))
            {
                OnWidthExceeded();

                // Exceeding the width may disallow the character to continue being added
                if (!CanAddCharacters)
                    return;
            }

            // Kerning is not just a draw-time offset - it affects the position of all further drawn characters, so it must be added to the position
            currentPos.X += kerning;

            // Add the character
            Characters.Add(new SpriteText.CharacterPart
            {
                Glyph = glyph,
                Texture = glyph.Texture,
                DrawRectangle = new RectangleF(new Vector2(currentPos.X + glyph.XOffset, currentPos.Y + glyph.YOffset),
                    new Vector2(glyph.Width, glyph.Height)),
            });

            // Move the current position
            currentPos.X += glyph.XAdvance;
            currentLineHeight = Math.Max(currentLineHeight, getGlyphHeight(glyph));
        }

        public void AddNewLine()
        {
            // Reset + vertically offset the current position
            currentPos.X = startOffset.X;
            currentPos.Y += currentLineHeight + spacing.Y;

            // Immediately after moving to a new line, the line is empty
            currentLineHeight = 0;
        }

        public void RemoveLastCharacter()
        {
            if (Characters.Count == 0)
                return;

            FontStore.CharacterGlyph glyph = Characters[Characters.Count - 1].Glyph;
            Characters.RemoveAt(Characters.Count - 1);

            if (lastGlyph != null)
                currentPos.X -= glyph.GetKerning(lastGlyph.Value);

            currentPos.X -= glyph.XAdvance;
            currentLineHeight = Characters.Count == 0 ? 0 : Characters.Max(c => getGlyphHeight(c.Glyph));
        }

        protected virtual void OnWidthExceeded()
        {
        }

        protected bool HasAvailableSpace(float length) => currentPos.X + length <= maxWidth;

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

            return glyph.YOffset + glyph.Height;
        }
    }
}
