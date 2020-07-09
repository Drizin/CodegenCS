using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodegenCS
{
    /// <summary>
    /// All files in a Visual Studio project have a build action. The build action controls what happens to the file when the project is compiled. <br />
    /// The most common action (like for compiling CS files) is Compile.
    /// </summary>
    public enum MSBuildActionType
    {
        /// <summary>
        /// The file isn't part of the build in any way. This value can be used for documentation files such as "ReadMe" files, for example.
        /// </summary>
        None,

        /// <summary>
        /// The file is passed to the compiler as a source file.
        /// </summary>
        Compile,

        /// <summary>
        /// A file marked as Content can be retrieved as a stream by calling Application.GetContentStream.  <br />
        /// For ASP.NET projects, these files are included as part of the site when it's deployed.
        /// </summary>
        Content,

        /// <summary>
        /// The file is passed to the compiler as a resource to be embedded in the assembly.  <br />
        /// You can call System.Reflection.Assembly.GetManifestResourceStream to read the file from the assembly.
        /// </summary>
        EmbeddedResource,

        /// <summary>
        /// Special enum which means that the file will NOT be added to the CSPROJ <br />
        /// For .NET Framework projects (not NET Core) this means that the file will NOT be added to the CSPROJ (even if you use MSBuildProjectEditor) <br />
        /// For .NET Core projects (where all files are automatically considered as compilable except if explicitly removed) this means that the file will be explicitly REMOVED from the CSPROJ.
        /// </summary>
        NonProjectItem
    }
}
