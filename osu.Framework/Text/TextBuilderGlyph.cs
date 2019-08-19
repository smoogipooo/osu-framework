// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;

namespace osu.Framework.Text
{
    public struct TextBuilderGlyph : ITexturedCharacterGlyph
    {
        public Texture Texture => glyph.Texture;
        public float XOffset => ((fixedWidth - glyph.Width) / 2 ?? glyph.XOffset) * textSize;
        public float YOffset => glyph.YOffset * textSize;
        public float XAdvance => (fixedWidth ?? glyph.XAdvance) * textSize;
        public float Width => glyph.Width * textSize;
        public float Height => glyph.Height * textSize;
        public char Character => glyph.Character;

        /// <summary>
        /// The rectangle for the character to be drawn in.
        /// </summary>
        public RectangleF DrawRectangle;

        /// <summary>
        /// Whether this is the first character on a new line.
        /// </summary>
        public bool OnNewLine;

        private readonly ITexturedCharacterGlyph glyph;
        private readonly float textSize;
        private readonly float? fixedWidth;

        public TextBuilderGlyph(ITexturedCharacterGlyph glyph, float textSize, float? fixedWidth = null)
        {
            this = default;
            this.glyph = glyph;
            this.textSize = textSize;
            this.fixedWidth = fixedWidth;
        }

        public float GetKerning<T>(T lastGlyph)
            where T : ICharacterGlyph
            => fixedWidth != null ? 0 : glyph.GetKerning(lastGlyph);
    }
}
