#addin nuget:?package=Cake.StyleCop&version=1.1.0
#tool "xunit.runner.console"

using Cake.Core.Diagnostics;
using System.Linq;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionFile = GetFiles("./*.sln").First();
var distDir = Directory("./dist");
var buildDir = Directory("./build");

Task("Clean")
	.IsDependentOn("Clean-Dist")	
	.Does(() => 
	{
		DotNetBuild(solutionFile, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Clean")
			.SetVerbosity(Verbosity.Minimal));
	});

Task("Clean-Dist")
	.Does(() => 
	{
		CleanDirectory(distDir);
	});

Task("Restore-Packages")
	.Does(() => 
	{
		NuGetRestore(solutionFile);
	});

Task("Build")
	.IsDependentOn("Restore-Packages")
    .Does(() =>
	{
		CreateDirectory(buildDir);

		Information("Running StyleCop analysis");
        StyleCopAnalyse(settings => settings
			.WithSolution(solutionFile)
			.WithSettings(File("./Settings.StyleCop"))
			.ToResultFile(buildDir + File("StyleCopViolations.xml")));
		
		Information("Running MSBuild");
		DotNetBuild(solutionFile, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Rebuild")
			.SetVerbosity(Verbosity.Minimal));
    });

Task("Test")
	.IsDependentOn("Build")
    .Does(() =>
	{
		XUnit2("./test/**/bin/**/*.Tests.dll", new XUnit2Settings {
			XmlReport = true,
			OutputDirectory = buildDir
		});
    });
	
Task("Create-Packages")
	.IsDependentOn("Test")
	.IsDependentOn("Clean-Dist")
	.Does(() =>
	{
		var packagesOutputDir = distDir + Directory("packages");
		CreateDirectory(packagesOutputDir);
		
		var projectFilesToPack = GetFiles("./src/**/*.nuspec").Select(f => f.ChangeExtension(".csproj"));
		NuGetPack(projectFilesToPack, new NuGetPackSettings
		{
			OutputDirectory = packagesOutputDir,
			Properties = new Dictionary<string, string> 
			{
				{ "Configuration", configuration }
			}
		});
	});

Task("Publish-Websites")
	.IsDependentOn("Test")
	.IsDependentOn("Clean-Dist")
	.Does(() =>
	{
		var projects = ParseSolution(solutionFile).Projects.Where(p => p.Name.EndsWith(".Web"));
		foreach(var project in projects)
		{
			Information("Publishing {0}", project.Name);
			
			var publishDir = distDir + Directory("web") + Directory(project.Name);

			DotNetBuild(project.Path, settings => settings
				.SetConfiguration(configuration)
				.WithProperty("DeployOnBuild", "true")
				.WithProperty("WebPublishMethod", "FileSystem")
				.WithProperty("DeployTarget", "WebPublish")
				.WithProperty("publishUrl", MakeAbsolute(publishDir).FullPath)
				.SetVerbosity(Verbosity.Minimal));

			Zip(publishDir, distDir + File(project.Name + ".zip"));
		}
	});
	
Task("Default")
	.IsDependentOn("Create-Packages")
	.IsDependentOn("Publish-Websites");

RunTarget(target);