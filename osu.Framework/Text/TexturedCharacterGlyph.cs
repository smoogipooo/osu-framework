// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Textures;

namespace osu.Framework.Text
{
    public sealed class TexturedCharacterGlyph : ITexturedCharacterGlyph
    {
        public Texture Texture { get; }

        public float XOffset => glyph.XOffset * scaleAdjustment;
        public float YOffset => glyph.YOffset * scaleAdjustment;
        public float XAdvance => glyph.XAdvance * scaleAdjustment;
        public char Character => glyph.Character;
        public float Width => Texture.Width * scaleAdjustment;
        public float Height => Texture.Height * scaleAdjustment;

        private readonly CharacterGlyph glyph;
        private readonly float scaleAdjustment;

        public TexturedCharacterGlyph(CharacterGlyph glyph, Texture texture, float scaleAdjustment)
        {
            this.glyph = glyph;
            this.scaleAdjustment = scaleAdjustment;

            Texture = texture;
        }

        public float GetKerning<T>(T lastGlyph)
            where T : ICharacterGlyph
            => glyph.GetKerning(lastGlyph) * scaleAdjustment;
    }
}
