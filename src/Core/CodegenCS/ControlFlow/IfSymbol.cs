using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CodegenCS.ControlFlow
{
    [DebuggerDisplay("[IF({IfConditionValue,nq})]")]
    public class IfSymbol : IControlFlowSymbol
    {
        public bool IfConditionValue { get; set; }
        public IfSymbol(bool condition)
        {
            IfConditionValue = condition;
        }
    }
}
