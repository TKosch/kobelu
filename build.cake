var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Clean")
    .Does(() =>
{
    CleanDirectories(string.Format("./**/obj/{0}", configuration));
    CleanDirectories(string.Format("./**/bin/{0}", configuration));
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
    MSBuild("./KoBeLUAdmin.sln", settings =>
    settings.SetConfiguration(configuration));
});

Task("Publish")
    .IsDependentOn("Build")
    .Does(() => 
{
    InnoSetup("./installer.iss");
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);
