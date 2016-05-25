# Cake Sample

An example of using [Cake](http://cakebuild.net/) (C# Make) to build .NET solutions.

The sample solution includes the following projects:

* Class library (which is also a NuGet package).
* Test project for the library.
* Web application that uses the library.

The build script illustrates common tasks on .NET builds, such as creating a NuGet package and publishing a web application to a local folder.

## How to run the sample

1. Clone the repo.
2. Open a powershell prompt on the repo folder.
3. Run `.\build.ps1`. This script is the Cake bootstrapper which will download Cake (if needed) and execute the `build.cake` script.
4. Check the outputs on the `dist` folder.
