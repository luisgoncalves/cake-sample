# cake-sample
An example of using [CAKE](http://cakebuild.net/) to build .NET solutions.

The sample solution includes the following projects:

* Class library (also a NuGet package)
* Test project for the library
* Web application that uses the library

The build script illustrates common tasks on .NET builds, such as creating a NuGet package and publishing a web application to a local folder.

To try the build, just clone the repo, open a powershell prompt and run `.\build.ps1`. This bootstrapper will download CAKE and execute the `build.cake` script.
