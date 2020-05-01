// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Tests.Visual.UserInterface;

namespace osu.Framework.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchmarkRoslynTypeReferenceBuilder
    {
        private RoslynTypeReferenceBuilder referenceBuilder;
        private string solutionDirectory;

        private string drawableCsFile;

        [GlobalSetup]
        public void GlobalSetup()
        {
            solutionDirectory = getSolutionPath(new DirectoryInfo(Path.GetDirectoryName(typeof(Drawable).Assembly.Location)));

            // Drawable is a file that is unlikely to ever be moved without serious breaking changes and provides the largest type reference hierarchy.
            // A file path that is unlikely to ever change without serious breaking changes, but also .
            drawableCsFile = Path.Combine(solutionDirectory, "osu.Framework", "Graphics", "Drawable.cs");

            referenceBuilder = new RoslynTypeReferenceBuilder(typeof(Tests.Program).Assembly);
            referenceBuilder.Initialise(Directory.GetFiles(solutionDirectory, "*.sln").First()).Wait();

            static string getSolutionPath(DirectoryInfo d)
            {
                if (d == null)
                    return null;

                return d.GetFiles().Any(f => f.Extension == ".sln") ? d.FullName : getSolutionPath(d.Parent);
            }
        }

        [Benchmark]
        public void NestedMenus()
        {
            referenceBuilder.Reset();
            referenceBuilder.GetReferencedFiles(typeof(TestSceneNestedMenus), drawableCsFile).Wait();
        }
    }
}
