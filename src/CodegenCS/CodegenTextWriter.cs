using CodegenCS.___InternalInterfaces___;
using CodegenCS.ControlFlow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodegenCS
{
    /// <summary>
    /// This text writer has some features to help code-generation tools: <br />
    /// - Will keep track of "Indent Levels", and will write whitespace-indents accordingly to the current level. <br />
    ///   It's possible to explicitly increase/decrease indent level <br />
    ///   It's possible to change the IndentString <br />
    ///   It's possible to increase level using "using (writer.WithIndent()) {...}". <br />
    ///   There's a shortcut to start C-style blocks: using (writer.WithCStyleBlock($"public class {myClass}")). Will automatically start "{" and close with "}" <br /><br />
    ///   
    /// - Allows to execute "inline actions", which mean that writer will save the current cursor position, and will run the action which may write a string. <br />
    ///   If the string has multiple lines, all lines starting from the second will "preserve" the same indent (cursor position) that was there when we started writing the first line. <br />
    ///   This means that it's possible to embed blocks "inline", while preserving the correct indent where the block should start. <br />
    ///    This allow us to use any kind of "template include" functions without having to manually control indentation. <br />
    ///    In case ICodegenTextWriter uses these inline actions to write string-interpolation-based templates, but we could also use Razor, Dotliquid, Scriban, or any other template engine. <br /><br />
    /// 
    /// - Allows to write complex templates using pure C# language and interpolated strings. <br />
    ///   Basically, we split any interpolated string, and write block by block, doing lazy-evaluation of arguments.  <br />
    ///   Since we also control indentation (and preserve indentation even when we run "inline actions" in the middle of the template), this works like a charm. <br /><br />
    ///   
    /// - For convenience, all multi-line string blocks will have the first empty line removed and Left Padding removed. <br />
    ///   This means that you can write the multi-line strings with any number of padding spaces, and yet those spaces will be ignored - so you can align the  <br />
    ///   generated code with the outer control code. <br /><br />
    ///   
    /// Summary of the method chain: <br />
    /// - _innerWriter is the underlying TextWriter. It's used by: <br />
    ///     - InnerWriteRaw is the only method which writes to _innerWriter. It's used by: <br />
    ///         - InnerIndentCurrentLine (which only writes whitespace indentation) <br />
    ///         - WriteLine (which only writes a raw "NewLine") <br />
    ///         - InnerWrite (which splits multiline blocks and writes NewLines and strings individually by using InnerWriteRaw. <br />
    ///           InnerWrite is where all Indentation is controlled and automatically managed. It can write multiline texts and correct "respect" parent indentation. <br />
    ///           InnerWrite is used by: <br />
    ///             - IndentedBlockScope class (which write _beforeBlock and _afterBlock - these usually are one-line, but if multiple lines they need to respect parent indentation) <br />
    ///             - All public Write* methods <br />
    ///             - InnerWriteFormattable (which writes interpolated strings which can mix strings, variables, and also action delegates which will be lazy-evaluated) <br />
    /// AdjustMultilineString will manipulate multi-line blocks (trim first line if it's empty, and will removes the left padding of the block by calculating the minimum number of spaces which happens in every line)<br />
    /// All public methods should call AdjustMultilineString to adjust the block before calling other methods.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public partial class CodegenTextWriter : TextWriter, ICodegenTextWriter
    {
        #region Members
        /// <summary>
        /// CodegenTextWriter will always write to an inner TextWriter. <br />
        /// This TextWriter can be explicitly defined; <br />
        /// Can be StreamWriter writing to a file; <br />
        /// Or can be an in-memory StringWriter.
        /// </summary>
        protected readonly TextWriter _innerWriter;

        /// <summary>
        /// Identify all types of line-breaks
        /// </summary>
        protected static readonly Regex _lineBreaksRegex = new Regex(@"(\r\n|\n|\r)", RegexOptions.Compiled);

        /// <summary>
        /// If true, will normalize line breaks by replacing different styles (\n, \r\n, \r, etc) by _innerWriter.NewLine
        /// </summary>
        protected bool _normalizeLineEndings = true;

        /// <summary>
        /// How multi-line text blocks are adjusted
        /// </summary>
        public MultilineBehaviorType MultilineBehavior { get; set; } = MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine;

        /// <summary>
        /// How multi-line text blocks are adjusted
        /// </summary>
        public enum MultilineBehaviorType
        {
            /// <summary>
            /// Do not remove manipulate multi-line text blocks (write them as they are). <br />
            /// You'll have to handle mixed indentation levels for text blocks and outer control code.
            /// </summary>
            None,
            /// <summary>
            /// Will remove the left padding of multi-line text blocks, by "untabbing" the block until some row "touches" the margin.
            /// </summary>
            TrimLeftPadding,
            /// <summary>
            /// Will remove left padding and will remove the first empty line
            /// </summary>
            TrimLeftPaddingAndRemoveFirstEmptyLine
        }

        /// <summary>
        /// How Curly-Braces are written
        /// </summary>
        public enum CurlyBracesStyleType
        {
            /// <summary>
            /// K&amp;R style (Kernighan &amp; Ritchie Style). <br />
            /// Used in most C/C++/C# code: <br /> 
            /// - There's a new line before opening curly braces. <br />
            /// - There's a new line after closing curly braces.
            /// </summary>
            C,

            /// <summary>
            /// Java style: <br />
            /// - No new line before opening curly braces <br />
            /// - There's a new line after closing curly braces. 
            /// </summary>
            Java
        }

        /// <summary>
        /// How Curly-Braces are written
        /// </summary>
        public CurlyBracesStyleType CurlyBracesStyle { get; set; } = CurlyBracesStyleType.C;

        /// <summary>
        /// Encoding
        /// </summary>
        protected readonly Encoding _encoding;


        /// <summary>
        /// This keeps tracks of what was written to the current line.<br /> 
        /// This includes whitespace which was explicitly or implicitally written, but DOES NOT COUNT indent-strings that were automatically generated by this text writer (InnerIndentCurrentLine). <br />
        /// In other words, it's the current line written after automatic indentation
        /// </summary>
        StringBuilder _currentLine = new StringBuilder();

        bool _nextWriteRequiresLineBreak = false;
        bool _dontIndentCurrentLine = false; // when we're in the middle of a line and start an inline block (which could be multiline string), the first line don't need to be indented - only the next ones
        public DependencyContainer DependencyContainer { get { return _dependencyContainer; } internal set { _dependencyContainer = value; } }
        protected DependencyContainer _dependencyContainer = new DependencyContainer();

        public RenderEnumerableOptions DefaultIEnumerableRenderOptions { get; set; } = RenderEnumerableOptions.LineBreaksWithAutoSpacer;
        #endregion

        #region ctors
        /// <summary>
        /// New CodegenTextWriter writing to another (inner) textWriter
        /// </summary>
        /// <param name="textWriter">Inner TextWriter to write to</param>
        public CodegenTextWriter(TextWriter textWriter)
        {
            _innerWriter = textWriter;
            _encoding = textWriter.Encoding;
            _dependencyContainer.RegisterSingleton<ICodegenTextWriter>(() => this);
            _dependencyContainer.RegisterSingleton<CodegenTextWriter>(() => this);
        }

        /// <summary>
        /// New CodegenTextWriter writing to an in-memory StringWriter (using UTF-8 encoding). <br />
        /// You may choose when to save this file.
        /// </summary>
        public CodegenTextWriter() : this(new StringWriter())
        {
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// New CodegenTextWriter writing directly to a file. <br />
        /// Default encoding is UTF-8.
        /// </summary>
        /// <param name="filePath">Target file</param>
        public CodegenTextWriter(string filePath) : this(new StreamWriter(filePath))
        {
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// New CodegenTextWriter writing directly to a file. 
        /// </summary>
        /// <param name="filePath">Target file</param>
        /// <param name="encoding">Encoding</param>
        public CodegenTextWriter(string filePath, Encoding encoding) : this(new StreamWriter(filePath, append: false, encoding: encoding))
        {
            _encoding = encoding;
        }
        #endregion

        #region Text Writer overrides
        /// <summary>
        /// The default line terminator string is a carriage return followed by a line feed ("\r\n"). <br />
        /// You may override it (Unix uses "\n", Apple use "\r"). <br />
        /// PS: Null will be replaced by the default terminator (use empty if appropriate).
        /// </summary>
        public override string NewLine { get { return _innerWriter.NewLine; } set { _innerWriter.NewLine = value; } } // use NewLine from the most inner writer


        /// <summary>
        /// The character encoding in which the output is written.
        /// </summary>
        public override Encoding Encoding => _encoding;

        /// <summary>
        /// Closes the current writer and releases any system resources associated with the writer.
        /// </summary>
        public override void Close() => _innerWriter.Close();

        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush() => _innerWriter.Flush();
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => _innerWriter.ToString();

        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _innerWriter.Flush();
            _innerWriter.Dispose();
            base.Dispose(disposing);
        }
        #endregion

        #region Indent-control: methods and members
        /// <summary>
        /// Each level of indentation may have it's own indentation marker <br />
        /// e.g. one block may have "    " (4 spaces), while other may have "-- " (SQL line-comment), etc.
        /// </summary>
        protected Stack<string> _levelIndent = new Stack<string>();
        
        /// <summary>
        /// Current IndentLevel
        /// </summary>
        public int IndentLevel { get { return _levelIndent.Count; } }
        
        /// <summary>
        /// Default Indentation marker is 4 strings. <br />
        /// You can change to whatever you want
        /// </summary>
        public string IndentString { get; set; } = "    ";

        /// <summary>
        /// Increases indentation level, so that the next text lines are all indented with an increased level. 
        /// </summary>
        protected void InnerIncreaseIndent()
        {
            _levelIndent.Push(IndentString);
        }

        /// <summary>
        /// Explicitly Increases indentation level, <br />
        /// so that the next text lines are all indented with an increased level. <br />
        /// If you're using helpers like WithIndent, WithCurlyBraces or WithPythonBlock you don't need to manually control indent level.
        /// </summary>
        public ICodegenTextWriter IncreaseIndent()
        {
            InnerIncreaseIndent();
            return this;
        }

        /// <summary>
        /// Decreases indentation level, so that the next text lines are all indented with a decreased level. 
        /// </summary>
        protected void InnerDecreaseIndent()
        {
            _levelIndent.Pop();
        }

        /// <summary>
        /// Explicitly Decreases indentation level, <br />
        /// so that the next text lines are all indented with an decreased level. <br />
        /// If you're using helpers like WithIndent, WithCurlyBraces or WithPythonBlock you don't need to manually control indent level.
        /// </summary>
        public ICodegenTextWriter DecreaseIndent()
        {
            InnerDecreaseIndent();
            return this;
        }

        /// <summary>
        /// Ensures that current cursor position is not dirty (cursor position is zero). If dirty, writes line break
        /// </summary>
        /// <returns></returns>
        public ICodegenTextWriter EnsureEmptyLine()
        {
            if (_currentLine.Length > 0) // currentLine review
                WriteLine();
            return this;
        }

        /// <summary>
        /// Ensures that if current line is dirty then nothing more can be written to this line, so the next write will enforce (render automatically) a line break.
        /// </summary>
        /// <returns></returns>
        public ICodegenTextWriter EnsureLineBreakBeforeNextWrite()
        {
            if (_currentLine.Length > 0)
                _nextWriteRequiresLineBreak = true;
            return this;
        }

        /// <summary>
        /// Explicitly opens a new indented Block (prefer using implicit indentation when possible). Will automatically handle increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent. <br />
        /// This method will automatically fix "dirty lines" (lines which do not end in a line break).
        /// </summary>
        /// <returns></returns>
        public IDisposable WithIndent()
        {
            return new IndentedBlockScope(this);
        }

        /// <summary>
        /// Explicitly opens a new indented Block (prefer using implicit indentation when possible). Will automatically handle increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent. <br />
        /// This method will automatically fix "dirty lines" (lines which do not end in a line break).
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify something to be written BEFORE the indented block starts (before the automatic line break, yet with outer indentation)</param>
        /// <param name="afterBlock">Optional - you can specify something to be written immediately AFTER the block finishes (back with outer indentation)
        /// If you're closing with a curly brace you'll probably want to add a line-break after that curly brace.
        /// </param>
        /// <returns></returns>
        public IDisposable WithIndent(string beforeBlock = null, string afterBlock = null)
        {
            return new IndentedBlockScope(this, AdjustMultilineString(beforeBlock), AdjustMultilineString(afterBlock));
        }

        /// <summary>
        /// Explicitly opens a new indented indented Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the block <br />
        /// Will automatically handle increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify something to be written BEFORE the indented block starts (before the automatic line break, yet with outer indentation)</param>
        /// <param name="afterBlock">Optional - you can specify something to be written immediately AFTER the block finishes (back with outer indentation)</param>
        /// <returns></returns>
        public ICodegenTextWriter WithIndent(string beforeBlock, string afterBlock, Action innerBlockAction)
        {
#pragma warning disable 612, 618 // WithCurlyBraces is currently public/obsolete - will be deprecated soon, to give preference to FluentAPI
            IDisposable innerBlock = WithIndent(beforeBlock, afterBlock);
#pragma warning restore 612, 618
            using (innerBlock)
            {
                innerBlockAction();
            }
            return this;
        }

        /// <summary>
        /// Explicitly opens a new indented indented Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the block <br />
        /// Will automatically handle increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify something to be written BEFORE the indented block starts (before the automatic line break, yet with outer indentation)</param>
        /// <param name="afterBlock">Optional - you can specify something to be written immediately AFTER the block finishes (back with outer indentation)</param>
        /// <returns></returns>
        public ICodegenTextWriter WithIndent(string beforeBlock, string afterBlock, Action<ICodegenTextWriter> innerBlockAction)
        {
            return WithIndent(beforeBlock, afterBlock, () => innerBlockAction(this));
        }


        /// <summary>
        /// This writes the whole indentation before the current line writes any text <br />
        /// E.g. Usually for IndentLevel 1 this would be 4 spaces, for IndentLevel 2 it would be 8 spaces. <br />
        /// Depending on settings it can be based on tabs, or different number of spaces, etc.
        /// </summary>
        protected void InnerIndentCurrentLine()
        {
            string indent = string.Join("", _levelIndent.Reverse().ToList());
            InnerWriteRaw(indent);
        }
        #endregion

        #region If-Else-Blocks
        /// <summary>
        /// Each level of indentation may have it's own indentation marker <br />
        /// e.g. one block may have "    " (4 spaces), while other may have "-- " (SQL line-comment), etc.
        /// </summary>
        protected Stack<IControlFlowSymbol> _controlFlowSymbols = new Stack<IControlFlowSymbol>();
        
        /// <summary>
        /// Returns true if there's no IF/ELSE block open, or if the current block is "active" (should NOT be discarded according to the IF clauses in the stack)
        /// </summary>
        protected bool IsControlBlockActive //TODO: is this hurting performance? we can update a variable when pushing/popping into _controlFlowSymbols
        { 
            get 
            {
                return _controlFlowSymbols.All(s =>
                    (s is IfSymbol && ((IfSymbol)s).IfConditionValue == true) ||
                    (s is ElseSymbol && ((ElseSymbol)s).IfConditionValue == false)
                );
            }
        }
        #endregion

        #region Block-Scope: methods based on IndentedBlockScope(), including language-specific helpers
        /// <summary>
        /// Explicitly opens a new indented Curly-Braces Block (prefer using implicit indentation when possible). Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="curlyBracesStyle">How Curly-Braces are written. If not defined will use current CurleBracesStyleType property (default is C-Style, which starts the curly braces in its own line) </param>
        /// <returns></returns>
        public IDisposable WithCurlyBraces(string beforeBlock, CurlyBracesStyleType curlyBracesStyle)
        {
            IDisposable innerBlock;
            switch (curlyBracesStyle)
            {
                case CurlyBracesStyleType.C:
                default:
                        innerBlock = new IndentedBlockScope(this, 
                            beforeBlock: 
                                (!string.IsNullOrEmpty(beforeBlock) ? (AdjustMultilineString(beforeBlock) + NewLine) :("")) 
                                + "{", 
                            afterBlock: "}");
                    break;
                case CurlyBracesStyleType.Java:
                    innerBlock = new IndentedBlockScope(this,
                        beforeBlock:
                            (!string.IsNullOrEmpty(beforeBlock) ? (AdjustMultilineString(beforeBlock) + " ") : (""))
                            + "{",
                        afterBlock: "}");
                    break;
            }
            return innerBlock;
        }
        /// <summary>
        /// Explicitly opens a new indented Curly-Braces Block (prefer using implicit indentation when possible). Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <returns></returns>
        public IDisposable WithCurlyBraces(string beforeBlock) => WithCurlyBraces(beforeBlock: beforeBlock, curlyBracesStyle: this.CurlyBracesStyle);

        /// <summary>
        /// Explicitly opens a new indented Curly-Braces Block (prefer using implicit indentation when possible). Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <returns></returns>
        public IDisposable WithCurlyBraces() => WithCurlyBraces(beforeBlock: null, curlyBracesStyle: this.CurlyBracesStyle);

        /// <summary>
        /// Explicitly opens a new indented Curly-Braces Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the curly-braces block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public ICodegenTextWriter WithCurlyBraces(string beforeBlock, Action innerBlockAction)
        {
#pragma warning disable 612, 618 // WithCurlyBraces is currently public/obsolete - will be deprecated soon, to give preference to FluentAPI
            IDisposable innerBlock = WithCurlyBraces(beforeBlock, CurlyBracesStyle);
#pragma warning restore 612, 618
            using (innerBlock)
            {
                innerBlockAction();
            }
            return this;
        }

        /// <summary>
        /// Explicitly opens a new indented Curly-Braces Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the curly-braces block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public ICodegenTextWriter WithCurlyBraces(string beforeBlock, Action<ICodegenTextWriter> innerBlockAction)
        {
            return WithCurlyBraces(beforeBlock, () => innerBlockAction(this));
        }

        /// <summary>
        /// Explicitly opens a new indented Python-Braces Block (prefer using implicit indentation when possible). Will automatically handle linebreaks, and increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <returns></returns>
        public IDisposable WithPythonBlock(string beforeBlock)
        {
            IDisposable innerBlock = new IndentedBlockScope(this,
                beforeBlock:
                    (!string.IsNullOrEmpty(beforeBlock) ? (AdjustMultilineString(beforeBlock) + " :") : ("")),
                afterBlock: "");
            return innerBlock;
        }
        /// <summary>
        /// Explicitly opens a new indented Python-style Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts.</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the indented block.</param>
        /// <returns></returns>
        public ICodegenTextWriter WithPythonBlock(string beforeBlock, Action innerBlockAction)
        {
#pragma warning disable 612, 618 // WithCBlock is currently public/obsolete - will be deprecated soon, to give preference to FluentAPI
            IDisposable innerBlock = WithPythonBlock(beforeBlock);
#pragma warning restore 612, 618

            using (innerBlock)
            {
                innerBlockAction();
            }
            return this;
        }
        /// <summary>
        /// Explicitly opens a new indented Python-style Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts.</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the indented block.</param>
        /// <returns></returns>
        public ICodegenTextWriter WithPythonBlock(string beforeBlock, Action<ICodegenTextWriter> innerBlockAction)
        {
            return WithPythonBlock(beforeBlock, () => innerBlockAction(this));
        }



        /// <summary>
        /// Explicitly opens a new indented C-Style Block (prefer using implicit indentation when possible). Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the block starts (before curly braces)</param>
        /// <returns></returns>
        public IDisposable WithCBlock(string beforeBlock = null) => WithCurlyBraces(beforeBlock, CurlyBracesStyleType.C);

        /// <summary>
        /// Explicitly opens a new indented C-style Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public ICodegenTextWriter WithCBlock(string beforeBlock, Action innerBlockAction)
        {
#pragma warning disable 612, 618 // WithCBlock is currently public/obsolete - will be deprecated soon, to give preference to FluentAPI
            IDisposable innerBlock = WithCBlock(beforeBlock);
#pragma warning restore 612, 618
            using (innerBlock)
            {
                innerBlockAction();
            }
            return this;
        }
        /// <summary>
        /// Explicitly opens a new indented C-style Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public ICodegenTextWriter WithCBlock(string beforeBlock, Action<ICodegenTextWriter> innerBlockAction)
        {
            return WithCBlock(beforeBlock, () => innerBlockAction(this));
        }


        /// <summary>
        /// Explicitly opens a new indented Java-Style Block (prefer using implicit indentation when possible). Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the block starts (before curly braces)</param>
        /// <returns></returns>
        public IDisposable WithJavaBlock(string beforeBlock = null) => WithCurlyBraces(beforeBlock, CurlyBracesStyleType.Java);

        /// <summary>
        /// Explicitly opens a new indented Java-style Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public ICodegenTextWriter WithJavaBlock(string beforeBlock, Action innerBlockAction)
        {
#pragma warning disable 612, 618 // WithJavaBlock is currently public/obsolete - will be deprecated soon, to give preference to FluentAPI
            IDisposable innerBlock = WithJavaBlock(beforeBlock);
#pragma warning restore 612, 618
            using (innerBlock)
            {
                innerBlockAction();
            }
            return this;
        }
        /// <summary>
        /// Explicitly opens a new indented Java-style Block (prefer using implicit indentation when possible). Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public ICodegenTextWriter WithJavaBlock(string beforeBlock, Action<ICodegenTextWriter> innerBlockAction)
        {
            return WithJavaBlock(beforeBlock, () => innerBlockAction(this));
        }


        #endregion

        #region Raw Writes to _innerWriter (raw methods don't add any indent): InnerWriteRaw
        /// <summary>
        /// This is the lowest-level Write method. All writing methods end up here.
        /// </summary>
        protected void InnerWriteRaw(string value)
        {
            if (IsControlBlockActive)
            {
                _innerWriter.Write(value);
                //System.Diagnostics.Debug.Write(value);
            }
        }
        #endregion

        #region InnerWrite(string value): this is the "heart" of the indentation-control. It writes line by line, and writes the indent strings before each new line
        /// <summary>
        /// This is the "heart" of the indentation-control. Basically, this split any multi-line string, and writes line by line.  <br />
        /// Before writing each new line we write the indent block, <br />
        /// which could for example be 8 spaces (4 spaces in first indent level and 4 spaces for second indent level), <br />
        /// or 2 tabs (one for each indent level), or any combination.
        /// </summary>
        /// <param name="value"></param>
        protected void InnerWrite(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (!IsControlBlockActive)
                return;

            var matches = _lineBreaksRegex.Matches(value);

            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                string line = value.Substring(lastPos, matches[i].Index - lastPos);
                string lineBreak = value.Substring(matches[i].Index, matches[i].Length);
                lastPos = matches[i].Index + matches[i].Length;

                // Previous block finished without a newline and decrease indent - so it makes sense to expect that new block should start on a non-dirty line
                if (line.Length > 0 && _nextWriteRequiresLineBreak)
                {
                    WriteLine();
                }

                // indent before starting writing a new line
                // if _dontIndentCurrentLine is set, it's because we're starting an inner block right "at cursor position"-  no need to indent again - we're already positioned!
                if (line.Length > 0 && _currentLine.Length == 0 && !_dontIndentCurrentLine)
                    InnerIndentCurrentLine();

                InnerWriteRaw(line);
                if (_normalizeLineEndings)
                    InnerWriteRaw(NewLine);
                else
                    InnerWriteRaw(lineBreak);
                _nextWriteRequiresLineBreak = false;
                _currentLine.Clear();
                _dontIndentCurrentLine = false;
            }
            string lastLine = value.Substring(lastPos);

            // Previous block finished without a newline and decrease indent - so it makes sense to expect that new block should start on a non-dirty line
            if (lastLine.Length > 0 && _nextWriteRequiresLineBreak)
            {
                WriteLine();
            }

            if (lastLine.Length > 0 && _currentLine.Length == 0 && !_dontIndentCurrentLine)
                InnerIndentCurrentLine();
            InnerWriteRaw(lastLine);
            _currentLine.Append(lastLine);
            _dontIndentCurrentLine = false;
        }
        #endregion

        #region Inline Actions: basically we "save" the current cursor position and subsequent lines written in the action (after the first line) will all be idented with the same starting position

        /// <summary>
        /// Invokes an inline action (which may reference current ICodegenTextWriter and write to it) <br />
        /// If the action writes multiple lines and current line has some manually-written whitespace, <br />
        /// this method will "save" current cursor position and the subsequent lines (after the first) will "preserve" the cursor position by prepending this manual indentation. <br />
        /// In other words, this will capture manually-written whitespace indentation (those whice are not yet tracked by the automatic indentation), and will consider this manual indentation and preserve it in subsequent lines.
        /// </summary>
        protected void InnerInlineAction(Action inlineAction)
        {
            string indent = _currentLine.ToString();
            if (indent != null && indent.Length > 0 && string.IsNullOrWhiteSpace(indent))
            {
                // TODO: we could "detect" if the current indent is multiple of 4 spaces, or tabs, and do the conversion if appropriate, etc.
                // TODO: we should probably reuse InnerIncreaseIndent , but push not only a primitive string but an object containing info like "dontIndentCurrentLine=true" (which is currently from ICodegenTextWriter)
                // maybe another IDisposable class like IndentedBlockScope
                _levelIndent.Push(indent);
                _dontIndentCurrentLine = true;
                _currentLine.Clear();
                inlineAction();
                _levelIndent.Pop(); // TODO: if there were no linebreaks written we should restore currentLine - levelIndent should be a class to keep track of more context
            }
            else
            {
                inlineAction();
            }
        }

        /// <summary>
        /// Invokes an inline action (which may reference current ICodegenTextWriter and write to it) <br />
        /// If the action writes multiple lines and current line has some manually-written whitespace, <br />
        /// this method will "save" current cursor position and the subsequent lines (after the first) will "preserve" the cursor position by prepending this manual indentation. <br />
        /// In other words, this will capture manually-written whitespace indentation (those whice are not yet tracked by the automatic indentation), and will consider this manual indentation and preserve it in subsequent lines.
        /// </summary>
        protected void ExecuteInlineAction(Action<ICodegenTextWriter> inlineAction)
        {
            InnerInlineAction(() => inlineAction(this));
        }
        #endregion

        #region InnerWriteFormattable: By using interpolated strings we can mix strings and action delegates, which will be lazy-evaluated (so will respect the order of execution)
        private static Regex _formattableArgumentRegex = new Regex(
              "{(?<ArgPos>\\d*)(:(?<Format>[^}]*))?}",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        /// <summary>
        /// All public Write methods pass through this method <br />.
        /// This method splits an interpolated string and writes block by block, doing lazy-evaluation of arguments <br />
        /// In the interpolated strings we can mix literals and variables (like any interpolated string), but also Func/Action delegates, which are evaluated only during the rendering. <br />
        /// One advantage of passing delegates (Func&lt;FormattableString&gt;, Func&lt;string&gt;, Action, Action&lt;ICodegenTextWriter&gt; ) as {arguments} <br />
        /// is that we do NOT evaluate those arguments BEFORE the outer string is being written - they are only evaluated when needed <br />
        /// so we can capture the cursor position in current line, and preserve it if the arguments render multi-line strings
        /// </summary>
        protected void InnerWriteFormattable(string format, params object[] arguments)
        {
            // this is like ICustomFormatter.Format, but writing directly to stream. Maybe we should add a IFormatProvider/ICustomFormatter associated to the TextWriter?

            if (string.IsNullOrEmpty(format))
                return;
            //https://www.meziantou.net/interpolated-strings-advanced-usages.htm
            var matches = _formattableArgumentRegex.Matches(format);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                // unescape escaped curly braces
                string literal = format.Substring(lastPos, matches[i].Index - lastPos).Replace("{{", "{").Replace("}}", "}");
                lastPos = matches[i].Index + matches[i].Length;
                InnerWrite(literal);
                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                string argFormat = matches[i].Groups["Format"].Value;
                object arg = arguments[argPos]; 

                InnerWriteFormattableArgument(arg, argFormat);
            }
            string lastPart = format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            InnerWrite(lastPart);
        }

        /// <summary>
        /// Interpolated strings used in ICodegenTextWriter may contain as arguments (expressions) not only variables/expressions but also Action delegates. <br />
        /// This method prints those arguments.
        /// </summary>
        protected void InnerWriteFormattableArgument(object arg, string format)
        {
            if (arg == null)
                return; 

            Type[] interfaceTypes = arg.GetType().GetInterfaces();
            Type interfaceType;

            #region if arg is string (most common case) or Func<string> (lazy-evaluated func which returns a string)
            if (arg as string != null)
            {
                InnerInlineAction(() =>
                {
                    string subText = ((string)arg);
                    InnerWrite(AdjustMultilineString(subText));
                });
                return;
            }
            
            if (arg as Func<string> != null)
            {
                InnerInlineAction(() =>
                {
                    string str = ((Func<string>)arg)();
                    InnerWrite(AdjustMultilineString(str));
                });
                return;
            }
            #endregion

            #region if arg is FormattableString or Func<FormattableString>
            if (arg as FormattableString != null)
            {
                InnerInlineAction(() =>
                {
                    FormattableString subText = (FormattableString)arg;
                    InnerWriteFormattable(AdjustMultilineString(subText.Format), subText.GetArguments());
                });
                return;
            }

            if (arg as Func<FormattableString> != null)
            {
                InnerInlineAction(() =>
                {
                    Func<FormattableString> fnFormattable = ((Func<FormattableString>)arg);
                    FormattableString formattable = fnFormattable();
                    InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
                });
                return;
            }
            #endregion

            #region if arg is IFormattable
            if (arg is IFormattable)
            {
                InnerWrite(((IFormattable)arg).ToString(format, System.Globalization.CultureInfo.InvariantCulture));
                return;
            }
            #endregion

            #region If IEnumerable<T> was wrapped using IEnumerableExtensions.Render (that allow to specify custom EnumerableRenderOptions), unwrap.
            RenderEnumerableOptions enumerableRenderOptions = this.DefaultIEnumerableRenderOptions; // by default uses the CodegenTextWriter setting, but it may be overriden in the wrapper
            if (typeof(IInlineIEnumerable).IsAssignableFrom(arg.GetType()))
            {
                enumerableRenderOptions = ((IInlineIEnumerable)arg).RenderOptions ?? enumerableRenderOptions;
                arg = ((IInlineIEnumerable)arg).Items;
                interfaceTypes = arg.GetType().GetInterfaces();
            }
            #endregion

            #region if arg is IEnumerable (which includes IEnumerable<T>) - which includes the case where use forgot to call .Render() extension

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(arg.GetType()))
            {
                int previousPos = this._innerWriter.ToString().Length;
                InnerInlineAction(() =>
                {
                    System.Collections.IEnumerable list = (System.Collections.IEnumerable)arg;
                    bool addMiddleSeparator = false;
                    bool previousItemWroteMultilines = false;
                    foreach (var item in list)
                    {
                        if (addMiddleSeparator)
                            WriteIEnumerableItemSeparator(enumerableRenderOptions, isLastItem: false, previousItemWroteMultilines: previousItemWroteMultilines);
                        InnerWriteFormattableArgument(item, "");
                        addMiddleSeparator = true;
                        string previousItem = this._innerWriter.ToString().Substring(previousPos); 
                        previousItemWroteMultilines = _lineBreaksRegex.Split(previousItem.Trim()).Length > 1; // at least 1 line break inside the rendered item
                        previousPos = this._innerWriter.ToString().Length;
                    }
                    WriteIEnumerableItemSeparator(enumerableRenderOptions, isLastItem: true, previousItemWroteMultilines);
                });
                return;
            }
            #endregion


            #region if arg is some Embedded Template (embedded using Template.Load<TemplateType>.Render<TModel>(model) - which creates a lazy-renderable IEmbeddedTemplateWithModel)
            if (typeof(__Hidden_IContextedTemplateWithModelWrapper).IsAssignableFrom(arg.GetType()))
            {
                var embeddedTemplateWrapper = (__Hidden_IContextedTemplateWithModelWrapper)arg;
                InnerInlineAction(() =>
                {
                    embeddedTemplateWrapper.Render(this, _dependencyContainer);
                });
                return;
            }
            #endregion

            #region If user by mistake interpolated a Template.Load<TemplateType> but forgot to invoke .Render() 
            if (IsAssignableToGenericType(arg.GetType(), typeof(IContextedTemplateWrapper<,>)))
            {
                var embeddedTemplateWrapper = (__Hidden_IContextedTemplateWrapper)arg;

                var templateType = arg.GetType().GetInterface("IContextedTemplateWrapper").GetGenericArguments()[0].GetType();
                // ContextedTemplateWrapper<TTemplate,> - but TTemplate can require models. If it doesn't require any model we can be lenient and just Render() it
                if (typeof(IBase0ModelTemplate).IsAssignableFrom(templateType))
                {
                    InnerInlineAction(() =>
                    {
                        IBase0ModelTemplate template = (IBase0ModelTemplate)embeddedTemplateWrapper.CreateTemplateInstance(_dependencyContainer);
                        TemplateRenderer.Render(template, this, _dependencyContainer);
                    });
                    return;
                }
                // EmbeddedTemplate<TModel> and EmbeddedTemplate <TModel1, TModel2> require a model which was not provided using Render()
                // try to output error in the container (which should go to Visual Studio errors through a VS Extension)
                try
                {
                    _dependencyContainer.Resolve<ICodegenContext>().Errors.Add($"Template of type {embeddedTemplateWrapper.TemplateType.FullName} was Loaded() but was not Rendered() with required models.");
                }
                catch { }
                this.Write("#ERROR#");

            }
            #endregion

            #region if arg is the typeof(T) of any IBase0ModelTemplate (those templates that do not require any TModel) we can just resolve (allowing injected dependencies in the constructor) and render
            if (arg is Type && typeof(IBase0ModelTemplate).IsAssignableFrom((Type)arg))
            {
                var template = (IBase0ModelTemplate)this.ResolveDependency((Type)arg);
                InnerInlineAction(() =>
                {
                    TemplateRenderer.Render(template, this, _dependencyContainer);
                });
                return;
            }
            #endregion


            #region if arg is Action<ICodegenTextWriter> or Action<TextWriter>
            if (arg as Action<ICodegenTextWriter> != null)
            {
                InnerInlineAction(() =>
                {
                    Action<ICodegenTextWriter> action = ((Action<ICodegenTextWriter>)arg);
                    action(this);
                });
                return;
            }
            if (arg as Action != null)
            {
                InnerInlineAction(() =>
                {
                    Action action = ((Action)arg);
                    action();
                });
                return;
            }

            if (arg as Action<TextWriter> != null)
            {
                InnerInlineAction(() =>
                {
                    Action<TextWriter> action = ((Action<TextWriter>)arg);
                    action(this);
                });
                return;
            }
            #endregion

            #region if arg is IControlFlowSymbol
            if (arg is IControlFlowSymbol)
            {
                if (arg is IfSymbol)
                    _controlFlowSymbols.Push((IControlFlowSymbol)arg); // just push IF with the condition-value
                else if (arg is ElseSymbol) // pop the previous IF and push the new ELSE with the previous IF condition-value
                {
                    if (!_controlFlowSymbols.Any())
                        throw new UnbalancedIfsException();
                    IControlFlowSymbol previousSymbol = _controlFlowSymbols.Pop();
                    if (!(previousSymbol is IfSymbol))
                        throw new UnbalancedIfsException();
                    _controlFlowSymbols.Push(new ElseSymbol(((IfSymbol)previousSymbol).IfConditionValue));
                }
                else if (arg is EndIfSymbol)
                {
                    if (!_controlFlowSymbols.Any())
                        throw new UnbalancedIfsException();
                    IControlFlowSymbol previousSymbol = _controlFlowSymbols.Pop();
                    if (!(previousSymbol is IfSymbol) && !(previousSymbol is ElseSymbol))
                        throw new UnbalancedIfsException();
                }
                else
                    throw new NotImplementedException();
                return;
            }
            #endregion


            #region else, just try ToString()
            InnerInlineAction(() =>
            {
                InnerWrite(arg.ToString());
            });
            #endregion

        }
        private void WriteIEnumerableItemSeparator(RenderEnumerableOptions options, bool isLastItem, bool previousItemWroteMultilines)
        {
            var behavior = (isLastItem) ? options.AfterLastItemBehavior : options.BetweenItemsBehavior;
            switch(behavior)
            {
                case ItemsSeparatorBehavior.WriteLineBreak:
                    WriteLine(); // add a line break (even if it's already there)
                    break;
                case ItemsSeparatorBehavior.EnsureLineBreak:
                    EnsureEmptyLine(); // if current line is dirty, add a line break (giving a new empty line)
                    break;
                case ItemsSeparatorBehavior.EnsureFullEmptyLine:
                    EnsureEmptyLine(); // if current line is dirty, add a line break (giving a new empty line)
                    WriteLine();       // then we forcibly add a full empty line
                    break;
                case ItemsSeparatorBehavior.EnsureFullEmptyLineAfterMultilineItems:
                    EnsureEmptyLine(); // if current line is dirty, add a line break (giving a new empty line)
                    if (previousItemWroteMultilines)
                        WriteLine();   // if item wrote multiple lines we add a full empty line
                    break;
                case ItemsSeparatorBehavior.EnsureLineBreakBeforeNextWrite:
                    EnsureLineBreakBeforeNextWrite(); 
                    break;
                case ItemsSeparatorBehavior.WriteCustomSeparator:
                    InnerWriteRaw(options.CustomSeparator); 
                    break;
                case ItemsSeparatorBehavior.None:
                    break;
            }
        }
        #endregion

        #region public Write/WriteLine methods for formattable strings (which basically are shortcuts to InnerWriteFormattable())
        /// <summary>
        /// Writes to the stream/writer an interpolated string (which arguments can mix strings, variables, and also action delegates which will be lazy-evaluated)
        /// </summary>
        public ICodegenTextWriter Write(FormattableString formattable)
        {
            InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer an interpolated string (which arguments can mix strings, variables, and also action delegates which will be lazy-evaluated) and a new line
        /// </summary>
        public ICodegenTextWriter WriteLine(FormattableString formattable)
        {
            InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
            WriteLine();
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer a formatted string (like string.Format, where arguments are replaced in the string)
        /// </summary>
        public ICodegenTextWriter Write(RawString format, params object[] arguments)
        {
            InnerWriteFormattable(AdjustMultilineString(format), arguments);
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer a formatted string (like string.Format, where arguments are replaced in the string) and a new line
        /// </summary>
        public ICodegenTextWriter WriteLine(RawString format, params object[] arguments)
        {
            InnerWriteFormattable(AdjustMultilineString(format), arguments);
            WriteLine();
            return this;
        }

        /// <summary>
        /// Writes object to the stream/writer
        /// </summary>
        public ICodegenTextWriter Write(object value)
        {
            // since we use RawString (to prioritize FormattableString overloads), it happens that strings may end up using this object overload.
            if (value is RawString)
                return Write((RawString)value);


            InnerWriteFormattableArgument(value, "");
            return this;
        }

        /// <summary>
        /// Writes object and new line to the stream/writer
        /// </summary>
        public ICodegenTextWriter WriteLine(object value)
        {
            // since we use RawString (to prioritize FormattableString overloads), it happens that strings may end up using this object overload.
            if (value is RawString)
                return WriteLine((RawString)value);

            InnerWriteFormattableArgument(value, "");
            WriteLine();
            return this;
        }
        #endregion

        #region public Write/WriteLine methods (which basically are shortcuts to InnerWrite())
        /// <summary>
        /// Writes to the stream/writer a new line
        /// </summary>
        public ICodegenTextWriter WriteLine()
        {
            InnerWriteRaw(this.NewLine);
            _currentLine.Clear();
            _nextWriteRequiresLineBreak = false;
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer a plain string 
        /// </summary>
        public ICodegenTextWriter Write(RawString value)
        {
            InnerWrite(AdjustMultilineString(value));
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer a plain string and a new line
        /// </summary>
        public ICodegenTextWriter WriteLine(RawString value)
        {
            InnerWrite(AdjustMultilineString(value));
            WriteLine();
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer the result of Lazy evaluation of a Func&lt;string&gt;
        /// </summary>
        public ICodegenTextWriter Write(Func<RawString> fnString)
        {
            string value = fnString();
            InnerWrite(AdjustMultilineString(value));
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer the result of Lazy evaluation of a Func&lt;string&gt; and a new line
        /// </summary>
        public ICodegenTextWriter WriteLine(Func<RawString> fnString)
        {
            string value = fnString();
            InnerWrite(AdjustMultilineString(value));
            WriteLine();
            return this;
        }


        /// <summary>
        /// Writes to the stream/writer the result of Lazy evaluation of a Func&lt;FormattableString&gt;
        /// </summary>
        public ICodegenTextWriter Write(Func<FormattableString> fnFormattableString)
        {
            FormattableString formattable = fnFormattableString();
            InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer the result of Lazy evaluation of a Func&lt;FormattableString&gt; and a new line
        /// </summary>
        public ICodegenTextWriter WriteLine(Func<FormattableString> fnFormattableString)
        {
            FormattableString formattable = fnFormattableString();
            InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
            WriteLine();
            return this;
        }


        /// <summary>
        /// Writes buffer to the stream/writer a plain string 
        /// </summary>
        public new ICodegenTextWriter Write(char[] buffer)
        {
            InnerWrite(AdjustMultilineString(new string(buffer)));
            return this;
        }

        /// <summary>
        /// Writes buffer to the stream/writer a plain string 
        /// </summary>
        public new ICodegenTextWriter Write(char[] buffer, int index, int count)
        {
            InnerWrite(AdjustMultilineString(new string(buffer, index, count)));
            return this;
        }

        /// <summary>
        /// Writes buffer to the stream/writer a plain string 
        /// </summary>
        public new ICodegenTextWriter WriteLine(char[] buffer)
        {
            InnerWrite(AdjustMultilineString(new string(buffer)));
            WriteLine();
            return this;
        }

        /// <summary>
        /// Writes buffer to the stream/writer
        /// </summary>
        public new ICodegenTextWriter WriteLine(char[] buffer, int index, int count)
        {
            InnerWrite(AdjustMultilineString(new string(buffer, index, count)));
            WriteLine();
            return this;
        }
        #endregion

        #region I/O (SaveToFile, GetContents, DebuggerDisplay)
        /// <summary>
        /// Writes current content (assuming it was in-memory writer) to a new file. If the target file already exists, it is overwritten. <br />
        /// </summary>
        /// <param name="path">Absolute path</param>
        /// <param name="createFolder">If this is true (default is true) and target folder does not exist, it will be created</param>
        public void SaveToFile(string path, bool createFolder = true)
        {
            FileInfo fi = new FileInfo(path);
            if (createFolder)
            {
                string folder = fi.Directory.FullName;
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            // If file exists with different case, delete to overwrite
            if (fi.Exists && new DirectoryInfo(fi.Directory.FullName).GetFiles(fi.Name).Single().Name != fi.Name)
                fi.Delete();
            File.WriteAllText(fi.FullName, GetContents(), Encoding);
        }

        /// <summary>
        /// Get full contents of current Writer
        /// </summary>
        /// <returns></returns>
        public string GetContents()
        {
            if (_controlFlowSymbols.Any())
                throw new UnbalancedIfsException();
            return _innerWriter.ToString();
        }

        private string DebuggerDisplay 
        { 
            get 
            {
                return _innerWriter.ToString() + 
                    (_controlFlowSymbols.Any() ? $" - (warning: there are {_controlFlowSymbols.Count} open IF block(s)" : "");
            }
        }
        #endregion

        #region Multi-line blocks can be conveniently used with any indentation, and we will correctly adjust the indentation of those blocks (TrimLeftPadding and TrimFirstEmptyLine)
        /// <summary>
        /// Given a text block (multiple lines), this removes the left padding of the block, by calculating the minimum number of spaces which happens in EVERY line.
        /// Then, other methods writes the lines one by one, which in case will respect the current indent of the writer.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected string AdjustMultilineString(string block)
        {
            if (string.IsNullOrEmpty(block))
                return null;
            if (MultilineBehavior == MultilineBehaviorType.None)
                return block;
            string[] parts = _lineBreaksRegex.Split(block);
            if (parts.Length <= 1) // no linebreaks at all
                return block;
            var nonEmptyLines = parts.Where(line => line.TrimEnd().Length > 0).ToList();
            if (nonEmptyLines.Count <= 1) // if there's not at least 2 non-empty lines, assume that we don't need to adjust anything
                return block;

            if (MultilineBehavior == MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine)
            {
                Match m = _lineBreaksRegex.Match(block);
                if (m != null && m.Success && m.Index == 0)
                {
                    block = block.Substring(m.Length); // remove first empty line
                    parts = _lineBreaksRegex.Split(block);
                    nonEmptyLines = parts.Where(line => line.TrimEnd().Length > 0).ToList();
                }
            }


            int minNumberOfSpaces = nonEmptyLines.Select(nonEmptyLine => nonEmptyLine.Length - nonEmptyLine.TrimStart().Length).Min();

            StringBuilder sb = new StringBuilder();

            var matches = _lineBreaksRegex.Matches(block);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                string line = block.Substring(lastPos, matches[i].Index - lastPos);
                string lineBreak = block.Substring(matches[i].Index, matches[i].Length);
                lastPos = matches[i].Index + matches[i].Length;

                sb.Append(line.Substring(Math.Min(line.Length, minNumberOfSpaces)));
                sb.Append(lineBreak);
            }
            string lastLine = block.Substring(lastPos);
            sb.Append(lastLine.Substring(Math.Min(lastLine.Length, minNumberOfSpaces)));

            return sb.ToString();
        }
        #endregion

        #region Dependency Injection Container
        /// <summary>
        /// Creates an instance of a dependency <typeparamref name="T"/> (usually a Template) and (if constructor needs) it injects ICodegenContext or ICodegenTextWriter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected T ResolveDependency<T>(params object[] otherDependencies) where T : class
        {
            return (T)_dependencyContainer.Resolve(typeof(T), otherDependencies);
        }

        /// <summary>
        /// Creates an instance of a dependency<paramref name= "type" /> (usually a Template) and(if constructor needs) it injects ICodegenContext or ICodegenTextWriter
        /// </summary>
        protected object ResolveDependency(Type type, params object[] otherDependencies)
        {
            return _dependencyContainer.Resolve(type, otherDependencies);
        }
        #endregion

        #region Templates
        
       
        
        public ICodegenTextWriter RenderTemplate(ICodegenTemplate template)
        {
            template.Render(this);
            return this;
        }

        public ICodegenTextWriter RenderTemplate(ICodegenStringTemplate template)
        {
            Write(() => template.Render());
            return this;
        }
        
        public IContextedTemplateWrapper<T, ICodegenTextWriter> LoadTemplate<T>(params object[] dependencies) where T : IBaseSinglefileTemplate
        {
            return new ContextedTemplateWrapper<T, ICodegenTextWriter>(typeof(T), dependencies) { CodegenTextWriter = this };
        }
        #endregion

        #region Utils
        static bool IsInstanceOfGenericType(Type genericType, object instance)
        {
            Type type = instance.GetType();
            return IsAssignableToGenericType(type, genericType);
        }
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }
        #endregion
    }
}
