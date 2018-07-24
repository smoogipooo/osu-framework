// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Numerics;

namespace osu.Framework.Graphics
{
    public static class Vector2Extensions
    {
        /// <summary>Transform a Position by the given Matrix</summary>
        /// <param name="pos">The position to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed position</returns>
        public static Vector2 Transform(Vector2 pos, Matrix4x4 mat)
        {
            Transform(ref pos, ref mat, out Vector2 result);
            return result;
        }

        /// <summary>Transform a Position by the given Matrix</summary>
        /// <param name="pos">The position to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void Transform(ref Vector2 pos, ref Matrix4x4 mat, out Vector2 result)
        {
            result.X = mat.M11 * pos.X + mat.M21 * pos.Y + mat.M31;
            result.Y = mat.M12 * pos.X + mat.M22 * pos.Y + mat.M32;
        }

        public static float Component(this Vector2 vector, int index) => index == 0 ? vector.X : vector.Y;

        public static void SetComponentComponent(this Vector2 vector, int index, float value)
        {
            if (index == 0)
                vector.X = value;
            else
                vector.Y = value;
        }
    }
}
