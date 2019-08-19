// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.IO.Stores;

namespace osu.Framework.Text
{
    public sealed class CharacterGlyph : ICharacterGlyph
    {
        public float XOffset { get; }
        public float YOffset { get; }
        public float XAdvance { get; }
        public char Character { get; }

        private readonly GlyphStore containingStore;

        public CharacterGlyph(char character, float xOffset, float yOffset, float xAdvance, [CanBeNull] GlyphStore containingStore)
        {
            this.containingStore = containingStore;
            Character = character;
            XOffset = xOffset;
            YOffset = yOffset;
            XAdvance = xAdvance;
        }

        public float GetKerning(ICharacterGlyph lastGlyph)
            => lastGlyph == null || containingStore == null ? 0 : containingStore.GetKerning(lastGlyph.Character, Character);
    }
}
