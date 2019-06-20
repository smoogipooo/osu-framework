// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Textures;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Logging;
using System.Collections.Concurrent;

namespace osu.Framework.IO.Stores
{
    public class FontStore : TextureStore, IGlyphLookupStore
    {
        private readonly List<GlyphStore> glyphStores = new List<GlyphStore>();

        private readonly List<FontStore> nestedFontStores = new List<FontStore>();

        private readonly Func<(string, char), Texture> cachedTextureLookup;

        /// <summary>
        /// A local cache to avoid string allocation overhead. Can be changed to (string,char)=>string if this ever becomes an issue,
        /// but as long as we directly inherit <see cref="TextureStore"/> this is a slight optimisation.
        /// </summary>
        private readonly ConcurrentDictionary<(string, char), Texture> namespacedTextureCache = new ConcurrentDictionary<(string, char), Texture>();

        public FontStore(IResourceStore<TextureUpload> store = null, float scaleAdjust = 100)
            : base(store, scaleAdjust: scaleAdjust)
        {
            cachedTextureLookup = t =>
            {
                var tex = Get(getTextureName(t.Item1, t.Item2));

                if (tex == null)
                    Logger.Log($"Glyph texture lookup for {getTextureName(t.Item1, t.Item2)} was unsuccessful.");

                return tex;
            };
        }

        public CharacterGlyph Get(string fontName, char character)
        {
            var glyphStore = getGlyphStore(fontName, character);

            if (glyphStore == null)
                return null;

            var glyph = glyphStore.GetCharacterInfo(character);

            glyph.Texture = namespacedTextureCache.GetOrAdd((fontName, character), cachedTextureLookup);
            glyph.ApplyScaleAdjust(1 / ScaleAdjust);

            return glyph;
        }

        public Task<CharacterGlyph> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));

        /// <summary>
        /// Retrieves the base height of a font containing a particular character.
        /// </summary>
        /// <param name="c">The charcter to search for.</param>
        /// <returns>The base height of the font.</returns>
        public float? GetBaseHeight(char c)
        {
            var glyphStore = getGlyphStore(string.Empty, c);

            return glyphStore?.GetBaseHeight() / ScaleAdjust;
        }

        /// <summary>
        /// Retrieves the base height of a font containing a particular character.
        /// </summary>
        /// <param name="fontName">The font to search for.</param>
        /// <returns>The base height of the font.</returns>
        public float? GetBaseHeight(string fontName)
        {
            var glyphStore = getGlyphStore(fontName);

            return glyphStore?.GetBaseHeight() / ScaleAdjust;
        }

        private string getTextureName(string fontName, char charName) => string.IsNullOrEmpty(fontName) ? charName.ToString() : $"{fontName}/{charName}";

        /// <summary>
        /// Retrieves a <see cref="GlyphStore"/> from this <see cref="FontStore"/> that matches a font and character.
        /// </summary>
        /// <param name="fontName">The font to look up the <see cref="GlyphStore"/> for.</param>
        /// <param name="charName">A character to look up in the <see cref="GlyphStore"/>.</param>
        /// <returns>The first available <see cref="GlyphStore"/> matches the name and contains the specified character. Null if not available.</returns>
        private GlyphStore getGlyphStore(string fontName, char? charName = null)
        {
            foreach (var store in glyphStores)
            {
                if (charName == null)
                {
                    if (store.FontName == fontName)
                        return store;
                }
                else
                {
                    if (store.ContainsTexture(getTextureName(fontName, charName.Value)))
                        return store;
                }
            }

            foreach (var store in nestedFontStores)
            {
                var nestedStore = store.getGlyphStore(fontName, charName);
                if (nestedStore != null)
                    return nestedStore;
            }

            return null;
        }

        protected override IEnumerable<string> GetFilenames(string name)
        {
            // extensions should not be used as they interfere with character lookup.
            yield return name;
        }

        public override void AddStore(IResourceStore<TextureUpload> store)
        {
            switch (store)
            {
                case FontStore fs:
                    nestedFontStores.Add(fs);
                    return;

                case GlyphStore gs:
                    glyphStores.Add(gs);
                    queueLoad(gs);
                    break;
            }

            base.AddStore(store);
        }

        private Task childStoreLoadTasks;

        /// <summary>
        /// Append child stores to a single threaded load task.
        /// </summary>
        private void queueLoad(GlyphStore store)
        {
            var previousLoadStream = childStoreLoadTasks;

            childStoreLoadTasks = Task.Run(async () =>
            {
                if (previousLoadStream != null)
                    await previousLoadStream;

                try
                {
                    Logger.Log($"Loading Font {store.FontName}...", level: LogLevel.Debug);
                    await store.LoadFontAsync();
                    Logger.Log($"Loaded Font {store.FontName}!", level: LogLevel.Debug);
                }
                catch (OperationCanceledException)
                {
                }
            });
        }

        public override void RemoveStore(IResourceStore<TextureUpload> store)
        {
            switch (store)
            {
                case FontStore fs:
                    nestedFontStores.Remove(fs);
                    return;

                case GlyphStore gs:
                    glyphStores.Remove(gs);
                    break;
            }

            base.RemoveStore(store);
        }

        public override Texture Get(string name)
        {
            var found = base.Get(name);

            if (found == null)
            {
                foreach (var store in nestedFontStores)
                    if ((found = store.Get(name)) != null)
                        break;
            }

            return found;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            glyphStores.ForEach(g => g.Dispose());
        }

        /// <summary>
        /// Contains the texture and associated spacing information for a character.
        /// </summary>
        public class CharacterGlyph
        {
            /// <summary>
            /// The texture for this character.
            /// </summary>
            public Texture Texture
            {
                get => texture;
                internal set
                {
                    texture = value;

                    width = texture?.Width ?? 0;
                    height = texture?.Height ?? 0;
                }
            }

            public float XOffset => xOffset * scaleAdjust;

            public float YOffset => yOffset * scaleAdjust;

            public float XAdvance => xAdvance * scaleAdjust;

            public float Width => IsWhiteSpace ? XAdvance : width * scaleAdjust;

            public float Height => IsWhiteSpace ? 0 : height * scaleAdjust;

            private readonly float yOffset;
            private Texture texture;
            private float width;
            private float height;
            private float xOffset;
            private float xAdvance;
            private float scaleAdjust;

            private readonly GlyphStore containingStore;
            private readonly char character;
            private bool widthOverridden;

            public CharacterGlyph(char character, float xOffset, float yOffset, float xAdvance, GlyphStore containingStore)
            {
                this.character = character;
                this.xOffset = xOffset;
                this.yOffset = yOffset;
                this.xAdvance = xAdvance;
                this.containingStore = containingStore;

                texture = null;
                width = 0;
                height = 0;
                scaleAdjust = 1;
                widthOverridden = false;
            }

            /// <summary>
            /// Apply a scale adjust to metrics of this glyph.
            /// </summary>
            /// <param name="scaleAdjust">The adjustment to apply. This will be multiplied into any existing adjustment.</param>
            public void ApplyScaleAdjust(float scaleAdjust) => this.scaleAdjust *= scaleAdjust;

            /// <summary>
            /// Overrides the width of this <see cref="CharacterGlyph"/>, adjusting <see cref="XOffset"/> to reposition the texture in the centre of the new width.
            /// Useful for displaying this <see cref="CharacterGlyph"/> in as part of a fake fixed-width font.
            /// </summary>
            /// <remarks>
            /// This only adjusts the <see cref="XAdvance"/> of the <see cref="CharacterGlyph"/>.
            /// </remarks>
            /// <param name="widthOverride">The new width.</param>
            public void ApplyWidthOverride(float widthOverride)
            {
                widthOverridden = true;

                xAdvance = widthOverride / scaleAdjust;
                xOffset = (widthOverride - Width) / 2 / scaleAdjust;
            }

            /// <summary>
            /// Retrieves the kerning between this <see cref="CharacterGlyph"/> and the one prior to it.
            /// </summary>
            /// <param name="lastGlyph">The <see cref="CharacterGlyph"/> prior to this one.</param>
            public float GetKerning(CharacterGlyph lastGlyph) => widthOverridden ? 0 : containingStore.GetKerning(lastGlyph.character, character) * scaleAdjust;

            /// <summary>
            /// Whether this <see cref="CharacterGlyph"/> represents a whitespace.
            /// </summary>
            public bool IsWhiteSpace => Texture == null || char.IsWhiteSpace(character);
        }
    }
}
