// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace osu.Framework.Text
{
    public class CharacterGlyph : ICharacterGlyph
    {
        public Texture Texture { get; internal set; }
        public char Character { get; }

        internal float ScaleAdjustment = 1;

        public float XOffset => xOffset * ScaleAdjustment;
        public float YOffset => yOffset * ScaleAdjustment;
        public float XAdvance => xAdvance * ScaleAdjustment;
        public float Width => (Texture?.Width ?? 0) * ScaleAdjustment;
        public float Height => (Texture?.Height ?? 0) * ScaleAdjustment;

        private readonly float xOffset;
        private readonly float yOffset;
        private readonly float xAdvance;

        private readonly GlyphStore containingStore;

        public CharacterGlyph(char character, float xOffset, float yOffset, float xAdvance, [CanBeNull] GlyphStore containingStore)
            : this(null, character, xOffset, yOffset, xAdvance, containingStore)
        {
        }

        public CharacterGlyph(Texture texture, char character, float xOffset, float yOffset, float xAdvance, [CanBeNull] GlyphStore containingStore)
        {
            this.containingStore = containingStore;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.xAdvance = xAdvance;

            Texture = texture;
            Character = character;
        }

        public float GetKerning(ICharacterGlyph lastGlyph)
            => lastGlyph == null || containingStore == null ? 0 : containingStore.GetKerning(lastGlyph.Character, Character) * ScaleAdjustment;
    }
}
