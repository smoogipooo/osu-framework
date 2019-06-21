// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Text;

namespace osu.Framework.IO.Stores
{
    public interface IGlyphLookupStore
    {
        /// <summary>
        /// Retrieves a glyph from the store.
        /// </summary>
        /// <param name="fontName">The name of the font.</param>
        /// <param name="character">The character to retrieve.</param>
        /// <returns>The character glyph.</returns>
        ICharacterGlyph Get(string fontName, char character);

        /// <summary>
        /// Retrieves a glyph from the store asynchronously.
        /// </summary>
        /// <param name="fontName">The name of the font.</param>
        /// <param name="character">The character to retrieve.</param>
        /// <returns>The character glyph.</returns>
        Task<ICharacterGlyph> GetAsync(string fontName, char character);
    }
}
