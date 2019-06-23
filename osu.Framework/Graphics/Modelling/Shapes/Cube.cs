// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.OpenGL.Vertices;

namespace osu.Framework.Graphics.Modelling.Shapes
{
    public class Cube : Model
    {
        protected override DrawNode CreateDrawNode() => base.CreateDrawNode();

        protected class CubeDrawNode : ModelDrawNode
        {
            public CubeDrawNode(Model source)
                : base(source)
            {
            }

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);


            }
        }
    }
}
