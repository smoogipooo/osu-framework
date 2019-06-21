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
        public float XOffset => glyph.XOffset * scaleAdjustment;
        public float YOffset => glyph.YOffset * scaleAdjustment;
        public float XAdvance => glyph.XAdvance * scaleAdjustment;
        public float Width => glyph.Width * scaleAdjustment;
        public float Height => glyph.Height * scaleAdjustment;
        public char Character => glyph.Character;

        private readonly ICharacterGlyph glyph;
        private readonly float scaleAdjustment;

        public ScaleAdjustedCharacterGlyph(ICharacterGlyph glyph, float scaleAdjustment)
        {
            this.glyph = glyph;
            this.scaleAdjustment = scaleAdjustment;
        }

        public float GetKerning(ICharacterGlyph lastGlyph) => glyph.GetKerning(lastGlyph) * scaleAdjustment;
    }
}
