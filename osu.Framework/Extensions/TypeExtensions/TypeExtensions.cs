// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.Extensions.TypeExtensions
{
    public static class TypeExtensions
    {
        private static string readableName(Type t, HashSet<Type> usedTypes)
        {
            usedTypes.Add(t);

            string result = t.Name;

            // Trim away amount of type arguments
            int amountTypeArgumentsPos = result.IndexOf("`", StringComparison.Ordinal);
            if (amountTypeArgumentsPos >= 0)
                result = result.Substring(0, amountTypeArgumentsPos);

            // We were declared inside another class. Preprend the name of that class.
            if (t.DeclaringType != null && !usedTypes.Contains(t.DeclaringType))
                result = readableName(t.DeclaringType, usedTypes) + "+" + result;

            if (t.IsGenericType)
            {
                var typeArgs = t.GetGenericArguments().Except(usedTypes);
                if (typeArgs.Any())
                    result += "<" + string.Join(",", typeArgs.Select(genType => readableName(genType, usedTypes))) + ">";
            }

            return result;
        }

        public static string ReadableName(this Type t) => readableName(t, new HashSet<Type>());

        /// <summary>
        /// Determines whether a type derives from or equivalent to another type.
        /// </summary>
        /// <remarks>
        /// This performs the same function as <see cref="Type.IsSubclassOf"/> but allows the two types to match.
        /// </remarks>
        /// <param name="t">The type to check.</param>
        /// <param name="c">The type to compare with.</param>
        /// <returns>True if <paramref name="t"/> derives from <paramref name="c"/> or is the same type as <paramref name="c"/>.</returns>
        public static bool IsSubclassOrTypeOf(this Type t, Type c) => t.IsSubclassOf(c) || t == c;
    }
}
