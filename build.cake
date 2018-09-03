var target = Argument("target", "Default");

var output = Directory("./Setups/");

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./KoBeLUAdmin/bin/");
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./KoBeLUAdmin.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild("./KoBeLUAdmin.sln", new MSBuildSettings {
        Configuration = "Debug"
    });
});

Task("Publish")
    .IsDependentOn("Build")
    .Does(() => 
{
    InnoSetup("./installer.iss");
});

Task("Default")
    .IsDependentOn("Publish");

RunTarget(target);
