// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Framework.Graphics.Shaders
{
    public class Uniform<T> : UniformStorage<T>
        where T : struct, IEquatable<T>
    {
        public T Value
        {
            get => this[0];
            set => this[0] = value;
        }

        public Uniform(Shader owner, string name, int uniformLocation)
            : base(owner, name, uniformLocation, 1)
        {
        }

        public void UpdateValue(ref T newValue) => this[0] = newValue;
    }
}
