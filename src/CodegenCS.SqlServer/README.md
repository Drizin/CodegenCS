# CodegenCS.SqlServer
C# Code and Scripts to extract schema from SQL database into JSON files.

Based on https://drizin.io/code-generation-in-c-csx-extracting-sql-server-schema/

# Description

By using a full-featured language (C#) and a full featured IDE (Visual Studio or Visual Studio Code) we can write complex and reusable scripts, with strong-typing, intellisense, debugging support, etc.
We can use Dapper, Newtonsoft, and other amazing libraries.
Generating code with C# is much easier (and less ugly) than using T4 templates - easier to read, easier to write, easier to debug, easier to reuse.  

This project contains C# code and a CSX (C# Script file) which executes the C# code. There's also a PowerShell Script which helps to launch the CSX script.
This is cross-platform code and can be embedded into any project (even a class library, there's no need to build an exe since CSX is just invoked by a scripting runtime).

This code only uses netstandard2.0 libraries, so any project (.NET Framework or .NET Core) can use these scripts.
Actually the scripts are executed using CSI (C# REPL), which is a scripting engine - the CSPROJ just helps us to test/compile, use NuGet packages, etc.

## Usage
Just copy these files into your project, tweak connection string, and execute the PowerShell script.

## Contributing
- This is a brand new project, and I hope with your help it can grow a lot. As I like to say, **If you’re writing repetitive code by hand, you’re stealing from your employer or from your client.**

If you you want to contribute, you can either:
- Fork it, optionally create a feature branch, commit your changes, push it, and submit a Pull Request.
- Drop me an email (http://drizin.io/pages/Contact/) and let me know how you can help. I really don't have much time and would appreciate your help.

Some ideas for next steps:
- Scripts to generate POCO classes (can be used by Dapper, PetaPoco or other micro-ORMs)
- Scripts to generate EFCore Entities/DbContext


## History
- 2020-07-05: Initial public version. See [blog post here](https://drizin.io/code-generation-in-c-csx-extracting-sql-server-schema/)

## License
MIT License
