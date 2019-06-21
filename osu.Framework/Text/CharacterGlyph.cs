// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace osu.Framework.Text
{
    public readonly struct CharacterGlyph : ICharacterGlyph
    {
        public Texture Texture { get; }
        public float XOffset { get; }
        public float YOffset { get; }
        public float XAdvance { get; }
        public float Width => Texture?.Width ?? 0;
        public float Height => Texture?.Height ?? 0;
        public char Character { get; }

        private readonly GlyphStore containingStore;

        public CharacterGlyph(char character, float xOffset, float yOffset, float xAdvance, GlyphStore containingStore)
            : this(null, character, xOffset, yOffset, xAdvance, containingStore)
        {
        }

        public CharacterGlyph(Texture texture, char character, float xOffset, float yOffset, float xAdvance, GlyphStore containingStore)
        {
            this.containingStore = containingStore;

            Texture = texture;
            Character = character;
            XOffset = xOffset;
            YOffset = yOffset;
            XAdvance = xAdvance;
        }

        public float GetKerning(ICharacterGlyph lastGlyph) => containingStore.GetKerning(lastGlyph.Character, Character);

        public CharacterGlyph WithTexture(Texture texture) => new CharacterGlyph(texture, Character, XOffset, YOffset, XAdvance, containingStore);
    }
}
