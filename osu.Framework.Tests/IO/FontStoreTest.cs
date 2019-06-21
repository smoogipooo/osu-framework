// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Text;

namespace osu.Framework.Tests.IO
{
    [TestFixture]
    public class FontStoreTest
    {
        private ResourceStore<byte[]> fontResourceStore;

        [SetUp]
        public void Setup()
        {
            fontResourceStore = new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(Drawable).Assembly.Location), "Resources.Fonts.OpenSans");
        }

        [Test]
        public void TestNestedScaleAdjust()
        {
            var fontStore = new FontStore(new GlyphStore(fontResourceStore, "OpenSans"));
            var nestedFontStore = new FontStore(new GlyphStore(fontResourceStore, "OpenSans-Bold"), 10);

            fontStore.AddStore(nestedFontStore);

            var normalGlyph = (ScaleAdjustedCharacterGlyph)fontStore.Get("OpenSans", 'a');
            var boldGlyph = (ScaleAdjustedCharacterGlyph)fontStore.Get("OpenSans-Bold", 'a');

            Assert.That(normalGlyph.ScaleAdjustment, Is.EqualTo(1f / 100));
            Assert.That(boldGlyph.ScaleAdjustment, Is.EqualTo(1f / 10));
        }
    }
}
