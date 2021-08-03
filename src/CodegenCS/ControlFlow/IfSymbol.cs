using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS.ControlFlow
{
    public class IfSymbol : IControlFlowSymbol
    {
        public bool IfConditionValue { get; set; }
        public IfSymbol(bool condition)
        {
            IfConditionValue = condition;
        }
    }
}
