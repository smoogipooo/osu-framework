// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;

namespace osu.Framework.Text
{
    public struct TextBuilderGlyph : ITexturedCharacterGlyph
    {
        public Texture Texture => Glyph.Texture;
        public float XOffset => ((fixedWidth - Glyph.Width) / 2 ?? Glyph.XOffset) * textSize;
        public float YOffset => Glyph.YOffset * textSize;
        public float XAdvance => (fixedWidth ?? Glyph.XAdvance) * textSize;
        public float Width => Glyph.Width * textSize;
        public float Height => Glyph.Height * textSize;
        public char Character => Glyph.Character;

        public readonly ITexturedCharacterGlyph Glyph;

        /// <summary>
        /// The rectangle for the character to be drawn in.
        /// </summary>
        public RectangleF DrawRectangle;

        /// <summary>
        /// Whether this is the first character on a new line.
        /// </summary>
        public bool OnNewLine;

        private readonly float textSize;
        private readonly float? fixedWidth;

        public TextBuilderGlyph(ITexturedCharacterGlyph glyph, float textSize, float? fixedWidth = null)
        {
            this = default;
            this.textSize = textSize;
            this.fixedWidth = fixedWidth;

            Glyph = glyph;
        }

        public float GetKerning<T>(T lastGlyph)
            where T : ICharacterGlyph
            => fixedWidth != null ? 0 : Glyph.GetKerning(lastGlyph);
    }
}
