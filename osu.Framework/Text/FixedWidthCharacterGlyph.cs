// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Text
{
    public sealed class FixedWidthCharacterGlyph : CharacterGlyph
    {
        public FixedWidthCharacterGlyph(CharacterGlyph glyph, float width)
            : base(glyph.Character, (width - glyph.Width) / 2, glyph.YOffset, width, null, glyph.Width, glyph.Height)
        {
            Texture = glyph.Texture;
        }

        public override float GetKerning(CharacterGlyph lastGlyph) => 0;
    }
}
