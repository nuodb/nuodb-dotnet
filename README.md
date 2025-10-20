nuodb-dotnet
============

[![CircleCI](https://dl.circleci.com/status-badge/img/gh/nuodb/nuodb-dotnet/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/gh/nuodb/nuodb-dotnet/tree/master)

This repository contains the official NuoDB .NET driver. The driver is updated and tested periodically.

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
[BSD-3-Clause](https://github.com/nuodb/nuodb-dotnet/blob/master/LICENSE.txt)
