using System.Threading;
#addin "nuget:?package=CodeFileSanity&version=0.0.36"
#addin "nuget:?package=JetBrains.ReSharper.CommandLineTools&version=2019.3.3"
#tool "nuget:?package=NVika.MSBuild&version=1.0.1"
var nVikaToolPath = GetFiles("./tools/NVika.MSBuild.*/tools/NVika.exe").First();

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Debug");
var version = "0.0.0";

var rootDirectory = new DirectoryPath("..");
var tempDirectory = new DirectoryPath("temp");
var artifactsDirectory = rootDirectory.Combine("artifacts");

// Used for dotnet format.
var slns = new[]
{
    rootDirectory.CombineWithFilePath("templates/template-empty/TemplateGame.sln"),
    rootDirectory.CombineWithFilePath("templates/template-flappy/FlappyDon.sln"),
};

// Used for inspectcode.
var desktopSlnfs = new[]
{
    rootDirectory.CombineWithFilePath("templates/template-empty/TemplateGame.Desktop.slnf"),
    rootDirectory.CombineWithFilePath("templates/template-flappy/FlappyDon.Desktop.slnf"),
};

// Used for compilation.
var desktopBuilds = rootDirectory.CombineWithFilePath("build/Desktop.proj");

// Used for packing.
var templateProject = rootDirectory.CombineWithFilePath("osu.Framework.Templates.csproj");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("DetermineAppveyorBuildProperties")
    .WithCriteria(AppVeyor.IsRunningOnAppVeyor)
    .Does(() => {
        version = $"0.0.{AppVeyor.Environment.Build.Number}";
        configuration = "Debug";
    });

Task("DetermineAppveyorDeployProperties")
    .WithCriteria(AppVeyor.IsRunningOnAppVeyor)
    .Does(() => {
        Environment.SetEnvironmentVariable("APPVEYOR_DEPLOY", "1");

        if (AppVeyor.Environment.Repository.Tag.IsTag)
        {
            AppVeyor.UpdateBuildVersion(AppVeyor.Environment.Repository.Tag.Name);
            version = AppVeyor.Environment.Repository.Tag.Name;
        }

        configuration = "Release";
    });

Task("Clean")
    .Does(() => {
        EnsureDirectoryExists(artifactsDirectory);
        CleanDirectory(artifactsDirectory);
    });

Task("Compile")
    .Does(() => {
        DotNetCoreBuild(desktopBuilds.FullPath, new DotNetCoreBuildSettings {
            Configuration = configuration,
            Verbosity = DotNetCoreVerbosity.Minimal,
        });
    });

// windows only because both inspectcore and nvika depend on net45
Task("InspectCode")
    .WithCriteria(IsRunningOnWindows())
    .IsDependentOn("Compile")
    .Does(() => {
        int returnCode = 0;

        foreach (var slnf in desktopSlnfs)
        {
            var inspectcodereport = tempDirectory.CombineWithFilePath("inspectcodereport.xml");

            InspectCode(slnf, new InspectCodeSettings {
                CachesHome = tempDirectory.Combine("inspectcode"),
                OutputFile = inspectcodereport,
                ArgumentCustomization = args => args.Append("--verbosity=WARN")
            });

            returnCode |= StartProcess(nVikaToolPath, $@"parsereport ""{inspectcodereport}"" --treatwarningsaserrors");
        }

        if (returnCode != 0)
            throw new Exception($"inspectcode failed with return code {returnCode}");
    });

Task("CodeFileSanity")
    .Does(() => {
        ValidateCodeSanity(new ValidateCodeSanitySettings {
            RootDirectory = rootDirectory.FullPath,
            IsAppveyorBuild = AppVeyor.IsRunningOnAppVeyor
        });
    });

Task("DotnetFormat")
    .Does(() => {
        foreach (var sln in slns)
        {
            DotNetCoreTool(sln.FullPath, "format", "--dry-run --check");
        }
    });

Task("PackTemplates")
    .Does(() => {
        DotNetCorePack(templateProject.FullPath, new DotNetCorePackSettings{
            OutputDirectory = artifactsDirectory,
            Configuration = configuration,
            Verbosity = DotNetCoreVerbosity.Quiet,
            ArgumentCustomization = args => {
                args.Append($"/p:Version={version}");
                args.Append($"/p:GenerateDocumentationFile=true");
                args.Append($"/p:NoDefaultExcludes=true");

                return args;
            }
        });
    });

Task("Publish")
    .WithCriteria(AppVeyor.IsRunningOnAppVeyor)
    .Does(() => {
        foreach (var artifact in GetFiles(artifactsDirectory.CombineWithFilePath("*").FullPath))
            AppVeyor.UploadArtifact(artifact);
    });

Task("Deploy")
    .IsDependentOn("Clean")
    .IsDependentOn("DetermineAppveyorBuildProperties")
    .IsDependentOn("CodeFileSanity")
    .IsDependentOn("DotnetFormat")
    .IsDependentOn("InspectCode")
    .IsDependentOn("DetermineAppveyorDeployProperties")
    .IsDependentOn("PackTemplates")
    .IsDependentOn("Publish");

RunTarget(target);;
