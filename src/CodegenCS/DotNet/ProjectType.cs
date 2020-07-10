using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodegenCS.DotNet
{
    /// <summary>
    /// There are basically 2 types of .NET Projects: SDK-Style, Non-SDK-Style (or Legacy) <br />
    /// Those types do not have a special extension - they can be csproj, vbproj, etc. <br />
    /// The Non-SDK style was the most popular until Visual Studio 2015, and is more complex because it describes all files. <br />
    /// The SDK style started with MSBuild 15 (Visual Studio 2017), and is more modern and more concise since it doesn't require to describe all files in the project. <br />
    /// The SDK style also replaces the old XPROJ/Project.JSON format which was introduced with NETCORE but is barely used now. 
    /// </summary>
    public enum ProjectType
    {
        /// <summary>
        /// MSBuild SDK-Style is the new .NET Standard - these project files auto-include files with certain extensions automatically from the project folder. <br />
        /// This change has been introduced with MSBuld 15 (Visual Studio 2017 /NetStandard /.NET Core) and makes it much easier to maintain project files. <br />
        /// </summary>
        SDKStyle,

        /// <summary>
        /// MSBuild Non-SDK-Style is the traditional project format, now considered legacy. It's more verbose than the new SDK-Style.
        /// </summary>
        NonSDKStyle,
    }
}
