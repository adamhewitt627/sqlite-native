### Abandoned
Unless I change my mind, I don't expect to come back to this project. It was created to solve the same problem as [SQLitePCL.raw](https://github.com/ericsink/SQLitePCL.raw), but since most other libraries are wrapping that one, using this library alongside it can make for some troublesome dependency resolution. (Especially if you want to also use a different SQLite provider.) I created this project largely for:
* My own experience with `DLLImport` and creating an open source library.
* To help a company project where we had written a more idiomatic database connection API, and just needed the raw library access.


# sqlite-native  [![Build status](https://ci.appveyor.com/api/projects/status/duwn5reoawrc60yx?svg=true)](https://ci.appveyor.com/project/adamhewitt627/sqlite-native) [![NuGet Status](http://img.shields.io/nuget/v/SqliteNative.NET.svg?style=flat)](https://www.nuget.org/packages/SqliteNative.NET/)
Lightweight C# wrapper around the native SQLite API. This does *not* provide any idiomatic C# API to make consuming a database feel at home in a .NET library. While those are nice, sometimes I've found it simpler in handling SQLite documentation to be able to map their API directly into your code. (It also means this can (in theory) stay up to date with SQLite releases by simply updating references.

Right now, this only includes native runtimes for Windows and UWP. iOS, Android, and beyond are planned.
