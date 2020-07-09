using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    /// <summary>
    /// MSBuildCodegenContext keeps track of multiple files which can be saved at once in the output folder, <br />
    /// while also tracking the MSBuild action for each output file
    /// </summary>
    public class MSBuildCodegenContext : CodegenContext<MSBuildActionType>
    {
        /// <inheritdocs />
        public MSBuildCodegenContext() : base(defaultType: MSBuildActionType.Compile)
        {

        }
    }


}
