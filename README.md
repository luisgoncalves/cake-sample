# Cake sample

An example of using [Cake](http://cakebuild.net/) (C# Make) to build .NET solutions.

The sample solution includes the following projects:

* Class library (which is also a NuGet package).
* Test project for the library.
* Web application that uses the library.
* Console application that uses the library.

The build script illustrates common tasks, namely:

- Running xUnit tests.
- Creating a NuGet package.
- Publishing a web application to a local folder.
- Transforming App.config files.

## Requirements

1. MS Build Tools (tested with v14)
1. .NET 4.5+ and .NET Target Pack 4.6.1 (or change projects' target framework)
1. Powershell

## How to run the sample

1. Clone the repo.
2. Open a powershell prompt on the repo folder.
3. Run `.\build.ps1`. This script is the Cake bootstrapper which will download Cake (if needed) and execute the `build.cake` script.
4. Check the outputs on the `dist` folder.

The default target creates the NuGet package and publishes the web and console applications. There's also a target to remove the build outputs. Just run  `.\build.ps1 -Target Clean`.