// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics.ES30;

namespace osu.Framework.Graphics.Shapes
{
    /// <summary>
    /// A simple rectangular box. Can be colored using the <see cref="Drawable.Colour"/> property.
    /// </summary>
    public class Box : Sprite
    {
        public Box()
        {
            Texture = Texture.WhitePixel;
        }

        protected override DrawNode CreateDrawNode() => new BoxDrawNode();

        private class BoxDrawNode : SpriteDrawNode
        {
            public override void Draw(RenderPass pass, Action<TexturedVertex2D> vertexAction, ref float vertexDepth)
            {
                if (pass == RenderPass.Front && GLWrapper.IsMaskingActive && GLWrapper.CurrentMaskingInfo.CornerRadius > 0)
                {
                    // Todo: Consider colours

                    var lastScreenSpaceDrawQuad = ScreenSpaceDrawQuad;

                    var shrinkedQuad = GLWrapper.CurrentMaskingInfo.ScreenSpaceQuad;
                    shrinkedQuad.Shrink(GLWrapper.CurrentMaskingInfo.CornerRadius);

                    ScreenSpaceDrawQuad = ScreenSpaceDrawQuad.IntersectWith(shrinkedQuad);

                    base.Draw(pass, vertexAction, ref vertexDepth);

                    ScreenSpaceDrawQuad = lastScreenSpaceDrawQuad;
                }
                else
                    base.Draw(pass, vertexAction, ref vertexDepth);

                if (pass == RenderPass.Front && GLWrapper.IsMaskingActive)
                {

                }
            }

            protected internal override bool SupportsFrontRenderPass
            {
                get
                {
                    if (DrawColourInfo.Colour.MinAlpha != 1)
                        return false;
                    if (DrawColourInfo.Blending.RGBEquation != BlendEquationMode.FuncAdd)
                        return false;
                    return InflationAmount == Vector2.Zero;
                }
            }
        }
    }
}
