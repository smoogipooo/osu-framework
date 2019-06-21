// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Textures;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Logging;
using System.Collections.Concurrent;
using osu.Framework.Text;
using osuTK.Graphics.OpenGL;

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

        public ICharacterGlyph Get(string fontName, char character)
        {
            foreach (var store in glyphStores)
            {
                if (store.ContainsTexture(getTextureName(fontName, character)))
                {
                    return store.GetCharacterInfo(character)
                                .WithTexture(namespacedTextureCache.GetOrAdd((fontName, character), cachedTextureLookup))
                                .WithScaleAdjust(1 / ScaleAdjust);
                }
            }

            foreach (var store in nestedFontStores)
            {
                var glyph = store.Get(fontName, character);
                if (glyph != null)
                    return glyph;
            }

            return null;
        }

        public Task<ICharacterGlyph> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));

        /// <summary>
        /// Retrieves the base height of a font containing a particular character.
        /// </summary>
        /// <param name="c">The character to search for.</param>
        /// <returns>The base height of the font.</returns>
        public float? GetBaseHeight(char c)
        {
            foreach (var store in glyphStores)
            {
                if (store.ContainsTexture(getTextureName(null, c)))
                    return store.GetBaseHeight() / ScaleAdjust;
            }

            foreach (var store in nestedFontStores)
            {
                var bh = store.GetBaseHeight(c);
                if (bh != null)
                    return bh;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the base height of a font containing a particular character.
        /// </summary>
        /// <param name="fontName">The font to search for.</param>
        /// <returns>The base height of the font.</returns>
        public float? GetBaseHeight(string fontName)
        {
            foreach (var store in glyphStores)
            {
                if (store.FontName == fontName)
                    return store.GetBaseHeight();
            }

            foreach (var store in nestedFontStores)
            {
                var bh = store.GetBaseHeight(fontName);
                if (bh != null)
                    return bh;
            }

            return null;
        }

        private string getTextureName(string fontName, char charName) => string.IsNullOrEmpty(fontName) ? charName.ToString() : $"{fontName}/{charName}";

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
    }
}
