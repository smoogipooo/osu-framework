function Check-Python {
    return (&{python -V}).Exception.Message
}

# Variables
$inspectCodePath = "~/.nuget/packages/jetbrains.resharper.commandlinetools/2019.1.1/tools/inspectcode.exe"
$nvikaPath = "~/.nuget/packages/nvika.msbuild/1.0.1/tools/NVika.exe"
$solution = "osu-framework.sln"
$tmpDir = "./tmp"
$toolsDir = "./tools"

# Setup
New-Item -Path $tmpDir -ItemType "directory" -Force | Out-Null
dotnet tool install CodeFileSanity --tool-path $toolsDir --version 0.0.33

if ((Check-Python)) {
    pip install httpbin waitress
    Start-Process waitress-serve -ArgumentList ("--listen=*:80", "httpbin:app") -NoNewWindow
}

# CodeFileSanity
./tools/CodeFileSanity.exe


# Build
dotnet build $solution

# InspectCode
$inspectCodeReport = "$tmpDir/inspectcodereport.xml"
Start-Process -FilePath $inspectCodePath -ArgumentList ($solution, "-o=$inspectCodeReport", "--verbosity=WARN") -NoNewWindow -Wait
Start-Process -FilePath $nvikaPath -ArgumentList ("parsereport", $inspectCodeReport, "--treatwarningsaserrors") -NoNewWindow -Wait