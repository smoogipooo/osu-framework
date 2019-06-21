// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Textures;

namespace osu.Framework.Text
{
    /// <summary>
    /// An <see cref="ICharacterGlyph"/> with a fixed width.
    /// </summary>
    public readonly struct FixedWidthCharacterGlyph : ICharacterGlyph
    {
        public Texture Texture => glyph.Texture;
        public float XOffset { get; }
        public float YOffset => glyph.YOffset;
        public float XAdvance { get; }
        public float Width => glyph.Width;
        public float Height => glyph.Height;
        public char Character => glyph.Character;

        private readonly ICharacterGlyph glyph;

        public FixedWidthCharacterGlyph(ICharacterGlyph glyph, float width)
        {
            this.glyph = glyph;

            XOffset = (width - glyph.Width) / 2;
            XAdvance = width;
        }

        public float GetKerning(ICharacterGlyph lastGlyph) => 0;
    }
}
