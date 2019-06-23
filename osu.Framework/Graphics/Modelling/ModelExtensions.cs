// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Transforms;
using osuTK;

namespace osu.Framework.Graphics.Modelling
{
    public static class ModelExtensions
    {
        public static TransformSequence<T> RotateTo<T>(this TransformSequence<T> t, Vector3 newRotation, double duration = 0, Easing easing = Easing.None)
            where T : Model =>
            t.Append(o => o.RotateTo(newRotation, duration, easing));

        public static TransformSequence<T> Spin<T>(this TransformSequence<T> t, double revolutionDuration, ModelAxes axes, RotationDirection direction, Vector3 startRotation = default)
            where T : Model
        {
            float amount = direction == RotationDirection.Clockwise ? 360 : -360;
            Vector3 endRotation = startRotation + new Vector3(
                                      (axes & ModelAxes.X) > 0 ? amount : 0,
                                      (axes & ModelAxes.Y) > 0 ? amount : 0,
                                      (axes & ModelAxes.Z) > 0 ? amount : 0);

            return t.Loop(d => d.RotateTo(startRotation).RotateTo(endRotation, revolutionDuration));
        }

        public static TransformSequence<T> Spin<T>(this TransformSequence<T> t, double revolutionDuration, ModelAxes axes, RotationDirection direction, Vector3 startRotation, int numRevolutions)
            where T : Model
        {
            float amount = direction == RotationDirection.Clockwise ? 360 : -360;
            Vector3 endRotation = startRotation + new Vector3(
                                      (axes & ModelAxes.X) > 0 ? amount : 0,
                                      (axes & ModelAxes.Y) > 0 ? amount : 0,
                                      (axes & ModelAxes.Z) > 0 ? amount : 0);

            return t.Loop(0, numRevolutions, d => d.RotateTo(startRotation).RotateTo(endRotation, revolutionDuration));
        }

        public static TransformSequence<T> RotateTo<T>(this T model, Vector3 newRotation, double duration = 0, Easing easing = Easing.None)
            where T : Model
            => model.TransformTo(nameof(model.Rotation), newRotation, duration, easing);

        public static TransformSequence<T> Spin<T>(this T drawable, double revolutionDuration, ModelAxes axes, RotationDirection direction, Vector3 startRotation = default) where T : Model =>
            drawable.Delay(0).Spin(revolutionDuration, axes, direction, startRotation);

        public static TransformSequence<T> Spin<T>(this T drawable, double revolutionDuration, ModelAxes axes, RotationDirection direction, Vector3 startRotation, int numRevolutions) where T : Model =>
            drawable.Delay(0).Spin(revolutionDuration, axes, direction, startRotation, numRevolutions);
    }
}
