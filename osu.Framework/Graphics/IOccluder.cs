// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.Primitives;

namespace osu.Framework.Graphics
{
    public interface IOccluder : IDrawable
    {
    }

    public static class OccluderExtensions
    {
        public static bool Occludes(this IOccluder occluder, IDrawable target, RectangleF maskingBounds)
        {
            IConvexPolygon occluderPolygon = occluder.OcclusionPolygon;
            IConvexPolygon targetPolygon = target.OcclusionPolygon;

            // Do a very quick AABB test
            if (!occluderPolygon.AABBFloat.IntersectsWith(targetPolygon.AABBFloat))
                return false;

            // Perform an exact test
            return occluderPolygon.Occludes(ref targetPolygon, ref maskingBounds);
        }
    }
}
