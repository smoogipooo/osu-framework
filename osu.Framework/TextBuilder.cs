// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
        public Vector2 TextSize { get; private set; }

        public readonly List<SpriteText.CharacterPart> Characters = new List<SpriteText.CharacterPart>();
        private FontStore.CharacterGlyph? lastGlyph => Characters.Count == 0 ? null : (FontStore.CharacterGlyph?)Characters[Characters.Count - 1].Glyph;

        private readonly float fontSize;
        private readonly bool useFullGlyphHeight;
        private readonly Vector2 startOffset;
        private readonly Vector2 spacing;
        private readonly float maxWidth;

        private Vector2 currentPos;
        private float currentLineHeight;
        private bool currentNewLine = true;

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
        public void AddCharacter(FontStore.CharacterGlyph glyph)
        {
            if (!CanAddCharacters)
                return;

            // Apply the font size scale
            glyph.ApplyScaleAdjust(fontSize);

            // Kerning is not applied if the user provided a custom width
            float kerning = lastGlyph == null ? 0 : glyph.GetKerning(lastGlyph.Value);

            // Check if there is enough space for the character, let subclasses decide whether to continue adding the character if not
            if (!HasAvailableSpace(kerning + glyph.XAdvance))
            {
                OnWidthExceeded();

                if (!CanAddCharacters)
                    return;
            }

            // Kerning is not just a draw-time offset - it affects the position of all further drawn characters, so it must be added to the position
            // Note that this is added after it is guaranteed that the character will be added, to not leave the current position in a bad state
            currentPos.X += kerning;

            // Add the character
            Characters.Add(new SpriteText.CharacterPart
            {
                Glyph = glyph,
                DrawRectangle = new RectangleF(new Vector2(currentPos.X + glyph.XOffset, currentPos.Y + glyph.YOffset),
                    new Vector2(glyph.Width, glyph.Height)),
                OnNewLine = currentNewLine,
            });

            // Move the current position
            currentPos.X += glyph.XAdvance;
            currentLineHeight = Math.Max(currentLineHeight, getGlyphHeight(glyph));
            currentNewLine = false;

            // Calculate the text size
            TextSize = Vector2.ComponentMax(TextSize, currentPos + new Vector2(0, currentLineHeight));
        }

        public void AddNewLine()
        {
            // Reset + vertically offset the current position
            currentPos.X = startOffset.X;
            currentPos.Y += currentLineHeight + spacing.Y;

            // Immediately after moving to a new line, the line is empty
            currentLineHeight = 0;
            currentNewLine = true;
        }

        public void RemoveLastCharacter()
        {
            if (Characters.Count == 0)
                return;

            SpriteText.CharacterPart currentCharacter = Characters[Characters.Count - 1];
            SpriteText.CharacterPart? lastCharacter = Characters.Count == 1 ? null : (SpriteText.CharacterPart?)Characters[Characters.Count - 2];

            Characters.RemoveAt(Characters.Count - 1);

            currentLineHeight = 0;

            // Note: This is a very fast O(n^2) for removing all characters within a line
            for (int i = Characters.Count - 1; i >= 0; i--)
            {
                currentLineHeight = Math.Max(currentLineHeight, getGlyphHeight(Characters[i].Glyph));

                if (Characters[i].OnNewLine)
                    break;
            }
            if (currentCharacter.OnNewLine)
            {
                // Move up to the previous line
                currentPos.Y -= currentLineHeight + spacing.Y;
                currentPos.X = 0;

                // Calculate the cursor position to right of the last glyph in the previous line
                if (lastCharacter != null)
                {
                    // The original cursor position can be retrieved by subtracting the draw offset from the draw position
                    currentPos.X = lastCharacter.Value.DrawRectangle.Left - lastCharacter.Value.Glyph.XOffset + lastCharacter.Value.Glyph.XAdvance;
                }
            }
            else
            {
                // Move back within the currently line by reversing the operations done in AddCharacter()
                currentPos.X -= currentCharacter.Glyph.XAdvance;

                if (lastCharacter != null)
                    currentPos.X -= currentCharacter.Glyph.GetKerning(lastCharacter.Value.Glyph);
            }
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
