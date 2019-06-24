// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Textures;

namespace osu.Framework.Text
{
    /// <summary>
    /// Interface for an object that contains the texture and associated spacing information for a character.
    /// </summary>
    public interface ICharacterGlyph
    {
        /// <summary>
        /// The texture for this character.
        /// </summary>
        Texture Texture { get; }

        /// <summary>
        /// How much the current x-position should be moved for drawing. This should not adjust the cursor position.
        /// </summary>
        float XOffset { get; }

        /// <summary>
        /// How much the current y-position should be moved for drawing. This should not adjust the cursor position.
        /// </summary>
        float YOffset { get; }

        /// <summary>
        /// How much the current x-position should be moved after drawing a character.
        /// </summary>
        float XAdvance { get; }

        /// <summary>
        /// The width of the area that should be drawn.
        /// </summary>
        float Width { get; }

        /// <summary>
        /// The height of the area that should be drawn.
        /// </summary>
        float Height { get; }

        /// <summary>
        /// The character represented by this glyph.
        /// </summary>
        char Character { get; }

        /// <summary>
        /// Retrieves the kerning between this <see cref="CharacterGlyph"/> and the one prior to it.
        /// </summary>
        /// <param name="lastGlyph">The <see cref="CharacterGlyph"/> prior to this one.</param>
        float GetKerning(ICharacterGlyph lastGlyph);
    }

    public static class CharacterGlyphExtensions
    {
        /// <summary>
        /// Whether a <see cref="CharacterGlyph"/> represents a whitespace.
        /// </summary>
        public static bool IsWhiteSpace<T>(this T glyph)
            where T : ICharacterGlyph
            => glyph.Texture == null || char.IsWhiteSpace(glyph.Character);
    }
}
