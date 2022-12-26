namespace CodegenCS
{
    public partial class CodegenTextWriter
    {
        /// <summary>
        /// ScopeContexts are used to manage the indentation that is automatically added to each new line.
        /// Each context represents one "level" of indentation.
        /// By default each indentation level will add 4 spaces on top of previous level (controller by <see cref="CodegenTextWriter.IndentString"/>)
        /// (except for first level which doesn't add any indentation)
        /// 
        /// A new ScopeContext is created (and added to Stack) when:
        /// - Indent is explicitly controlled like <see cref="CodegenTextWriter.IncreaseIndent"/> and <see cref="CodegenTextWriter.DecreaseIndent"/>
        /// - Block Helpers like <see cref="CodegenTextWriter.WithCurlyBraces"/>) are used
        /// - An interpolated object is rendered (including IEnumerables). This means that any interpolated object that renders into multiple lines 
        ///   will "capture" the current indent of the line (whitespace or non-whitespace text that was added before the first interpolated object)
        ///   and will automatically "replay" (preserve) that indent in the subsequent lines
        /// </summary>
        protected internal class ScopeContext //TODO: maybe this class should be merged with IndentedBlockScope?
        {
            #region ctor
            private ScopeContext(CodegenTextWriter writer)
            {
                ParentTextWriter = writer;
                StartingPos = writer._innerWriter.GetStringBuilder().Length;
            }

            /// <inheritdoc />
            /// <param name="writer"></param>
            /// <param name="indentString">Indent String to be appended by this new scope</param>
            protected internal ScopeContext(CodegenTextWriter writer, string indentString) : this(writer)
            {
                IndentLevel = writer._scopeContexts.Peek().IndentLevel + (string.IsNullOrEmpty(indentString) ? 0 : 1);
                IndentString = writer._scopeContexts.Peek().IndentString + indentString;
            }

            protected internal static ScopeContext CreateRootContext(CodegenTextWriter writer)
            {
                return new ScopeContext(writer)
                {
                    IndentLevel = 0,
                    IndentString = string.Empty // The first level (outer level) doesn't append any indentation
                };
            }
            #endregion

            #region Members
            protected internal CodegenTextWriter ParentTextWriter { get; protected set; }

            /// <summary>
            /// The whole Indent prefix that should be applied to all lines under this context
            /// ( <see cref="_currentLine" /> doesn't include this auto-indent )
            /// </summary>
            protected internal string IndentString { get; protected set; }

            /// <summary>
            /// Indent was written to current line
            /// </summary>
            protected internal bool IndentWritten { get; set; } = false;

            protected internal int StartingPos { get; protected set; }

            protected internal int WhitespaceLines { get; set; }
            protected internal int NonWhitespaceLines { get; set; }

            /// <summary>
            /// Implicit Indent captured before the first placeholder in the current line
            /// </summary>
            protected internal string ImplicitIndentBeforeFirstPlaceHolder { get; set; } = null;

            /// <summary>
            /// When we're in the middle of a line and start an inline block (which could be multiline string), the first line don't need to be indented - only the next ones
            /// </summary>
            internal bool DontIndentFirstLine = false;

            protected internal int IndentLevel { get; set; }

            #endregion

            #region Methods
            /// <summary>
            /// This writes the whole indentation before the current line writes any text <br />
            /// E.g. Usually for IndentLevel 1 this would be 4 spaces, for IndentLevel 2 it would be 8 spaces. <br />
            /// Depending on settings it can be based on tabs, or different number of spaces, etc.
            /// </summary>
            protected internal void InnerIndentCurrentLine()
            {
                if (IndentWritten)
                    return;
                if (string.IsNullOrEmpty(this.IndentString))
                    return;
                // if DontIndentFirstLine is set it's because we're starting an inner block right "at cursor position"-  no need to indent again - we're already positioned!
                if (DontIndentFirstLine && WhitespaceLines == 0 && NonWhitespaceLines == 0)
                    return;
                ParentTextWriter.InnerWriteRaw(this.IndentString);
                IndentWritten = true;
            }
            #endregion
        }
    }
}
