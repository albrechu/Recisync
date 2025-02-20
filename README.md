# Resisync
Recipe Sync is an application for created cross-plattform cooking recipes and 
sharable over the cloud.

## Documentation
This project is just a little sample made with .NET MAUI and Azure Cloud Storage. I made this for my C#/.NET class. 
The documentation contains private information and is in german, meaning I would need to remove stuff and translate everything. 
Therefore I will unfortunately keep it private. 

The application is simple enough anyway, and the way storage works here should not be used in a real application. For example 
are recipe files with their images (cover, recipe steps, ...) just kept in a remote Blob Storage and accessed via SAS-Token.
In a real application you would probably have a database like Cosmos DB with small sized information and URLs to the images
contained in a Blob Storage. Data should be accessed via something like Entra-ID on a Business-To-Consumer (B2C) basis.

## Project Structure
<pre>
  root
    ├── artifacts/               # Contains all build artifacts, e.g. resulting files(.exe) and intermediate files (.obj).
    ├── docs/                    # Contains documentation about the project 
    ├── src/                     # Source files
    |    ├── Resisync.Core/      # Project source files that are core to the app
    |    ├── Resisync.Core.UI/   # Project source files that are core to its appearance
    |    └── Resisync.App/       # Applications and deployment projects
    ├── tests/                   # Test code and test data
    ├── build.cmd/sh             # Build scripts for the projects src
    ├── LICENSE
    ├── NuGet.Config             # Used nuget packages
    ├── README.md
    └── Resisync.sln             # Solution
</pre>
