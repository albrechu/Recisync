# Resisync
Recipe Sync is an application for created cross-plattform cooking recipes and 
sharable over the cloud.

## How it works
=> See docs/Programmbeilage_VS.docx

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