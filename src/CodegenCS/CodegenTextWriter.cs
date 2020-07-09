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
    ///   generated code with the outer control code.
    /// </summary>
    public class CodegenTextWriter : TextWriter
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
        /// This keeps tracks of what was written to the current line, NOT COUNTING indent-strings which were generated by this text writer.
        /// Example: if this text writer was already in IndentLevel 1 (4 spaces) when it started rendering a template, and the template opens a nested if block (adding 1 more indent level, totalling 8 spaces),
        /// This will be 4 (4 extra spaces not counting inherited indent).
        /// If we render an inner template (e.g. @Include ("SubTemplate")), the subtemplate should honor BOTH the current indent level and also the number of spaces before the template was included.
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

        #region Indent class
        internal class IndentedBlockScope : IDisposable
        {
            private readonly CodegenTextWriter _writer;
            private bool _disposed = false;
            private string _beforeBlock = null; // what to write immediately before we increase indent (before the line break, yet with outer indentation level)
            private string _afterBlock = null; // what to write immediately after we decrease indent (right after the line break, back with outer indentation level)
            // When we indent, we just get the IndentLevel of the writer, and increase it.
            public IndentedBlockScope(CodegenTextWriter writer)
            {
                _writer = writer;
                
                // line break and increase indent
                _writer.WriteLine();
                writer.IncreaseIndent();
            }
            public IndentedBlockScope(CodegenTextWriter writer, string beforeBlock, string afterBlock)
            {
                _writer = writer;
                _beforeBlock = beforeBlock;
                _afterBlock = afterBlock;
                _writer.WriteWithIndents(_beforeBlock);

                // line break and increase indent
                _writer.WriteLine();
                writer.IncreaseIndent();
            }

            // When we dispose, we just restore back the old level of the writer
            public void Dispose()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(IndentedBlockScope));
                }
                // line break and decrease indent
                _writer.DecreaseIndent();
                if (_afterBlock != null)
                    _writer.WriteWithIndents(_afterBlock);
            }
        }
        #endregion

        #region Indent-control: methods and members
        /// <summary>
        /// Each level of indentation may have it's own indentation marker <br />
        /// e.g. one block may have "    " (4 spaces), while other may have "-- " (SQL line-comment), etc.
        /// </summary>
        public Stack<string> _levelIndent = new Stack<string>();
        
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
        /// Increases indentation level
        /// </summary>
        public void IncreaseIndent()
        {
            _levelIndent.Push(IndentString);
        }

        /// <summary>
        /// Decreases indentation level
        /// </summary>
        public void DecreaseIndent()
        {
            _levelIndent.Pop();
        }

        /// <summary>
        /// If the current line has some manually-written whitespace, this method will "save" current cursor position, and in case the inline action writes a multi-line text,
        /// the subsequent lines (after the first) will "preserve" the cursor position by adding this extra indent.
        /// ("manually-written whitespace" does not count the automatic indents written by this class).
        /// </summary>
        public void ExecuteInlineAction(Action inlineAction)
        {
            string indent = _currentLine.ToString();
            if (indent != null && indent.Length > 0 && string.IsNullOrWhiteSpace(indent))
            {
                _levelIndent.Push(indent); // we could convert tabs to spaces or vice-versa
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
        /// Increases the indent level, and when disposed will decrease it.
        /// </summary>
        /// <returns></returns>
        public IDisposable WithIndent() => new IndentedBlockScope(this);

        /// <summary>
        /// Opens a new indented Block. Will automatically handle increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent. <br />
        /// This method will automatically write a line break right before the indented block starts, but will not automatically add a line break after it ends.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify something to be written BEFORE the indented block starts (before the automatic line break, yet with outer indentation)</param>
        /// <param name="afterBlock">Optional - you can specify something to be written immediately AFTER the block finishes (back with outer indentation)
        /// If you're closing with a curly brace you'll probably want to add a line-break after that curly brace.
        /// </param>
        /// <returns></returns>
        public IDisposable WithIndent(string beforeBlock = null, string afterBlock = null)
        {
            return new IndentedBlockScope(this, beforeBlock, afterBlock);
        }

        /// <summary>
        /// This writes the whole indentation level (e.g. if indent level is 2 it will be 2 levels of 4 spaces each) //TODO: WriteIndent(levels)?
        /// </summary>
        public void WriteIndent()
        {
            string indent = string.Join("", _levelIndent.Reverse().ToList());
            WriteRaw(indent);
            _currentLine.Clear();
        }
        #endregion

        #region Indent-control: Language helper methods based on WithIndent()
        /// <summary>
        /// Opens a new indented Curly-Braces Block. Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent. <br />
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the indented block starts (before curly braces).</param>
        /// <param name="style">How Curly-Braces are written. If not defined will use current CurleBracesStyleType property (default is C-Style, which starts the curly braces in its own line) </param>
        /// <returns></returns>
        public IDisposable WithCurlyBraces(string beforeBlock = null, CurlyBracesStyleType? style = null)
        {
            CurlyBracesStyleType bracesStyleType = style ?? this.CurlyBracesStyle;
            switch (bracesStyleType)
            {
                case CurlyBracesStyleType.C:
                default:
                    if (string.IsNullOrEmpty(beforeBlock) && _currentLine.Length == 0) // if we're already at the beginning of a new line, just open block, no need to break line
                        return WithIndent(
                            beforeBlock: "{",
                            afterBlock: "}" + Environment.NewLine);
                    else
                        return WithIndent(
                            beforeBlock: beforeBlock + this.NewLine + "{",
                            afterBlock: "}" + Environment.NewLine);
                case CurlyBracesStyleType.Java:
                    return WithIndent(
                        beforeBlock: beforeBlock + "{",
                        afterBlock: "}" + Environment.NewLine);
            }
        }


        /// <summary>
        /// Opens a new indented C-Style Block. Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the block starts (before curly braces)</param>
        /// <returns></returns>
        public IDisposable WithCBlock(string beforeBlock = null) => WithCurlyBraces(beforeBlock, CurlyBracesStyleType.C);

        /// <summary>
        /// Opens a new indented Java-Style Block. Will automatically handle opening and closing of curly braces, linebreaks, and increasing/decreasing indent.
        /// Should be disposed (use "using" block) to correctly close braces and decrease indent.
        /// </summary>
        /// <param name="beforeBlock">Optional - you can specify what is written BEFORE the block starts (before curly braces)</param>
        /// <returns></returns>
        public IDisposable WithJavascriptBlock(string beforeBlock = null) => WithCurlyBraces(beforeBlock, CurlyBracesStyleType.Java);

        #endregion

        #region NotImplemented Writes (based on char[])
        public override void Write(char[] buffer)
        {
            throw new NotImplementedException();
        }
        public override void Write(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }
        public override void WriteLine(char[] buffer)
        {
            throw new NotImplementedException();
        }
        public override void WriteLine(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Raw Writes (to _innerWriter) - these don't add any indent
        private void WriteRaw(string value)
        {
            _innerWriter.Write(value);
            System.Diagnostics.Debug.Write(value);
        }
        #endregion

        #region WriteWithIndents(string value): writes line by line, and writes the indent strings before each new line
        /// <summary>
        /// This is the "heart" of this class. Basically, we split any multi-line string, and write line by line. 
        /// Before writing each new line we write the indent block, which could for example be 8 spaces (4 spaces in first indent level and 4 spaces for second indent level),
        /// or 2 tabs (one for each indent level), or any combination.
        /// </summary>
        /// <param name="value"></param>
        protected void WriteWithIndents(string value)
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
                    WriteIndent();

                WriteRaw(line);
                if (_normalizeLineEndings)
                    WriteRaw(NewLine);
                else
                    WriteRaw(lineBreak);
                _currentLine.Clear();
                _dontIndentCurrentLine = false;
            }
            string lastLine = value.Substring(lastPos);

            if (lastLine.Length > 0 && _currentLine.Length == 0 && !_dontIndentCurrentLine)
                WriteIndent();
            WriteRaw(lastLine);
            _currentLine.Clear().Append(lastLine);
            _dontIndentCurrentLine = false;
        }
        #endregion

        #region WriteFormattable(string format, params object[] arguments) - Basically, we split any interpolated string, and write block by block, doing lazy-evaluation of arguments. 
        /// <summary>
        /// This is the "heart" of this class. Basically, we split any interpolated string, and write block by block, doing lazy-evaluation of arguments. 
        /// The idea of writing Func&lt;FormattableString&gt; is that we do NOT evaluate the {arguments} BEFORE the outer string is being written - they are only evaluated when needed
        /// so we can capture the cursor position in current line, and preserve-it if the arguments render multi-line strings
        /// </summary>
        protected void WriteFormattable(string format, params object[] arguments)
        {
            //https://www.meziantou.net/interpolated-strings-advanced-usages.htm
            var matches = Regex.Matches(format, @"{\d}");
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                // unescape escaped curly braces
                string text = format.Substring(lastPos, matches[i].Index - lastPos).Replace("{{", "{").Replace("}}", "}");
                WriteWithIndents(text);
                // arguments[i] may not work because same argument can be used multiple times
                var arg = arguments[int.Parse(matches[i].Value.Substring(1, 1))];

                if (arg == null)
                    arg = "";

                Type[] interfaceTypes = arg.GetType().GetInterfaces();
                Type interfaceType;
                if ((interfaceType = interfaceTypes.SingleOrDefault(t => 
                    t.IsGenericType && 
                    t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                    t.GetGenericArguments()[0].IsAssignableFrom(typeof(FormattableString))
                    )) != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        IEnumerable<FormattableString> list = (IEnumerable<FormattableString>)arg;
                        for(int j = 0; j < list.Count(); j++)
                        {
                            FormattableString item = list.ElementAt(j);
                            Write(item);
                            if (j < list.Count() - 1)
                                WriteLine();
                        }
                    });
                }
                else if ((interfaceType = interfaceTypes.SingleOrDefault(t =>
                    t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                    t.GetGenericArguments()[0].IsGenericType &&
                    t.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Func<>) &&
                    t.GetGenericArguments()[0].GetGenericArguments()[0].IsAssignableFrom(typeof(FormattableString))
                    )) != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        IEnumerable<Func<FormattableString>> list = (IEnumerable<Func<FormattableString>>)arg;
                        for (int j = 0; j < list.Count(); j++)
                        {
                            Func<FormattableString> item = list.ElementAt(j);
                            Write(item);
                            if (j < list.Count() - 1)
                                WriteLine();
                        }
                    });
                }
                else if ((interfaceType = interfaceTypes.SingleOrDefault(t =>
                    t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                    t.GetGenericArguments()[0].IsGenericType &&
                    t.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Func<>) &&
                    t.GetGenericArguments()[0].GetGenericArguments()[0].IsAssignableFrom(typeof(string))
                    )) != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        IEnumerable<Func<string>> list = (IEnumerable<Func<string>>)arg;
                        for (int j = 0; j < list.Count(); j++)
                        {
                            Func<string> item = list.ElementAt(j);
                            Write(item);
                            if (j < list.Count() - 1)
                                WriteLine();
                        }
                    });
                }
                else if ((interfaceType = interfaceTypes.SingleOrDefault(t =>
                    t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                    t.GetGenericArguments()[0].IsAssignableFrom(typeof(string))
                    )) != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        IEnumerable<string> list = (IEnumerable<string>)arg;
                        for (int j = 0; j < list.Count(); j++)
                        {
                            string item = list.ElementAt(j);
                            Write(item);
                            if (j < list.Count() - 1)
                                WriteLine();
                        }
                    });

                }
                else if (arg as FormattableString != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        FormattableString subText = (FormattableString)arg;
                        Write(subText);
                    });
                }
                else if (arg as Func<FormattableString> != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        Func<FormattableString> subText = ((Func<FormattableString>)arg);
                        Write(subText);
                    });
                }
                else if (arg as Action<CodegenTextWriter> != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        Action<CodegenTextWriter> action = ((Action<CodegenTextWriter>)arg);
                        action(this);
                    });
                }
                else if (arg as Action<TextWriter> != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        Action<TextWriter> action = ((Action<TextWriter>)arg);
                        action(this);
                    });
                }
                else if (arg as Func<string> != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        string exec = ((Func<string>)arg)();
                        Write(exec);
                    });
                }
                else if (arg as string != null)
                {
                    ExecuteInlineAction(() =>
                    {
                        string subText = ((string)arg);
                        Write(subText);
                    });
                }
                else
                {
                    ExecuteInlineAction(() =>
                    {
                        Write(arg.ToString());
                    });
                }

                lastPos = matches[i].Index + matches[i].Length;
            }
            string lastPart = format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            WriteWithIndents(lastPart);
        }
        #endregion

        #region public Write/WriteLine methods for formattable strings (which basically are shortcuts to WriteFormattable())
        public void Write(FormattableString formattable)
        {
            string format = formattable.Format;
            format = AdjustMultilineString(format);
            WriteFormattable(format, formattable.GetArguments());
        }
        public void WriteLine(FormattableString formattable)
        {
            Write(formattable);
            WriteLine();
        }

        public override void Write(string format, params object[] arguments)
        {
            if (string.IsNullOrEmpty(format))
                return;
            format = AdjustMultilineString(format);

            WriteFormattable(format, arguments);
        }
        public override void WriteLine(string format, params object[] arguments)
        {
            Write(format, arguments);
            WriteLine();
        }
        public void Write(Func<FormattableString> fnFormattable)
        {
            FormattableString formattable = fnFormattable();
            Write(formattable);
        }
        public void WriteLine(Func<FormattableString> fnFormattable)
        {
            Write(fnFormattable());
            WriteLine();
        }
        #endregion

        #region public Write/WriteLine methods (which basically are shortcuts to WriteWithIndents())
        public override void WriteLine()
        {
            WriteRaw(this.NewLine);
            _currentLine.Clear();
        }
        public override void Write(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = AdjustMultilineString(value);
                WriteWithIndents(value);
            }
        }
        public void Write(Func<string> fnString)
        {
            string value = fnString();
            Write(value);
        }
        public void WriteLine(Func<string> fnString)
        {
            Write(fnString());
            WriteLine();
        }

        public override void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }
        #endregion

        #region I/O
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
            System.IO.File.WriteAllText(fi.FullName, GetContents(), Encoding);
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

        #region Helpers for convenient string usage: TrimLeftPadding(string) and TrimFirstEmptyLine(string)
        /// <summary>
        /// Given a text block (multiple lines), this removes the left padding of the block, by calculating the minimum number of spaces which happens in EVERY line.
        /// Then, this method writes one by one each line, which in case will respect the current indent.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        protected string AdjustMultilineString(string block)
        {
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
