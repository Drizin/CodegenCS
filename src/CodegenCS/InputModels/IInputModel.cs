using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS.InputModels
{
    /// <summary>
    /// Input Models provide information to be used in Templates (generators)
    /// </summary>
    public interface IInputModel
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        string Id { get; }
    }
}
