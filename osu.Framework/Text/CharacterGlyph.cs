// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace osu.Framework.Text
{
    /// <summary>
    /// Contains the texture and associated spacing information for a character.
    /// </summary>
    public class CharacterGlyph
    {
        /// <summary>
        /// The texture for this character.
        /// </summary>
        public Texture Texture { get; internal set; }

        public float XOffset => xOffset * scaleAdjust;

        public float YOffset => yOffset * scaleAdjust;

        public float XAdvance => xAdvance * scaleAdjust;

        public float Width => IsWhiteSpace ? XAdvance : (width ?? Texture?.Width ?? 0) * scaleAdjust;

        public float Height => IsWhiteSpace ? 0 : (height ?? Texture?.Height ?? 0) * scaleAdjust;

        public readonly char Character;

        private readonly GlyphStore containingStore;
        private readonly float yOffset;
        private readonly float xOffset;
        private readonly float xAdvance;
        private readonly float? width;
        private readonly float? height;

        private float scaleAdjust = 1;

        public CharacterGlyph(char character, float xOffset, float yOffset, float xAdvance, GlyphStore containingStore, float? width = null, float? height = null)
        {
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.xAdvance = xAdvance;
            this.containingStore = containingStore;
            this.width = width;
            this.height = height;

            Character = character;
        }

        /// <summary>
        /// Apply a scale adjust to metrics of this glyph.
        /// </summary>
        /// <param name="scaleAdjust">The adjustment to apply. This will be multiplied into any existing adjustment.</param>
        public void ApplyScaleAdjust(float scaleAdjust) => this.scaleAdjust *= scaleAdjust;

        /// <summary>
        /// Retrieves the kerning between this <see cref="CharacterGlyph"/> and the one prior to it.
        /// </summary>
        /// <param name="lastGlyph">The <see cref="CharacterGlyph"/> prior to this one.</param>
        public virtual float GetKerning(CharacterGlyph lastGlyph) => containingStore.GetKerning(lastGlyph.Character, Character) * scaleAdjust;

        /// <summary>
        /// Whether this <see cref="CharacterGlyph"/> represents a whitespace.
        /// </summary>
        public bool IsWhiteSpace => Texture == null || Char.IsWhiteSpace(Character);
    }
}
