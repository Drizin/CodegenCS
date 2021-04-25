using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS.InputModels
{
    /// <summary>
    /// JSON-based Input Models provide information to be used in Templates (generators)
    /// </summary>
    public interface IJsonInputModel : IInputModel
    {
        /// <summary>
        /// Json Schema following https://json-schema.org/ specs
        /// </summary>
        string Schema { get; }
    }
}
