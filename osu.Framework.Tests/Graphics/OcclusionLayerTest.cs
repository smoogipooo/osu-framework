// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Primitives;

namespace osu.Framework.Tests.Graphics
{
    [TestFixture]
    public class OcclusionLayerTest
    {
        private OcclusionLayer layer;

        [SetUp]
        public void Setup()
        {
            layer = new OcclusionLayer(1024, 1024);
        }

        [Test]
        public void TestAddFullScreen()
        {
            layer.Add(new Quad(0, 0, 1024, 1024));

            Assert.That(layer.IsOccluded(new Quad(0, 0, 1, 1)));
            Assert.That(layer.IsOccluded(new Quad(511, 511, 1, 1)));
            Assert.That(layer.IsOccluded(new Quad(1023, 1023, 1, 1)));
        }

        [Test]
        public void TestAddNonFullScreen()
        {
            layer.Add(new Quad(300, 300, 300, 300));

            Assert.That(!layer.IsOccluded(new Quad(0, 0, 1, 1)));
            Assert.That(layer.IsOccluded(new Quad(511, 511, 1, 1)));
            Assert.That(!layer.IsOccluded(new Quad(1023, 1023, 1, 1)));
        }

        [Test]
        public void TestAddInCentreOfTile()
        {
            layer.Add(new Quad(16, 16, 32, 32));

            Assert.That(!layer.IsOccluded(new Quad(0, 0, 1, 1)));
            Assert.That(!layer.IsOccluded(new Quad(32, 0, 1, 1)));
            Assert.That(!layer.IsOccluded(new Quad(0, 32, 1, 1)));
        }
    }
}
