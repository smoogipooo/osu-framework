// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.OpenGL;

namespace osu.Framework.Graphics.Shaders
{
    public abstract class UniformStorage<T> : IUniformWithValue<T>
        where T : struct, IEquatable<T>
    {
        public Shader Owner { get; }
        public string Name { get; }
        public int Location { get; }
        public int Count => array.Length;

        public bool HasChanged { get; private set; } = true;

        private readonly T[] array;

        internal UniformStorage(Shader owner, string name, int uniformLocation, int count)
        {
            Owner = owner;
            Name = name;
            Location = uniformLocation;

            array = new T[count];
        }

        public T this[int index]
        {
            get => array[index];
            set
            {
                if (value.Equals(array[index]))
                    return;

                array[index] = value;
                HasChanged = true;

                if (Owner.IsBound)
                    Update();
            }
        }

        public void Update()
        {
            if (!HasChanged) return;

            GLWrapper.SetUniform(this);
            HasChanged = false;
        }

        ref T IUniformWithValue<T>.GetValueByRef() => ref array[0];
    }
}
