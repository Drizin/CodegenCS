using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS.ControlFlow
{
    public class ElseSymbol : IControlFlowSymbol
    {
        public bool? IfConditionValue { get; set; }
        public ElseSymbol() // empty constructor is invoked by the template-text static getter, since the text doesn't know the previous if condition
        {
        }
        internal ElseSymbol(bool ifConditionValue) // but when this is processed by the TextWriter we have to check the current IF block and we know the state
        {
            IfConditionValue = ifConditionValue;
        }
    }

}
