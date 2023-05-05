using System.Diagnostics;

namespace CodegenCS.ControlFlow
{
    [DebuggerDisplay("[RAW({Text,nq})]")]
    public sealed class WriteRawSymbol : IControlFlowSymbol
    {
        public string Text { get; set; }

        public WriteRawSymbol()
        {
        }
        internal WriteRawSymbol(string text)
        {
            Text = text;
        }
    }
}
