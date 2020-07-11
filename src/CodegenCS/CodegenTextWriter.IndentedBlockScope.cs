using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodegenCS
{
    partial class CodegenTextWriter
    {
        /// <summary>
        /// An IndentedBlockScope defines the scope of an indented block of text <br />
        /// When created it increases the indent level of the associated CodegenTextWriter, <br />
        /// and when disposed it decreases the indent level (so it's important to use it in a "using" statements to automatically dispose scope at end). <br /><br />
        /// 
        /// Most programming languages use indented blocks to control scope: <br />
        /// - Python-like languages define scope solely based on the indentation of the text blocks. <br />
        /// - C/Java-like language define scope based on curly braces but use indented blocks for readibility. <br /><br />
        /// 
        /// For convenience/conciseness this helper allows to define some text to be written BEFORE the indented block starts, <br />
        /// and some text to be written AFTER the indented block ends. <br /><br />
        /// 
        /// For convenience this helper will automatically add line breaks if it find "dirty" lines (lines which have some text but don't end in a line break): <br />
        /// - The indented block will always start on an empty (non-dirty) line (adds linebreak if previous write didn't end in line break) <br />
        /// - The indented block will always end on an empty (non-dirty) line (adds linebreak if previous write didn't end in line break) <br />
        /// - The text that is written before the block (if provided) will always start on an empty (non-dirty) line (adds linebreak if previous write didn't end in line break) <br />
        /// - The text that is written after the block (if provided) will always end "into" an empty (non-dirty) line (automatically adds final linebreak if not defined in text)
        /// </summary>
        protected class IndentedBlockScope : IDisposable
        {
            #region Members
            private readonly CodegenTextWriter _writer;
            private bool _disposed = false;
            private string _beforeBlock = null; // What to write immediately before the indented block starts (before the line break, yet with outer indentation level)
            private string _afterBlock = null; // What to write immediately after the indented block ends (right after the line break, back with outer indentation level)
            #endregion

            /// <summary>
            /// An IndentedBlockScope defines the scope of an indented block of text
            /// </summary>
            /// <param name="writer"></param>
            public IndentedBlockScope(CodegenTextWriter writer)
            {
                _writer = writer;
                StartBlock();
            }

            /// <summary>
            /// An IndentedBlockScope defines the scope of an indented block of text
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="beforeBlock">What to write immediately before the indented block starts (before the line break, yet with outer indentation level)</param>
            /// <param name="afterBlock">What to write immediately after the indented block ends (right after the line break, back with outer indentation level)</param>
            public IndentedBlockScope(CodegenTextWriter writer, string beforeBlock, string afterBlock)
            {
                _writer = writer;
                _beforeBlock = beforeBlock;
                _afterBlock = afterBlock;
                StartBlock();
            }
            private void StartBlock()
            {
                if (!string.IsNullOrEmpty(_beforeBlock))
                {
                    // If there's something to write before block, it's safe to assume that this should be on it's own line, right?
                    // e.g. if the writer is in a dirty state (current line didn't end with a linebreak), it doesn't make sense to start a "if" statement which would open a new IndentedBlockScope
                    _writer.EnsureEmptyLine(); 

                    _writer.InnerWrite(_beforeBlock);
                }

                // By definition an indented block cannot start on a dirty line
                _writer.EnsureEmptyLine();

                _writer.InnerIncreaseIndent();
            }
            private void Endblock()
            {
                _writer.InnerDecreaseIndent();

                // By definition what happens after an indented block needs to be written on a blank (non-dirty) line
                _writer.EnsureEmptyLine();

                if (!string.IsNullOrEmpty(_afterBlock))
                {
                    _writer.InnerWrite(_afterBlock);

                    // If we're writing something after the indented block finishes (after indent is reverted back) 
                    // we can assume that this block-finisher should end up in a clean (non-dirty) line
                    _writer.EnsureEmptyLine();
                }
            }

            /// <summary>
            /// When we dispose, we just restore back the old level of the writer
            /// </summary>
            public void Dispose()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(IndentedBlockScope));
                }
                Endblock();
            }
        }
    }
}
