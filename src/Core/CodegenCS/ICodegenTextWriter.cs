using CodegenCS.___InternalInterfaces___;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using System;
using System.Text;

namespace CodegenCS
{
    public interface ICodegenTextWriter : IDisposable
    {
        CodegenTextWriter.MultilineBehaviorType MultilineBehavior { get; set; }
        CodegenTextWriter.CurlyBracesStyleType CurlyBracesStyle { get; set; }
        RenderEnumerableOptions DefaultIEnumerableRenderOptions { get; set; }
        string NewLine { get; set; }
        Encoding Encoding { get; }
        void Close();
        void Flush();
        string ToString();
        int IndentLevel { get; }
        string IndentString { get; set; }
        ICodegenTextWriter IncreaseIndent();
        ICodegenTextWriter DecreaseIndent();
        ICodegenTextWriter EnsureEmptyLine();
        ICodegenTextWriter EnsureLineBreakBeforeNextWrite();
        ICodegenTextWriter WithIndent(string beforeBlock, string afterBlock, Action innerBlockAction);
        ICodegenTextWriter WithIndent(string beforeBlock, string afterBlock, Action<ICodegenTextWriter> innerBlockAction);
        ICodegenTextWriter WithCurlyBraces(string beforeBlock, Action innerBlockAction);
        ICodegenTextWriter WithCurlyBraces(string beforeBlock, Action<ICodegenTextWriter> innerBlockAction);
        ICodegenTextWriter WithPythonBlock(string beforeBlock, Action innerBlockAction);
        ICodegenTextWriter WithPythonBlock(string beforeBlock, Action<ICodegenTextWriter> innerBlockAction);
        ICodegenTextWriter WithCBlock(string beforeBlock, Action innerBlockAction);
        ICodegenTextWriter WithCBlock(string beforeBlock, Action<ICodegenTextWriter> innerBlockAction);
        ICodegenTextWriter WithJavaBlock(string beforeBlock, Action innerBlockAction);
        ICodegenTextWriter WithJavaBlock(string beforeBlock, Action<ICodegenTextWriter> innerBlockAction);
        ICodegenTextWriter Write(FormattableString formattable);
        ICodegenTextWriter WriteLine(FormattableString formattable);
        ICodegenTextWriter Write(RawString format, params object[] arguments);
        ICodegenTextWriter WriteLine(RawString format, params object[] arguments);
        ICodegenTextWriter Write(object value);
        ICodegenTextWriter WriteLine(object value);
        ICodegenTextWriter WriteLine();
        ICodegenTextWriter Write(RawString value);
        ICodegenTextWriter WriteLine(RawString value);
        ICodegenTextWriter Write(Func<RawString> fnString);
        ICodegenTextWriter WriteLine(Func<RawString> fnString);
        ICodegenTextWriter Write(Func<FormattableString> fnFormattableString);
        ICodegenTextWriter WriteLine(Func<FormattableString> fnFormattableString);
        ICodegenTextWriter Write(char[] buffer);
        ICodegenTextWriter Write(char[] buffer, int index, int count);
        ICodegenTextWriter WriteLine(char[] buffer);
        ICodegenTextWriter WriteLine(char[] buffer, int index, int count);
        string GetContents();
        DependencyContainer DependencyContainer { get; }
        IDisposable WithCBlock(string beforeBlock = null); // obsolete
        IDisposable WithIndent(string beforeBlock = null, string afterBlock = null);

        [Obsolete("Please use CodegenCS.IO extensions: ICodegenTextWriter.SaveToFile() or ICodegenOutputFile.SaveToFolder()")]
        void SaveToFile(string path, bool createFolder = true, Encoding encoding = null);

        /// <summary>
        /// Loads any template by the Type.
        /// After loading don't forget to call Render() extensions (<see cref="IContextedTemplateWrapperExtensions.Render(IContextedTemplateWrapper{IBase0ModelTemplate, ICodegenContext})"/>)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencies">Optional dependencies can be used and will be automatically injected if template constructor requires it</param>
        /// <returns></returns>
        IContextedTemplateWrapper<T, ICodegenTextWriter> LoadTemplate<T>(params object[] dependencies) where T : IBaseSinglefileTemplate;

        /// <summary>
        /// Renders a <see cref="ICodegenTemplate"/> template that do not need any model.
        /// If you need a template that takes a model please use <see cref="LoadTemplate{T}(object[])"/> method.
        /// </summary>
        ICodegenTextWriter RenderTemplate(ICodegenTemplate template);

        /// <summary>
        /// Renders a <see cref="ICodegenStringTemplate"/> template that do not need any model.
        /// If you need a template that takes a model please use <see cref="LoadTemplate{T}(object[])"/> method.
        /// </summary>
        ICodegenTextWriter RenderTemplate(ICodegenStringTemplate template);

        event EventHandler<WrittenEventArgs> Written;
    }
    public class WrittenEventArgs : EventArgs
    {
        public string WrittenValue { get; protected set; }
        public WrittenEventArgs(string writtenValue) : base()
        {
            WrittenValue = writtenValue;
        }
    }

}
