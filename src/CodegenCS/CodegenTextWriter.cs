using System;
using System.Collections.Generic;
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
    ///    In case CodegenTextWriter uses these inline actions to write string-interpolation-based templates, but we could also use Razor, Dotliquid, Scriban, or any other template engine. <br /><br />
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
    public partial class CodegenTextWriter : TextWriter
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
        public MultilineBehaviorType MultilineBehavior = MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine;

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
        /// This includes whitespace which was explicitly written to the TextWrite, but DOES NOT COUNT indent-strings which were automatically generated by this text writer (InnerIndentCurrentLine). <br />
        /// </summary>

        StringBuilder _currentLine = new StringBuilder();
        bool _dontIndentCurrentLine = false; // when we're in the middle of a line and start an inline block (which could be multiline string), the first line don't need to be indented - only the next ones
        #endregion

        #region ctors
        /// <summary>
        /// New CodegenTextWriter writing to an in-memory StringWriter (using UTF-8 encoding). <br />
        /// You may choose when to save this file.
        /// </summary>
        public CodegenTextWriter()
        {
            _innerWriter = new StringWriter();
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// New CodegenTextWriter writing directly to a file. <br />
        /// Default encoding is UTF-8.
        /// </summary>
        /// <param name="filePath">Target file</param>
        public CodegenTextWriter(string filePath)
        {
            _innerWriter = new StreamWriter(filePath); // default encoding is UTF-8
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// New CodegenTextWriter writing directly to a file. 
        /// </summary>
        /// <param name="filePath">Target file</param>
        /// <param name="encoding">Encoding</param>
        public CodegenTextWriter(string filePath, Encoding encoding)
        {
            _innerWriter = new StreamWriter(filePath, append: false, encoding: encoding);
            _encoding = encoding;
        }

        /// <summary>
        /// New CodegenTextWriter writing to another (inner) textWriter
        /// </summary>
        /// <param name="textWriter">Inner TextWriter to write to</param>
        public CodegenTextWriter(TextWriter textWriter)
        {
            _innerWriter = textWriter;
            _encoding = textWriter.Encoding;
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
        /// Manually Increases indentation level, <br />
        /// so that the next text lines are all indented with an increased level. <br />
        /// If you're using helpers like WithIndent, WithCurlyBraces or WithPythonBlock you don't need to manually control indent level.
        /// </summary>
        public CodegenTextWriter IncreaseIndent()
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
        /// Manually Decreases indentation level, <br />
        /// so that the next text lines are all indented with an decreased level. <br />
        /// If you're using helpers like WithIndent, WithCurlyBraces or WithPythonBlock you don't need to manually control indent level.
        /// </summary>
        public CodegenTextWriter DecreaseIndent()
        {
            InnerDecreaseIndent();
            return this;
        }

        /// <summary>
        /// Ensures that current cursor position is not dirty (cursor position is zero). If dirty, writes line break
        /// </summary>
        /// <returns></returns>
        public CodegenTextWriter EnsureEmptyLine()
        {
            if (_currentLine.Length > 0)
                WriteLine();
            return this;
        }

        /// <summary>
        /// Opens a new indented Block. Will automatically handle increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent. <br />
        /// This method will automatically fix "dirty lines" (lines which do not end in a line break).
        /// </summary>
        /// <returns></returns>
        [Obsolete("Please prefer the Fluent-API methods, which receive an Action-delegate for writing inside the indented-block and return CodegenTextWriter")]
        public IDisposable WithIndent()
        {
            return new IndentedBlockScope(this);
        }

        /// <summary>
        /// Opens a new indented Block. Will automatically handle increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent. <br />
        /// This method will automatically fix "dirty lines" (lines which do not end in a line break).
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify something to be written BEFORE the indented block starts (before the automatic line break, yet with outer indentation)</param>
        /// <param name="afterBlock">Optional - you can specify something to be written immediately AFTER the block finishes (back with outer indentation)
        /// If you're closing with a curly brace you'll probably want to add a line-break after that curly brace.
        /// </param>
        /// <returns></returns>
        [Obsolete("Please prefer the Fluent-API methods, which receive an Action-delegate for writing inside the indented-block and return CodegenTextWriter")]
        public IDisposable WithIndent(string beforeBlock = null, string afterBlock = null)
        {
            return new IndentedBlockScope(this, beforeBlock, afterBlock);
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
            _currentLine.Clear();
        }
        #endregion

        #region Block-Scope: methods based on IndentedBlockScope(), including language-specific helpers
        /// <summary>
        /// Opens a new indented Curly-Braces Block. Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="curlyBracesStyle">How Curly-Braces are written. If not defined will use current CurleBracesStyleType property (default is C-Style, which starts the curly braces in its own line) </param>
        /// <returns></returns>
        [Obsolete("Please prefer the Fluent-API methods, which receive an Action-delegate for writing inside the indented-block and return CodegenTextWriter")]
        public IDisposable WithCurlyBraces(string beforeBlock, CurlyBracesStyleType curlyBracesStyle)
        {
            IDisposable innerBlock;
            switch (curlyBracesStyle)
            {
                case CurlyBracesStyleType.C:
                default:
                        innerBlock = new IndentedBlockScope(this, 
                            beforeBlock: 
                                (!string.IsNullOrEmpty(beforeBlock) ? (beforeBlock + NewLine) :("")) 
                                + "{", 
                            afterBlock: "}");
                    break;
                case CurlyBracesStyleType.Java:
                    innerBlock = new IndentedBlockScope(this,
                        beforeBlock:
                            (!string.IsNullOrEmpty(beforeBlock) ? (beforeBlock + " ") : (""))
                            + "{",
                        afterBlock: "}");
                    break;
            }
            return innerBlock;
        }
        /// <summary>
        /// Opens a new indented Curly-Braces Block. Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <returns></returns>
        [Obsolete("Please prefer the Fluent-API methods, which receive an Action-delegate for writing inside the indented-block and return CodegenTextWriter")]
        public IDisposable WithCurlyBraces(string beforeBlock) => WithCurlyBraces(beforeBlock: beforeBlock, curlyBracesStyle: this.CurlyBracesStyle);

        /// <summary>
        /// Opens a new indented Curly-Braces Block. Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Please prefer the Fluent-API methods, which receive an Action-delegate for writing inside the indented-block and return CodegenTextWriter")]
        public IDisposable WithCurlyBraces() => WithCurlyBraces(beforeBlock: null, curlyBracesStyle: this.CurlyBracesStyle);

        /// <summary>
        /// Opens a new indented Curly-Braces Block. Action delegate (lambda) should be used to write contents "inside" the curly-braces block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public CodegenTextWriter WithCurlyBraces(string beforeBlock, Action innerBlockAction)
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
        /// Opens a new indented Curly-Braces Block. Action delegate (lambda) should be used to write contents "inside" the curly-braces block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public CodegenTextWriter WithCurlyBraces(string beforeBlock, Action<CodegenTextWriter> innerBlockAction)
        {
            return WithCurlyBraces(beforeBlock, () => innerBlockAction(this));
        }

        /// <summary>
        /// Opens a new indented Python-Braces Block. Will automatically handle linebreaks, and increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <returns></returns>
        [Obsolete("Please prefer the Fluent-API methods, which receive an Action-delegate for writing inside the indented-block and return CodegenTextWriter")]
        public IDisposable WithPythonBlock(string beforeBlock)
        {
            IDisposable innerBlock = new IndentedBlockScope(this,
                beforeBlock:
                    (!string.IsNullOrEmpty(beforeBlock) ? (beforeBlock + " :") : ("")),
                afterBlock: "");
            return innerBlock;
        }
        /// <summary>
        /// Opens a new indented Python-style Block. Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts.</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the indented block.</param>
        /// <returns></returns>
        public CodegenTextWriter WithPythonBlock(string beforeBlock, Action innerBlockAction)
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
        /// Opens a new indented Python-style Block. Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts.</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the indented block.</param>
        /// <returns></returns>
        public CodegenTextWriter WithPythonBlock(string beforeBlock, Action<CodegenTextWriter> innerBlockAction)
        {
            return WithPythonBlock(beforeBlock, () => innerBlockAction(this));
        }



        /// <summary>
        /// Opens a new indented C-Style Block. Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the block starts (before curly braces)</param>
        /// <returns></returns>
        [Obsolete("Please prefer the Fluent-API methods, which receive an Action-delegate for writing inside the indented-block and return CodegenTextWriter")]
        public IDisposable WithCBlock(string beforeBlock = null) => WithCurlyBraces(beforeBlock, CurlyBracesStyleType.C);

        /// <summary>
        /// Opens a new indented C-style Block. Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public CodegenTextWriter WithCBlock(string beforeBlock, Action innerBlockAction)
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
        /// Opens a new indented C-style Block. Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public CodegenTextWriter WithCBlock(string beforeBlock, Action<CodegenTextWriter> innerBlockAction)
        {
            return WithCBlock(beforeBlock, () => innerBlockAction(this));
        }


        /// <summary>
        /// Opens a new indented Java-Style Block. Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the block starts (before curly braces)</param>
        /// <returns></returns>
        [Obsolete("Please prefer the Fluent-API methods, which receive an Action-delegate for writing inside the indented-block and return CodegenTextWriter")]
        public IDisposable WithJavaBlock(string beforeBlock = null) => WithCurlyBraces(beforeBlock, CurlyBracesStyleType.Java);

        /// <summary>
        /// Opens a new indented Java-style Block. Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public CodegenTextWriter WithJavaBlock(string beforeBlock, Action innerBlockAction)
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
        /// Opens a new indented Java-style Block. Action delegate (lambda) should be used to write contents "inside" the indented block <br />
        /// Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="innerBlockAction">Action delegate (lambda) should be used to write contents "inside" the curly-braces block.</param>
        /// <returns></returns>
        public CodegenTextWriter WithJavaBlock(string beforeBlock, Action<CodegenTextWriter> innerBlockAction)
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
            _innerWriter.Write(value);
            System.Diagnostics.Debug.Write(value);
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

            var matches = _lineBreaksRegex.Matches(value);

            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                string line = value.Substring(lastPos, matches[i].Index - lastPos);
                string lineBreak = value.Substring(matches[i].Index, matches[i].Length);
                lastPos = matches[i].Index + matches[i].Length;

                // if _dontIndentCurrentLine is set, it's because we're starting an inner block right "at cursor position"-  no need to indent again - we're already positioned!
                if (line.Length > 0 && _currentLine.Length == 0 && !_dontIndentCurrentLine)
                    InnerIndentCurrentLine();

                InnerWriteRaw(line);
                if (_normalizeLineEndings)
                    InnerWriteRaw(NewLine);
                else
                    InnerWriteRaw(lineBreak);
                _currentLine.Clear();
                _dontIndentCurrentLine = false;
            }
            string lastLine = value.Substring(lastPos);

            if (lastLine.Length > 0 && _currentLine.Length == 0 && !_dontIndentCurrentLine)
                InnerIndentCurrentLine();
            InnerWriteRaw(lastLine);
            _currentLine.Clear().Append(lastLine);
            _dontIndentCurrentLine = false;
        }
        #endregion

        #region Inline Actions: basically we "save" the current cursor position and subsequent lines written in the action (after the first line) will all be idented with the same starting position

        /// <summary>
        /// Invokes an inline action (which may reference current CodegenTextWriter and write to it) <br />
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
                // TODO: we should probably reuse InnerIncreaseIndent , but push not only a primitive string but an object containing info like "dontIndentCurrentLine=true" (which is currently from CodegenTextWriter)
                // maybe another IDisposable class like IndentedBlockScope
                _levelIndent.Push(indent);
                _dontIndentCurrentLine = true;
                _currentLine.Clear();
                inlineAction();
                _levelIndent.Pop();
            }
            else
            {
                inlineAction();
            }
        }

        /// <summary>
        /// Invokes an inline action (which may reference current CodegenTextWriter and write to it) <br />
        /// If the action writes multiple lines and current line has some manually-written whitespace, <br />
        /// this method will "save" current cursor position and the subsequent lines (after the first) will "preserve" the cursor position by prepending this manual indentation. <br />
        /// In other words, this will capture manually-written whitespace indentation (those whice are not yet tracked by the automatic indentation), and will consider this manual indentation and preserve it in subsequent lines.
        /// </summary>
        protected void ExecuteInlineAction(Action<CodegenTextWriter> inlineAction)
        {
            InnerInlineAction(() => inlineAction(this));
        }
        #endregion

        #region InnerWriteFormattable: By using interpolated strings we can mix strings and action delegates, which will be lazy-evaluated (so will respect the order of execution)
        private static Regex _formattableArgumentRegex = new Regex(
              "{\\d(:(?<Format>[^}]*))?}",
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
        /// One advantage of passing delegates (Func&lt;FormattableString&gt;, Func&lt;string&gt;, Action, Action&lt;CodegenTextWriter&gt; ) as {arguments} <br />
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
                InnerWrite(literal);
                // arguments[i] may not work because same argument can be used multiple times
                var arg = arguments[int.Parse(matches[i].Value.Substring(1, 1))];
                string argFormat = matches[i].Groups["Format"].Value;

                InnerWriteFormattableArgument(arg, argFormat);

                lastPos = matches[i].Index + matches[i].Length;
            }
            string lastPart = format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            InnerWrite(lastPart);
        }

        /// <summary>
        /// Interpolated strings used in CodegenTextWriter may contain as arguments (expressions) not only variables/expressions but also Action delegates. <br />
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
                    InnerWrite(subText);
                });
                return;
            }
            
            if (arg as Func<string> != null)
            {
                InnerInlineAction(() =>
                {
                    string exec = ((Func<string>)arg)();
                    InnerWrite(exec);
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


            #region if arg is Action<CodegenTextWriter> or Action<TextWriter>
            if (arg as Action<CodegenTextWriter> != null)
            {
                InnerInlineAction(() =>
                {
                    Action<CodegenTextWriter> action = ((Action<CodegenTextWriter>)arg);
                    action(this);
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

            // TODO: maybe instead of accepting IEnumerable<FormattableString>, IEnumerable<string>, IEnumerable<Func<FormattableString>>, IEnumerable<Func<string>>, 
            // we should just remove this and expect users to use extensions like Join() that process each item and add separators (default is NewLine) between the items

            #region if arg is IEnumerable<string> or IEnumerable<Func<string>>
            if ((interfaceType = interfaceTypes.SingleOrDefault(t =>
                t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                t.GetGenericArguments()[0].IsAssignableFrom(typeof(string))
                )) != null)
            {
                InnerInlineAction(() =>
                {
                    IEnumerable<string> list = (IEnumerable<string>)arg;
                    for (int j = 0; j < list.Count(); j++)
                    {
                        string item = list.ElementAt(j);
                        InnerWrite(item);
                    }
                });
                return;
            }

            if ((interfaceType = interfaceTypes.SingleOrDefault(t =>
                t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                t.GetGenericArguments()[0].IsGenericType &&
                t.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Func<>) &&
                t.GetGenericArguments()[0].GetGenericArguments()[0].IsAssignableFrom(typeof(string))
                )) != null)
            {
                InnerInlineAction(() =>
                {
                    IEnumerable<Func<string>> list = (IEnumerable<Func<string>>)arg;
                    for (int j = 0; j < list.Count(); j++)
                    {
                        Func<string> item = list.ElementAt(j);
                        InnerWrite(item());
                    }
                });
                return;
            }
            #endregion

            #region if arg is IEnumerable<FormattableString> or IEnumerable<Func<FormattableString>>
            if ((interfaceType = interfaceTypes.SingleOrDefault(t =>
                t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                t.GetGenericArguments()[0].IsAssignableFrom(typeof(FormattableString))
                )) != null)
            {
                InnerInlineAction(() =>
                {
                    IEnumerable<FormattableString> list = (IEnumerable<FormattableString>)arg;
                    for (int j = 0; j < list.Count(); j++)
                    {
                        FormattableString item = list.ElementAt(j);
                        InnerWriteFormattable(AdjustMultilineString(item.Format), item.GetArguments());
                    }
                });
                return;
            }
            
            if ((interfaceType = interfaceTypes.SingleOrDefault(t =>
                t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                t.GetGenericArguments()[0].IsGenericType &&
                t.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Func<>) &&
                t.GetGenericArguments()[0].GetGenericArguments()[0].IsAssignableFrom(typeof(FormattableString))
                )) != null)
            {
                InnerInlineAction(() =>
                {
                    IEnumerable<Func<FormattableString>> list = (IEnumerable<Func<FormattableString>>)arg;
                    for (int j = 0; j < list.Count(); j++)
                    {
                        Func<FormattableString> fnFormattable = list.ElementAt(j);
                        FormattableString formattable = fnFormattable();
                        InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
                    }
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
        #endregion

        #region public Write/WriteLine methods for formattable strings (which basically are shortcuts to InnerWriteFormattable())
        /// <summary>
        /// Writes to the stream/writer an interpolated string (which arguments can mix strings, variables, and also action delegates which will be lazy-evaluated)
        /// </summary>
        public CodegenTextWriter Write(FormattableString formattable)
        {
            InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer an interpolated string (which arguments can mix strings, variables, and also action delegates which will be lazy-evaluated) and a new line
        /// </summary>
        public CodegenTextWriter WriteLine(FormattableString formattable)
        {
            InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
            WriteLine();
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer a formatted string (like string.Format, where arguments are replaced in the string)
        /// </summary>
        public CodegenTextWriter Write(RawString format, params object[] arguments)
        {
            InnerWriteFormattable(AdjustMultilineString(format), arguments);
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer a formatted string (like string.Format, where arguments are replaced in the string) and a new line
        /// </summary>
        public CodegenTextWriter WriteLine(RawString format, params object[] arguments)
        {
            InnerWriteFormattable(AdjustMultilineString(format), arguments);
            WriteLine();
            return this;
        }

        /// <summary>
        /// Writes object to the stream/writer
        /// </summary>
        public CodegenTextWriter Write(object value)
        {
            // since we use RawString (to prioritize FormattableString overloads), it happens that strings may end up using this object overload.
            if (value is string)
                return Write((string)value);
            if (value is Func<string>)
                return Write(() => ((Func<string>)value)());
            if (value is Action)
            {
                ((Action)value).Invoke();
                return this;
            }
            if (value is Action<CodegenTextWriter>)
            {
                ((Action<CodegenTextWriter>)value).Invoke(this);
                return this;
            }

            InnerWriteFormattable(AdjustMultilineString(value.ToString()));
            return this;
        }

        /// <summary>
        /// Writes object and new line to the stream/writer
        /// </summary>
        public CodegenTextWriter WriteLine(object value)
        {
            // since we use RawString (to prioritize FormattableString overloads), it happens that strings may end up using this object overload.
            if (value is string)
                return WriteLine((string)value);
            if (value is Func<string>)
                return WriteLine(() => ((Func<string>)value)());
            if (value is Action)
            {
                ((Action)value).Invoke();
                WriteLine();
                return this;
            }
            if (value is Action<CodegenTextWriter>)
            {
                ((Action<CodegenTextWriter>)value).Invoke(this);
                WriteLine();
                return this;
            }

            InnerWriteFormattable(AdjustMultilineString(value.ToString()));
            WriteLine();
            return this;
        }
        #endregion

        #region public Write/WriteLine methods (which basically are shortcuts to InnerWrite())
        /// <summary>
        /// Writes to the stream/writer a new line
        /// </summary>
        public CodegenTextWriter WriteLine()
        {
            InnerWriteRaw(this.NewLine);
            _currentLine.Clear();
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer a plain string 
        /// </summary>
        public CodegenTextWriter Write(RawString value)
        {
            InnerWrite(AdjustMultilineString(value));
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer a plain string and a new line
        /// </summary>
        public CodegenTextWriter WriteLine(RawString value)
        {
            InnerWrite(AdjustMultilineString(value));
            WriteLine();
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer the result of Lazy evaluation of a Func&lt;string&gt;
        /// </summary>
        public CodegenTextWriter Write(Func<RawString> fnString)
        {
            string value = fnString();
            InnerWrite(AdjustMultilineString(value));
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer the result of Lazy evaluation of a Func&lt;string&gt; and a new line
        /// </summary>
        public CodegenTextWriter WriteLine(Func<RawString> fnString)
        {
            string value = fnString();
            InnerWrite(AdjustMultilineString(value));
            WriteLine();
            return this;
        }


        /// <summary>
        /// Writes to the stream/writer the result of Lazy evaluation of a Func&lt;FormattableString&gt;
        /// </summary>
        public CodegenTextWriter Write(Func<FormattableString> fnFormattableString)
        {
            FormattableString formattable = fnFormattableString();
            InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
            return this;
        }

        /// <summary>
        /// Writes to the stream/writer the result of Lazy evaluation of a Func&lt;FormattableString&gt; and a new line
        /// </summary>
        public CodegenTextWriter WriteLine(Func<FormattableString> fnFormattableString)
        {
            FormattableString formattable = fnFormattableString();
            InnerWriteFormattable(AdjustMultilineString(formattable.Format), formattable.GetArguments());
            WriteLine();
            return this;
        }


        /// <summary>
        /// Writes buffer to the stream/writer a plain string 
        /// </summary>
        public new CodegenTextWriter Write(char[] buffer)
        {
            InnerWrite(AdjustMultilineString(new string(buffer)));
            return this;
        }

        /// <summary>
        /// Writes buffer to the stream/writer a plain string 
        /// </summary>
        public new CodegenTextWriter Write(char[] buffer, int index, int count)
        {
            InnerWrite(AdjustMultilineString(new string(buffer, index, count)));
            return this;
        }

        /// <summary>
        /// Writes buffer to the stream/writer a plain string 
        /// </summary>
        public new CodegenTextWriter WriteLine(char[] buffer)
        {
            InnerWrite(AdjustMultilineString(new string(buffer)));
            WriteLine();
            return this;
        }

        /// <summary>
        /// Writes buffer to the stream/writer
        /// </summary>
        public new CodegenTextWriter WriteLine(char[] buffer, int index, int count)
        {
            InnerWrite(AdjustMultilineString(new string(buffer, index, count)));
            WriteLine();
            return this;
        }
        #endregion

        #region I/O (SaveToFile, GetContents)
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
            return _innerWriter.ToString();
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
    }
}
