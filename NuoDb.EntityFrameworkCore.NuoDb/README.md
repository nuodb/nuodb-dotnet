nuodb-dotnet
============

[![CircleCI](https://dl.circleci.com/insights-snapshot/gh/nuodb/nuodb-dotnet/master/build-and-test/badge.svg?window=30d)](https://app.circleci.com/insights/github/nuodb/nuodb-dotnet/workflows/build-and-test/overview?branch=master&reporting-window=last-30-days&insights-snapshot=true)

This is the official NuoDB .NET driver. It is no longer actively supported by NuoDB. The driver will be updated and tested periodically.

Windows Support
---------------

The .NET and .Net Core distribution for Windows is provided as part of the official
NuGet package at [https://www.nuget.org/packages/NuoDb.EntityFrameworkCore.NuoDb/](https://www.nuget.org/packages/NuoDb.EntityFrameworkCore.NuoDb/)

Linux Support
-------------------

Linux is fully supported through .NET. The driver can be built and run natively on Linux without Mono.

To build the driver on Linux:
1. Ensure you have the .NET SDK installed (version 8.0 or later recommended)
2. Clone the repository
3. Follow the build steps outlined in our CircleCI configuration file (.circleci/config.yml)

The CircleCI file contains the complete and tested build process for Linux environments.

License
-------------------
[NuoDB License](https://github.com/nuodb/nuodb-dotnet/blob/master/LICENSE.txt)
