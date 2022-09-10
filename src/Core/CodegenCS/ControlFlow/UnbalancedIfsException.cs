using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS.ControlFlow
{
    /// <summary>
    /// When there is an unbalanced number of IF/ELSE/ENDIF. Each IF should have a matching ENDIF, and may have an optional ELSE.
    /// </summary>
    public class UnbalancedIfsException : Exception
    {
        /// <inheritdoc/>
        public UnbalancedIfsException() : base("Unbalanced number of IF/ELSE/ENDIF")
        {
        }
    }
}
