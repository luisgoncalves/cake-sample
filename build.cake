#addin "nuget:?package=Cake.StyleCop&version=1.1.0"
#addin "Cake.XdtTransform"
#addin "Cake.SemVer"
#tool "xunit.runner.console"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionFile = GetFiles("./*.sln").First();
var solution = new Lazy<SolutionParserResult>(() => ParseSolution(solutionFile));
var distDir = Directory("./dist");
var buildDir = Directory("./build");

Task("Clean")
	.IsDependentOn("Clean-Outputs")
	.Does(() => 
	{
		DotNetBuild(solutionFile, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Clean")
			.SetVerbosity(Verbosity.Minimal));
	});

Task("Clean-Outputs")
	.Does(() => 
	{
		if (DirectoryExists(buildDir))
		{
			CleanDirectory(buildDir);
		}
		else
		{
			CreateDirectory(buildDir);
		}

		if (DirectoryExists(distDir))
		{
			CleanDirectory(distDir);
		}
		else
		{
			CreateDirectory(distDir);
		}
	});

Task("StyleCop")
	.Does(() => 
	{
        StyleCopAnalyse(settings => settings
			.WithSolution(solutionFile)
			.WithSettings(File("./Settings.StyleCop"))
			.ToResultFile(buildDir + File("StyleCopViolations.xml")));
	});

Task("Build")
	.IsDependentOn("Clean-Outputs")
	.IsDependentOn("StyleCop")	
    .Does(() =>
	{
		NuGetRestore(solutionFile);

		DotNetBuild(solutionFile, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Rebuild")
			.SetVerbosity(Verbosity.Minimal));
    });

Task("Test")
	.IsDependentOn("Build")
    .Does(() =>
	{
		XUnit2(string.Format("./test/**/bin/{0}/*.Tests.dll", configuration), new XUnit2Settings {
			XmlReport = true,
			OutputDirectory = buildDir
		});
    });
	
Task("Packages")
	.IsDependentOn("Test")
	.WithCriteria(() => Jenkins.IsRunningOnJenkins)
	.Does(() =>
	{
		var projectFilesToPack = solution.Value
			.Projects
			.Where(p => FileExists(p.Path.ChangeExtension(".nuspec")))
			.Select(p => p.Path);
		
		foreach(var project in projectFilesToPack)
		{
			var assemblyInfo = ParseAssemblyInfo(project.GetDirectory().CombineWithFilePath("./Properties/AssemblyInfo.cs"));
			var assemblyVersion = ParseSemVer(assemblyInfo.AssemblyVersion); 
			var packageVersion = assemblyVersion.Change(prerelease: "pre" + Jenkins.Environment.Build.BuildNumber);

			NuGetPack(project, new NuGetPackSettings
			{
				OutputDirectory = distDir,
				Properties = new Dictionary<string, string> 
				{
					{ "Configuration", configuration }
				},
				Authors = new []{ "John Doe" },
				Version = packageVersion.ToString(),
			});
		}
	});

Task("Websites")
	.IsDependentOn("Test")
	.Does(() =>
	{
		var webProjects = solution.Value
			.Projects
			.Where(p => p.Name.EndsWith(".Web"));

		foreach(var project in webProjects)
		{
			Information("Publishing {0}", project.Name);
			
			var publishDir = distDir + Directory(project.Name);

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

Task("Consoles")
	.IsDependentOn("Test")
	.Does(() =>
	{
		var consoleProjects = solution.Value
			.Projects
			.Where(p => p.Name.EndsWith(".Console"));

		foreach(var project in consoleProjects)
		{
			Information("Publishing {0}", project.Name);

			var projectDir = project.Path.GetDirectory(); 
			var publishDir = distDir + Directory(project.Name);

			Information("Copying to output directory");
			CopyDirectory(
				projectDir.Combine("bin").Combine(configuration),
				publishDir);

			var configFile = publishDir + File(project.Name + ".exe.config");
			var transformFile = projectDir.CombineWithFilePath("App." + configuration + ".config");
			Information("Transforming configuration file");
			XdtTransformConfig(configFile, transformFile, configFile);

			Zip(publishDir, distDir + File(project.Name + ".zip"));
		}
	});
	
Task("Default")
	.IsDependentOn("Packages")
	.IsDependentOn("Websites")
	.IsDependentOn("Consoles");

RunTarget(target);