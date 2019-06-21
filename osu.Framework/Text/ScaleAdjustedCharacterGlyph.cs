// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Textures;

namespace osu.Framework.Text
{
    /// <summary>
    /// An <see cref="ICharacterGlyph"/> with an applied scale adjustment.
    /// </summary>
    public readonly struct ScaleAdjustedCharacterGlyph : ICharacterGlyph
    {
        public Texture Texture => glyph.Texture;
        public float XOffset => glyph.XOffset * ScaleAdjustment;
        public float YOffset => glyph.YOffset * ScaleAdjustment;
        public float XAdvance => glyph.XAdvance * ScaleAdjustment;
        public float Width => glyph.Width * ScaleAdjustment;
        public float Height => glyph.Height * ScaleAdjustment;
        public char Character => glyph.Character;

        public readonly float ScaleAdjustment;

        private readonly ICharacterGlyph glyph;

        public ScaleAdjustedCharacterGlyph(ICharacterGlyph glyph, float scaleAdjustment)
        {
            this.glyph = glyph;

            ScaleAdjustment = scaleAdjustment;
        }

        public float GetKerning(ICharacterGlyph lastGlyph) => glyph.GetKerning(lastGlyph) * ScaleAdjustment;
    }
}
