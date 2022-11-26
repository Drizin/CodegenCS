using CodegenCS.___InternalInterfaces___;
using CodegenCS.ControlFlow;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static CodegenCS.Utils.TypeUtils;
using System.Globalization;
using IOExtensions = global::CodegenCS.IO.Extensions;


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
        protected readonly StringWriter _innerWriter;

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
        [Obsolete("Please prefer Raw String Literals using $$\"\"\" text \"\"\" . Currently this property default to NONE (text is NOT adjusted)")]
        public MultilineBehaviorType MultilineBehavior { get; set; } = MultilineBehaviorType.None; // TODO: deprecate

        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

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
        bool _trimWhileWhitespace = false;
        public DependencyContainer DependencyContainer { get { return _dependencyContainer; } internal set { _dependencyContainer = value; } }
        protected DependencyContainer _dependencyContainer = new DependencyContainer();

        public RenderEnumerableOptions DefaultIEnumerableRenderOptions { get; set; } = RenderEnumerableOptions.LineBreaksWithAutoSpacer;
        #endregion

        #region ctors
        /// <summary>
        /// New CodegenTextWriter (using UTF-8 encoding). <br />
        /// </summary>
        /// <param name="textWriter">Inner TextWriter to write to</param>
        public CodegenTextWriter(StringWriter textWriter)
        {
            _innerWriter = textWriter;
            _encoding = _innerWriter.Encoding;
            _dependencyContainer.RegisterSingleton<ICodegenTextWriter>(() => this);
            _dependencyContainer.RegisterSingleton<CodegenTextWriter>(() => this);
            _dependencyContainer.RegisterSingleton<TextWriter>(() => this);
        }

        /// <summary>
        /// New CodegenTextWriter writing to an in-memory StringWriter
        /// </summary>
        public CodegenTextWriter() : this(new StringWriter())
        {
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
            if (!string.IsNullOrEmpty(indent))
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
        protected bool IsControlBlockActive { get; private set; } = true;
        protected void RefreshControlBlockActiveStatus()
        { 
            IsControlBlockActive = _controlFlowSymbols.All(s =>
                (s is IfSymbol && ((IfSymbol)s).IfConditionValue == true) ||
                (s is ElseSymbol && ((ElseSymbol)s).IfConditionValue == false)
            );
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
            if (!IsControlBlockActive || string.IsNullOrEmpty(value))
                return;
            if (_trimWhileWhitespace)
            {
                value = value.TrimStart();
                if (value.Length == 0)
                    return;
                else
                    _trimWhileWhitespace = false;
            }
            _innerWriter.Write(value);
            OnWritten(value);
        }
        
        /// <summary>
        /// Writes a new line
        /// </summary>
        /// <param name="newLine">Any newLine character (\r, \r\n, \n). If not defined will use default <see cref="NewLine"/> property </param>
        protected void InnerWriteNewLine(string newLine = null)
        {
            InnerWriteRaw(newLine ?? this.NewLine);
            _currentLine.Clear();
            _nextWriteRequiresLineBreak = false;
            _dontIndentCurrentLine = false;
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
                    InnerWriteNewLine(); // will normalize to writer this.NewLine
                else
                    InnerWriteNewLine(lineBreak);
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
        protected void InnerInlineAction(Action action)
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
                action();
                _levelIndent.Pop(); // TODO: if there were no linebreaks written we should restore currentLine - levelIndent should be a class to keep track of more context
            }
            else
            {
                action();
            }
        }
        #endregion

        #region InnerWriteFormattable: By using interpolated strings we can mix strings and action delegates, which will be lazy-evaluated (so will respect the order of execution)
        private static Regex _formattableArgumentRegex = new Regex(
              "{(?<ArgPos>\\d+)(:(?<Format>[^}]*))?}",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        /// <summary>
        /// All public Write methods pass through this method <br />.
        /// This method splits an interpolated string and writes block by block, doing smart-evaluation of arguments <br />
        /// In the interpolated strings we can mix literals and variables (like any interpolated string), but also Func/Action delegates, which are evaluated only during the rendering. <br />
        /// One advantage of passing delegates (Func&lt;FormattableString&gt;, Func&lt;string&gt;, Action, Action&lt;ICodegenTextWriter&gt; ) as {arguments} <br />
        /// is that we do NOT evaluate those arguments BEFORE the outer string is being written - they are only evaluated when needed <br />
        /// so we can capture the cursor position in current line, and preserve it if the arguments render multi-line strings.
        /// Any interpolated object should work fine (should keep cursor position), except for interpolated strings which are returned as string type - this is the only
        /// case where CodegenTextWriter loses the structured information (placeholder positions).
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

            #region if arg is IControlFlowSymbol
            if (arg is IControlFlowSymbol)
            {
                if (arg is IfSymbol)
                {
                    _controlFlowSymbols.Push((IControlFlowSymbol)arg); // just push IF with the condition-value
                    RefreshControlBlockActiveStatus();
                }
                else if (arg is ElseSymbol) // pop the previous IF and push the new ELSE with the previous IF condition-value
                {
                    if (!_controlFlowSymbols.Any())
                        throw new UnbalancedIfsException();
                    IControlFlowSymbol previousSymbol = _controlFlowSymbols.Pop();
                    if (!(previousSymbol is IfSymbol))
                        throw new UnbalancedIfsException();
                    _controlFlowSymbols.Push(new ElseSymbol(((IfSymbol)previousSymbol).IfConditionValue));
                    RefreshControlBlockActiveStatus();
                }
                else if (arg is EndIfSymbol)
                {
                    if (!_controlFlowSymbols.Any())
                        throw new UnbalancedIfsException();
                    IControlFlowSymbol previousSymbol = _controlFlowSymbols.Pop();
                    if (!(previousSymbol is IfSymbol) && !(previousSymbol is ElseSymbol))
                        throw new UnbalancedIfsException();
                    RefreshControlBlockActiveStatus();
                }
                else if (arg is TrimLeadingWhitespaceSymbol)
                {
                    var contents = _innerWriter.GetStringBuilder().ToString().TrimEnd();
                    if (contents.Length != _innerWriter.GetStringBuilder().Length)
                    {
                        _innerWriter.GetStringBuilder().Remove(contents.Length, _innerWriter.GetStringBuilder().Length - contents.Length);
                        var matches = _lineBreaksRegex.Matches(contents);
                        if (matches.Count == 0) // it's a single line
                        {
                            _currentLine.Clear().Append(_innerWriter.GetStringBuilder().ToString());
                        }
                        else
                        {
                            int lastLineBreakPos = matches[matches.Count - 1].Index;
                            int lastLineBreakEnd = matches[matches.Count - 1].Index + matches[matches.Count - 1].Length;
                            int previousLineBreakEnd = matches.Count == 1 ? 0 : matches[matches.Count - 2].Index + matches[matches.Count - 2].Length;
                            _currentLine.Clear().Append(_innerWriter.GetStringBuilder().ToString().Substring(previousLineBreakEnd));
                        }

                    }
                    //TODO: do we have to restore _nextWriteRequiresLineBreak and _dontIndentCurrentLine?
                }
                else if (arg is TrimTrailingWhitespaceSymbol)
                {
                    _trimWhileWhitespace = true;
                }
                else
                    throw new NotImplementedException();
                return;
            }
            #endregion

            // If we're inside an INACTIVE block of IF/ELSE (contents will be discarded according to the IF clauses in the stack) we can just skip until we find another IControlFlowSymbol
            if (!IsControlBlockActive)
                return;


            #region If Action delegate was wrapped using DelegateExtensions.WithArguments (that allow to specify specific args), resolve missing args (inject) and evaluate delegate.
            {
                object wrappedAction = null;

                if ((IsAssignableToGenericType(arg.GetType(), typeof(InlineAction<>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineAction<,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineAction<,,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineAction<,,,>)))
                    && (wrappedAction = ((PropertyInfo)arg.GetType().GetMember("Action").Single()).GetValue(arg)) != null) // unwrap inner Action
                {
                    var genericTypes = arg.GetType().GetGenericArguments(); // genericTypes contains both types that should be resolved/injected and others that were passed
                    Type genericAction;
                    switch (genericTypes.Length)
                    {
                        case 1: genericAction = typeof(Action<>).MakeGenericType(genericTypes); break;
                        case 2: genericAction = typeof(Action<,>).MakeGenericType(genericTypes); break;
                        case 3: genericAction = typeof(Action<,,>).MakeGenericType(genericTypes); break;
                        case 4: genericAction = typeof(Action<,,,>).MakeGenericType(genericTypes); break;
                        default: throw new NotImplementedException("Action<> can't have more than 4 generic types when used with WithArguments()");
                    }
                    MethodInfo actionInvokeMethod = genericAction.GetMethod("Invoke");
                    object[] actionInvokeMethodArgs = new object[genericTypes.Length];
                    for (int i = 0; i < genericTypes.Length; i++)
                    {
                        string propName = "Arg" + (i + 1).ToString();

                        // check if arg was provided. If not provided try to resolve/inject.
                        actionInvokeMethodArgs[i] = ((PropertyInfo)arg.GetType().GetMember(propName).Single()).GetValue(arg);
                        actionInvokeMethodArgs[i] = actionInvokeMethodArgs[i] ?? this.ResolveDependency(genericTypes[i]);
                    }

                    InnerInlineAction(() =>
                    {
                        actionInvokeMethod.Invoke(wrappedAction, actionInvokeMethodArgs);
                    });
                    return;
                }
            }
            #endregion

            #region If Func delegate was wrapped using DelegateExtensions.WithArguments (that allow to specify specific args), resolve missing args (inject) and invoke it
            {
                object wrappedFunc = null;

                if ((IsAssignableToGenericType(arg.GetType(), typeof(InlineFunc<,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineFunc<,,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineFunc<,,,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineFunc<,,,,>)))
                    && (wrappedFunc = ((PropertyInfo)arg.GetType().GetMember("Func").Single()).GetValue(arg)) != null) // unwrap inner Func
                {
                    // turns out the only "instance" we need is Func<T> itself (ienumerableCallbackAction), not the container type (the caller template)
                    var genericTypes = arg.GetType().GetGenericArguments(); // genericTypes contains both types that should be resolved/injected and others that were passed
                    Type genericFunc;
                    switch (genericTypes.Length)
                    {
                        case 2: genericFunc = typeof(Func<,>).MakeGenericType(genericTypes); break;
                        case 3: genericFunc = typeof(Func<,,>).MakeGenericType(genericTypes); break;
                        case 4: genericFunc = typeof(Func<,,,>).MakeGenericType(genericTypes); break;
                        case 5: genericFunc = typeof(Func<,,,,>).MakeGenericType(genericTypes); break;
                        default: throw new NotImplementedException("Func<> can't have more than 4 input types when used with WithArguments()");
                    }
                    MethodInfo funcInvokeMethod = genericFunc.GetMethod("Invoke");
                    object[] funcInvokeMethodArgs = new object[genericTypes.Length - 1]; // -1 because last type os func return
                    for (int i = 0; i < genericTypes.Length - 1; i++)
                    {
                        string propName = "Arg" + (i + 1).ToString();

                        // check if arg was provided. If not provided try to resolve/inject.
                        funcInvokeMethodArgs[i] = ((PropertyInfo)arg.GetType().GetMember(propName).Single()).GetValue(arg);
                        funcInvokeMethodArgs[i] = funcInvokeMethodArgs[i] ?? this.ResolveDependency(genericTypes[i]);
                    }

                    InnerInlineAction(() =>
                    {
                        // Func may write directly to the current writer (despite the results it returns), that's why we run inside InnerInlineAction block
                        // Then we recurse (evaluate the results, whatever type it is) - recursion allows usage of Func<Func>
                        var evaluatedArg = funcInvokeMethod.Invoke(wrappedFunc, funcInvokeMethodArgs);
                        InnerWriteFormattableArgument(evaluatedArg, format);
                    });
                    return;
                }
            }
            #endregion

            #region If arg is Func<> invoke it (unwrap it). If it requires arguments (Func<T, TResult> or Func<T1, T2, TResult>, etc) try to resolve and inject them
            if (IsAssignableToGenericType(arg.GetType(), typeof(Func<>)) ||
                IsAssignableToGenericType(arg.GetType(), typeof(Func<,>)) ||
                IsAssignableToGenericType(arg.GetType(), typeof(Func<,,>)) ||
                IsAssignableToGenericType(arg.GetType(), typeof(Func<,,,>)) ||
                IsAssignableToGenericType(arg.GetType(), typeof(Func<,,,,>)))
            {
                var genericTypes = arg.GetType().GetGenericArguments();
                Type genericFunc; // typeof(Func<T>) where T is IEnumerable, or Func<T, TResult>, etc.
                switch (genericTypes.Length - 1) // -1 because the last arg is the TResult
                {
                    case 0: genericFunc = typeof(Func<>).MakeGenericType(genericTypes); break;
                    case 1: genericFunc = typeof(Func<,>).MakeGenericType(genericTypes); break;
                    case 2: genericFunc = typeof(Func<,,>).MakeGenericType(genericTypes); break;
                    case 3: genericFunc = typeof(Func<,,,>).MakeGenericType(genericTypes); break;
                    case 4: genericFunc = typeof(Func<,,,,>).MakeGenericType(genericTypes); break;
                    case 5: genericFunc = typeof(Func<,,,,,>).MakeGenericType(genericTypes); break;
                    case 6: genericFunc = typeof(Func<,,,,,,>).MakeGenericType(genericTypes); break;
                    case 7: genericFunc = typeof(Func<,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 8: genericFunc = typeof(Func<,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 9: genericFunc = typeof(Func<,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 10: genericFunc = typeof(Func<,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 11: genericFunc = typeof(Func<,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 12: genericFunc = typeof(Func<,,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 13: genericFunc = typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 14: genericFunc = typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 15: genericFunc = typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 16: genericFunc = typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    default: throw new NotImplementedException("Func<> can't receive more than 16 arguments"); // 16 inputs and 1 TResult
                }

                var func = genericFunc.GetMethod("Invoke");
                var funcAction = arg;
                var funcArgs = new object[genericTypes.Length - 1]; // -1 because the return type is not passed to the Func
                for (int i = 0; i < genericTypes.Length - 1; i++)
                    funcArgs[i] = this.ResolveDependency(genericTypes[i]);
                InnerInlineAction(() =>
                {
                    // Func may write directly to the current writer (despite the results it returns), that's why we run inside InnerInlineAction block
                    // Then we recurse (evaluate the results, whatever type it is) - recursion allows usage of Func<Func>
                    var evaluatedArg = func.Invoke(arg, funcArgs);
                    InnerWriteFormattableArgument(evaluatedArg, format); // process the resulting type
                });
                return;
            }
            #endregion

            #region lazy evaluation: if arg is Action just invoke it
            if (arg as Action != null)
            {
                InnerInlineAction(() =>
                {
                    Action action = ((Action)arg);
                    action();
                });
                return;
            }
            #endregion

            #region lazy evaluation: if arg is Action Action<T> or Action<T1, T2>, etc, try to resolve arguments and invoke
            if (IsAssignableToGenericType(arg.GetType(), typeof(Action<>)) ||
                IsAssignableToGenericType(arg.GetType(), typeof(Action<,>)) ||
                IsAssignableToGenericType(arg.GetType(), typeof(Action<,,>)) ||
                IsAssignableToGenericType(arg.GetType(), typeof(Action<,,,>)))
            {
                var genericTypes = arg.GetType().GetGenericArguments();
                Type genericAction; // typeof(Action<T>) or Action<T1, T2>, etc.
                switch (genericTypes.Length)
                {
                    case 1: genericAction = typeof(Action<>).MakeGenericType(genericTypes); break;
                    case 2: genericAction = typeof(Action<,>).MakeGenericType(genericTypes); break;
                    case 3: genericAction = typeof(Action<,,>).MakeGenericType(genericTypes); break;
                    case 4: genericAction = typeof(Action<,,,>).MakeGenericType(genericTypes); break;
                    case 5: genericAction = typeof(Action<,,,,>).MakeGenericType(genericTypes); break;
                    case 6: genericAction = typeof(Action<,,,,,>).MakeGenericType(genericTypes); break;
                    case 7: genericAction = typeof(Action<,,,,,,>).MakeGenericType(genericTypes); break;
                    case 8: genericAction = typeof(Action<,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 9: genericAction = typeof(Action<,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 10: genericAction = typeof(Action<,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 11: genericAction = typeof(Action<,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 12: genericAction = typeof(Action<,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 13: genericAction = typeof(Action<,,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 14: genericAction = typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 15: genericAction = typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    case 16: genericAction = typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(genericTypes); break;
                    default: throw new NotImplementedException("Action<> can't receive more than 16 arguments");
                }
                var action = genericAction.GetMethod("Invoke");
                var funcAction = arg;
                var actionArgs = new object[genericTypes.Length];
                for (int i = 0; i < genericTypes.Length; i++)
                    actionArgs[i] = this.ResolveDependency(genericTypes[i]);
                InnerInlineAction(() =>
                {
                    action.Invoke(arg, actionArgs);
                });
                return;
            }
            #endregion


            #region if arg is regular string (regular string variable interpolated within another string)
            if (arg as string != null)
            {
                InnerInlineAction(() =>
                {
                    string subText = ((string)arg);
                    InnerWrite(AdjustMultilineString(subText));
                });
                return;
            }
            #endregion

            #region if arg is FormattableString - we have to preserve the structure (placeholders positions and objects)
            if (arg as FormattableString != null)
            {
                InnerInlineAction(() =>
                {
                    FormattableString subText = (FormattableString)arg;
                    InnerWriteFormattable(AdjustMultilineString(subText.Format), subText.GetArguments());
                });
                return;
            }
            #endregion

            #region if arg is IFormattable (Dates, integers, etc. All will be rendered according to current writer's culture).
            if (arg is IFormattable)
            {
                InnerWrite(((IFormattable)arg).ToString(format, this.CultureInfo));
                return;
            }
            #endregion
            #region If IEnumerable<T> was wrapped using IEnumerableExtensions.Render (that allow to specify custom EnumerableRenderOptions), unwrap.
            RenderEnumerableOptions enumerableRenderOptions = this.DefaultIEnumerableRenderOptions; // by default uses the CodegenTextWriter setting, but it may be overriden in the wrapper

            object ienumerableCallbackAction = null;
            MethodInfo ienumerableCallbackActionMethod = null;
            object[] ienumerableCallbackActionMethodArgs = null;

            object ienumerableCallbackFunc = null;
            MethodInfo ienumerableCallbackFuncMethod = null;
            object[] ienumerableCallbackFuncMethodArgs = null;

            if (typeof(IInlineIEnumerable).IsAssignableFrom(arg.GetType()))
            {
                if ((IsAssignableToGenericType(arg.GetType(), typeof(InlineIEnumerableAction<>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineIEnumerableAction<,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineIEnumerableAction<,,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineIEnumerableAction<,,,>)))
                    && (ienumerableCallbackAction = ((PropertyInfo)arg.GetType().GetMember("ItemAction").Single()).GetValue(arg)) != null)
                {
                    // turns out the only "instance" we need is Action<T> itself (ienumerableCallbackAction), not the container type (the caller template)
                    var genericTypes = arg.GetType().GetGenericArguments(); // genericTypes[0] is T of IEnumerable<T>, others should be resolved/injected.
                    Type genericAction;
                    switch (genericTypes.Length)
                    {
                        case 1: genericAction = typeof(Action<>).MakeGenericType(genericTypes); break;
                        case 2: genericAction = typeof(Action<,>).MakeGenericType(genericTypes); break;
                        case 3: genericAction = typeof(Action<,,>).MakeGenericType(genericTypes); break;
                        case 4: genericAction = typeof(Action<,,,>).MakeGenericType(genericTypes); break;
                        // 3 injected types, and 1 for enum item:
                        default: throw new NotImplementedException("Action<> can't have more than 4 generic types");
                    }
                    ienumerableCallbackActionMethod = genericAction.GetMethod("Invoke");
                    ienumerableCallbackActionMethodArgs = new object[genericTypes.Length];
                    // -1 because we skip the last element which is iterated through the ienumerable:
                    for (int i = 0; i < genericTypes.Length - 1; i++)
                        ienumerableCallbackActionMethodArgs[i] = this.ResolveDependency(genericTypes[i]);
                }

                if ((IsAssignableToGenericType(arg.GetType(), typeof(InlineIEnumerableFunc<,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineIEnumerableFunc<,,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineIEnumerableFunc<,,,>)) ||
                    IsAssignableToGenericType(arg.GetType(), typeof(InlineIEnumerableFunc<,,,,>)))
                    && (ienumerableCallbackFunc = ((PropertyInfo)arg.GetType().GetMember("ItemFunc").Single()).GetValue(arg)) != null)
                {
                    // turns out the only "instance" we need is Func<T> itself (ienumerableCallbackFunc), not the container type (the caller template)
                    var genericTypes = arg.GetType().GetGenericArguments(); // genericTypes[0] is T of IEnumerable<T>, others should be resolved/injected.
                    Type genericFunc;
                    switch (genericTypes.Length)
                    {
                        case 2: genericFunc = typeof(Func<,>).MakeGenericType(genericTypes); break;
                        case 3: genericFunc = typeof(Func<,,>).MakeGenericType(genericTypes); break;
                        case 4: genericFunc = typeof(Func<,,,>).MakeGenericType(genericTypes); break;
                        case 5: genericFunc = typeof(Func<,,,,>).MakeGenericType(genericTypes); break;
                        // 3 injected types, and 1 for enum item, 1 for returned FormatableString:
                        default: throw new NotImplementedException("Func<> can't have more than 5 generic types");
                    }
                    ienumerableCallbackFuncMethod = genericFunc.GetMethod("Invoke");
                    ienumerableCallbackFuncMethodArgs = new object[genericTypes.Length - 1]; // -1 because last is return type (FormattableString)
                                                                                                // -2 because we skip the last type which is the return type, and we skip the element before which is iterated through the ienumerable:
                    for (int i = 0; i < genericTypes.Length - 2; i++)
                        ienumerableCallbackFuncMethodArgs[i] = this.ResolveDependency(genericTypes[i]);
                }


                enumerableRenderOptions = ((IInlineIEnumerable)arg).RenderOptions ?? enumerableRenderOptions;
                arg = ((IInlineIEnumerable)arg).Items; // let the unwrapped list be processed later (it will be processed together with the optional Func/Action delegate and with the extracted rendering options)
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
                    int items = 0;
                    foreach (var item in list)
                    {
                        if (addMiddleSeparator)
                            WriteIEnumerableItemSeparator(enumerableRenderOptions, enumerableRenderOptions.BetweenItemsBehavior, previousItemWroteMultilines: previousItemWroteMultilines);

                        if (ienumerableCallbackActionMethod != null) // if there's a specific action to be executed for each item
                        {
                            ienumerableCallbackActionMethodArgs[ienumerableCallbackActionMethodArgs.Length-1] = item;
                            ienumerableCallbackActionMethod.Invoke(ienumerableCallbackAction, ienumerableCallbackActionMethodArgs);
                        }
                        else if (ienumerableCallbackFuncMethod != null) // if there's a specific func to be executed for each item
                        {
                            ienumerableCallbackFuncMethodArgs[ienumerableCallbackFuncMethodArgs.Length - 1] = item;
                            var renderedItem = (FormattableString) ienumerableCallbackFuncMethod.Invoke(ienumerableCallbackFunc, ienumerableCallbackFuncMethodArgs);
                            InnerWriteFormattableArgument(renderedItem, "");
                        }
                        else
                        {
                            InnerWriteFormattableArgument(item, "");
                        }
                        items++;
                        addMiddleSeparator = true;
                        string previousItem = this._innerWriter.ToString().Substring(previousPos); 
                        previousItemWroteMultilines = _lineBreaksRegex.Split(previousItem.Trim()).Length > 1; // at least 1 line break inside the rendered item
                        previousPos = this._innerWriter.ToString().Length;
                    }
                    if (items > 0) // write according to AfterLastItemBehavior
                        WriteIEnumerableItemSeparator(enumerableRenderOptions, enumerableRenderOptions.AfterLastItemBehavior, previousItemWroteMultilines);
                    else
                        WriteIEnumerableItemSeparator(enumerableRenderOptions, enumerableRenderOptions.EmptyListBehavior, previousItemWroteMultilines);
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

            #region else, just try ToString()
            InnerInlineAction(() =>
            {
                InnerWrite(arg.ToString());
            });
            #endregion

        }
        private void WriteIEnumerableItemSeparator(RenderEnumerableOptions options, ItemsSeparatorBehavior behavior, bool previousItemWroteMultilines)
        {
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
                    InnerWrite(options.CustomSeparator);
                    break;
                case ItemsSeparatorBehavior.RemoveLastLine:
                    this.RemoveLastLine();
                    break;
                case ItemsSeparatorBehavior.RemoveLastLineIfWhitespaceOnly:
                    this.RemoveLastLine(onlyIfAllWhitespace: true);
                    break;
                case ItemsSeparatorBehavior.ClearLastLine:
                    this.ClearLastLine();
                    break;
                case ItemsSeparatorBehavior.ClearLastLineIfWhitespaceOnly:
                    this.ClearLastLine(onlyIfAllWhitespace: true);
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
            InnerWriteNewLine();
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

        #region Text Manipulation
        public void RemoveLastLine(bool onlyIfAllWhitespace = false)
        {
            var contents = _innerWriter.GetStringBuilder().ToString();
            var matches = _lineBreaksRegex.Matches(contents);
            if (matches.Count == 0) // it's still a single line
            {
                if (onlyIfAllWhitespace && _innerWriter.ToString().Trim() != "")
                    return;
                _innerWriter.GetStringBuilder().Clear();
                _currentLine.Clear();
                return;
            }
            int lastLineBreakPos = matches[matches.Count - 1].Index;
            int lastLineBreakEnd = matches[matches.Count - 1].Index + matches[matches.Count - 1].Length;
            if (onlyIfAllWhitespace && _innerWriter.ToString().Substring(lastLineBreakEnd).Trim() != "")
                return;
            _innerWriter.GetStringBuilder().Remove(lastLineBreakPos, contents.Length - lastLineBreakPos);
            int previousLineBreakEnd = matches.Count == 1 ? 0 : matches[matches.Count - 2].Index + matches[matches.Count - 2].Length;
            _currentLine.Clear().Append(_innerWriter.GetStringBuilder().ToString().Substring(previousLineBreakEnd));
            //TODO: do we have to restore _nextWriteRequiresLineBreak and _dontIndentCurrentLine?
        }
        public void ClearLastLine(bool onlyIfAllWhitespace = false)
        {
            var contents = _innerWriter.GetStringBuilder().ToString();
            var matches = _lineBreaksRegex.Matches(contents);
            if (matches.Count == 0) // it's still a single line
            {
                if (onlyIfAllWhitespace && _innerWriter.ToString().Trim() != "")
                    return;
                _innerWriter.GetStringBuilder().Clear();
                _currentLine.Clear();
                return;
            }
            int lastLineBreakEnd = matches[matches.Count - 1].Index + matches[matches.Count - 1].Length;
            if (onlyIfAllWhitespace && _innerWriter.ToString().Substring(lastLineBreakEnd).Trim() != "")
                return;
            _innerWriter.GetStringBuilder().Remove(lastLineBreakEnd, contents.Length - lastLineBreakEnd);
            _currentLine.Clear();
            //TODO: do we have to restore _nextWriteRequiresLineBreak and _dontIndentCurrentLine?
        }
        #endregion

        #region I/O (SaveToFile)
        /// <summary>
        /// Writes current content to a new file. If the target file already exists, it is overwritten. <br />
        /// </summary>
        /// <param name="path">Absolute path</param>
        /// <param name="createFolder">If this is true (default is true) and target folder does not exist, it will be created</param>
        /// <param name="encoding">If not specified will save as UTF-8</param>
        [Obsolete("Please use CodegenCS.IO extensions: ICodegenTextWriter.SaveToFile() or ICodegenOutputFile.SaveToFolder()")]
        public void SaveToFile(string path, bool createFolder = true, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            FileInfo fi = new FileInfo(path);
            if (createFolder)
            {
                string folder = fi.Directory.FullName;
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            IOExtensions.SaveToFile(GetContents(), fi.FullName, Encoding.UTF8);
        }
        #endregion

        #region Contents
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

        #region Events
        public event EventHandler<WrittenEventArgs> Written;
        protected virtual void OnWritten(string writtenValue)
        {
            Written?.Invoke(this, new WrittenEventArgs(writtenValue));
        }
        #endregion
    }
}
