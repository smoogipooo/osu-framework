// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Text;
using osuTK;

namespace osu.Framework.Tests.Text
{
    [TestFixture]
    public class TextBuilderTest
    {
        /// <summary>
        /// Tests that the size of a fresh text builder is zero.
        /// </summary>
        [Test]
        public void TestInitialSizeIsZero()
        {
            var builder = new TextBuilder(null, null);

            Assert.That(builder.TextSize, Is.EqualTo(Vector2.Zero));
        }

        /// <summary>
        /// Tests that the first added character is correctly marked as being on a new line.
        /// </summary>
        [Test]
        public void TestFirstCharacterIsOnNewLine()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore((font, new TestGlyph('a', 1, 2, 3, 4, 5, 0))), font);
            builder.AddText("a");

            Assert.That(builder.Characters[0].OnNewLine, Is.True);
        }

        /// <summary>
        /// Tests that the first added fixed-width character metrics match the glyph's.
        /// </summary>
        [Test]
        public void TestFirstCharacterRectangleIsCorrect()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, 0))
            ), font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo(font.Size));
            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(font.Size * 2));
            Assert.That(builder.Characters[0].DrawRectangle.Width, Is.EqualTo(font.Size * 4));
            Assert.That(builder.Characters[0].DrawRectangle.Height, Is.EqualTo(font.Size * 5));
        }

        /// <summary>
        /// Tests that the first added character metrics match the glyph's.
        /// </summary>
        [Test]
        public void TestFirstFixedWidthCharacterRectangleIsCorrect()
        {
            var font = new FontUsage("test", fixedWidth: true);
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, 0)),
                (font, new TestGlyph('m', 7, 8, 9, 10, 11, 0))
            ), font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo((10 - 4) * font.Size / 2));
            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(font.Size * 2));
            Assert.That(builder.Characters[0].DrawRectangle.Width, Is.EqualTo(font.Size * 4));
            Assert.That(builder.Characters[0].DrawRectangle.Height, Is.EqualTo(font.Size * 5));
        }

        /// <summary>
        /// Tests that the current position is advanced after a character is added.
        /// </summary>
        [Test]
        public void TestCurrentPositionAdvancedAfterCharacter()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, 0))
            ), font);

            builder.AddText("a");
            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(3 * font.Size + font.Size));
            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(font.Size * 2));
            Assert.That(builder.Characters[1].DrawRectangle.Width, Is.EqualTo(font.Size * 4));
            Assert.That(builder.Characters[1].DrawRectangle.Height, Is.EqualTo(font.Size * 5));
        }

        /// <summary>
        /// Tests that the current position is advanced after a fixed width character is added.
        /// </summary>
        [Test]
        public void TestCurrentPositionAdvancedAfterFixedWidthCharacter()
        {
            var font = new FontUsage("test", fixedWidth: true);
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, 0)),
                (font, new TestGlyph('m', 7, 8, 9, 10, 11, 0))
            ), font);

            builder.AddText("a");
            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(10 * font.Size + (10 - 4) * font.Size / 2));
            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(font.Size * 2));
            Assert.That(builder.Characters[1].DrawRectangle.Width, Is.EqualTo(font.Size * 4));
            Assert.That(builder.Characters[1].DrawRectangle.Height, Is.EqualTo(font.Size * 5));
        }

        /// <summary>
        /// Tests that no kerning is added for the first character.
        /// </summary>
        [Test]
        public void TestFirstCharacterHasNoKerning()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6))
            ), font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo(font.Size));
        }

        /// <summary>
        /// Tests that kerning is added for the second character.
        /// </summary>
        [Test]
        public void TestKerningAddedOnSecondCharacter()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6))
            ), font);

            builder.AddText("a");
            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(3 * font.Size - 6 * font.Size + font.Size));
        }

        /// <summary>
        /// Tests that no kerning is added for fixed-width characters,
        /// </summary>
        [Test]
        public void TestFixedWidthCharacterHasNoKerning()
        {
            var font = new FontUsage("test", fixedWidth: true);
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6)),
                (font, new TestGlyph('m', 7, 8, 9, 10, 11, 0))
            ), font);

            builder.AddText("a");
            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo((10 - 4) * font.Size / 2));
            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(10 * font.Size + (10 - 4) * font.Size / 2));
        }

        /// <summary>
        /// Tests that a new line added to an empty builder always uses the font height.
        /// </summary>
        [TestCase(false)]
        [TestCase(true)]
        public void TestNewLineOnEmptyBuilderOffsetsPositionByFontSize(bool useFontHeightAsSize)
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6))
            ), font);

            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(font.Size + 2 * font.Size));
        }

        /// <summary>
        /// Tests that a new line added to an empty line always uses the font height.
        /// </summary>
        [TestCase(false)]
        [TestCase(true)]
        public void TestNewLineOnEmptyLineOffsetsPositionByFontSize(bool useFontHeightAsSize)
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6))
            ), font);

            builder.AddNewLine();
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(2 * font.Size + 2 * font.Size));
        }

        /// <summary>
        /// Tests that a new line added to a builder that is using the font height as size offsets the y-position by the font size and not the glyph size.
        /// </summary>
        [Test]
        public void TestNewLineUsesFontHeightWhenUsingFontHeightAsSize()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6)),
                (font, new TestGlyph('b', 7, 8, 9, 10, 11, -12))
            ), font);

            builder.AddText("a");
            builder.AddText("b");
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(font.Size + 2 * font.Size));
        }

        /// <summary>
        /// Tests that a new line added to a builder that is not using the font height as size offsets the y-position by the glyph size and not the font size.
        /// </summary>
        [Test]
        public void TestNewLineUsesGlyphHeightWhenNotUsingFontHeightAsSize()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6)),
                (font, new TestGlyph('b', 7, 8, 9, 10, 11, -12))
            ), font, useFontSizeAsHeight: false);

            builder.AddText("a");
            builder.AddText("b");
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(8 * font.Size + 11 * font.Size + 2 * font.Size));
        }

        /// <summary>
        /// Tests that the first added character on a new line is correctly marked as being on a new line.
        /// </summary>
        [Test]
        public void TestFirstCharacterOnNewLineIsOnNewLine()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6))
            ), font);

            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[1].OnNewLine, Is.True);
        }

        /// <summary>
        /// Tests that no kerning is added for the first character of a new line.
        /// </summary>
        [Test]
        public void TestFirstCharacterOnNewLineHasNoKerning()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6))
            ), font);

            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(font.Size));
        }

        /// <summary>
        /// Tests that the current position is correctly reset when the first character is removed.
        /// </summary>
        [Test]
        public void TestRemoveFirstCharacterResetsCurrentPosition()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6))
            ), font);

            builder.AddText("a");
            builder.RemoveLastCharacter();

            Assert.That(builder.TextSize, Is.EqualTo(Vector2.Zero));

            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo(font.Size));
        }

        /// <summary>
        /// Tests that the current position is moved backwards and the character is removed when a character is removed.
        /// </summary>
        [Test]
        public void TestRemoveCharacterOnSameLineRemovesCharacter()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6))
            ), font);

            builder.AddText("a");
            builder.AddText("a");
            builder.RemoveLastCharacter();

            Assert.That(builder.TextSize, Is.EqualTo(new Vector2(3 * font.Size, font.Size)));

            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(3 * font.Size - 6 * font.Size + font.Size));
        }

        /// <summary>
        /// Tests that the current position is moved to the end of the previous line, and that the character + new line is removed when a character is removed.
        /// </summary>
        [Test]
        public void TestRemoveCharacterOnNewLineRemovesCharacterAndLine()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, -6))
            ), font);

            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText("a");
            builder.RemoveLastCharacter();

            Assert.That(builder.TextSize, Is.EqualTo(new Vector2(3 * font.Size, font.Size)));

            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.TopLeft, Is.EqualTo(new Vector2(font.Size + 3 * font.Size - 6 * font.Size, 2 * font.Size)));
            Assert.That(builder.TextSize, Is.EqualTo(new Vector2(3 * font.Size, font.Size)));
        }

        /// <summary>
        /// Tests that the custom user-provided spacing is added for a new character/line.
        /// </summary>
        [Test]
        public void TestSpacingAdded()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('a', 1, 2, 3, 4, 5, 0))
            ), font, spacing: new Vector2(7, 8));

            builder.AddText("a");
            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo(font.Size));
            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(3 * font.Size + 7 + font.Size));
            Assert.That(builder.Characters[2].DrawRectangle.Left, Is.EqualTo(font.Size));
            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(font.Size + 8 + 2 * font.Size));
        }

        /// <summary>
        /// Tests that glyph lookup falls back to using the same character with no font name.
        /// </summary>
        [Test]
        public void TestSameCharacterFallsBackWithNoFontName()
        {
            var font = new FontUsage("test");
            var nullFont = new FontUsage(null);
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('b', 0, 0, 0, 0, 0, 0)),
                (nullFont, new TestGlyph('a', 0, 0, 0, 0, 0, 0)),
                (font, new TestGlyph('?', 0, 0, 0, 0, 0, 0)),
                (nullFont, new TestGlyph('?', 0, 0, 0, 0, 0, 0))
            ), font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].Character, Is.EqualTo('a'));
        }

        /// <summary>
        /// Tests that glyph lookup falls back to using the fallback character with the provided font name.
        /// </summary>
        [Test]
        public void TestFallBackCharacterFallsBackWithFontName()
        {
            var font = new FontUsage("test");
            var nullFont = new FontUsage(null);
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('b', 0, 0, 0, 0, 0, 0)),
                (nullFont, new TestGlyph('b', 0, 0, 0, 0, 0, 0)),
                (font, new TestGlyph('?', 0, 0, 0, 0, 0, 0)),
                (nullFont, new TestGlyph('?', 1, 0, 0, 0, 0, 0))
            ), font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].Character, Is.EqualTo('?'));
            Assert.That(builder.Characters[0].XOffset, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that glyph lookup falls back to using the fallback character with no font name.
        /// </summary>
        [Test]
        public void TestFallBackCharacterFallsBackWithNoFontName()
        {
            var font = new FontUsage("test");
            var nullFont = new FontUsage(null);
            var builder = new TextBuilder(new TestStore(
                (font, new TestGlyph('b', 0, 0, 0, 0, 0, 0)),
                (nullFont, new TestGlyph('b', 0, 0, 0, 0, 0, 0)),
                (font, new TestGlyph('b', 0, 0, 0, 0, 0, 0)),
                (nullFont, new TestGlyph('?', 1, 0, 0, 0, 0, 0))
            ), font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].Character, Is.EqualTo('?'));
            Assert.That(builder.Characters[0].XOffset, Is.EqualTo(font.Size));
        }

        /// <summary>
        /// Tests that a null glyph is correctly handled.
        /// </summary>
        [Test]
        public void TestFailedCharacterLookup()
        {
            var font = new FontUsage("test");
            var builder = new TextBuilder(new TestStore(), font);

            builder.AddText("a");

            Assert.That(builder.TextSize, Is.EqualTo(Vector2.Zero));
        }

        private class TestStore : IGlyphLookupStore
        {
            private readonly (FontUsage font, ICharacterGlyph glyph)[] glyphs;

            public TestStore(params (FontUsage font, ICharacterGlyph glyph)[] glyphs)
            {
                this.glyphs = glyphs;
            }

            public ICharacterGlyph Get(string fontName, char character)
            {
                if (string.IsNullOrEmpty(fontName))
                    return glyphs.FirstOrDefault(g => g.glyph.Character == character).glyph;

                return glyphs.FirstOrDefault(g => g.font.FontName == fontName && g.glyph.Character == character).glyph;
            }

            public Task<ICharacterGlyph> GetAsync(string fontName, char character) => throw new System.NotImplementedException();
        }

        private struct TestGlyph : ICharacterGlyph
        {
            public Texture Texture => new Texture(1, 1);
            public float XOffset { get; }
            public float YOffset { get; }
            public float XAdvance { get; }
            public float Width { get; }
            public float Height { get; }
            public char Character { get; }

            private readonly float kerning;

            public TestGlyph(char character, float xOffset, float yOffset, float xAdvance, float width, float height, float kerning)
            {
                this.kerning = kerning;

                Character = character;
                XOffset = xOffset;
                YOffset = yOffset;
                XAdvance = xAdvance;
                Width = width;
                Height = height;
            }

            public float GetKerning(ICharacterGlyph lastGlyph) => kerning;
        }
    }
}
