using CodegenCS.___InternalInterfaces___;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    /// <summary>
    /// ICodegenContext keeps track of multiple output files and define how those outputs are saved.
    /// </summary>
    public interface ICodegenContext
    {
        ICodegenOutputFile this[string relativePath] { get; }
        List<string> Errors { get; }
        void SaveFiles(string outputFolder);
        ICodegenOutputFile DefaultOutputFile { get; }
        HashSet<string> OutputFilesPaths { get; }

        /// <summary>
        /// Loads any template by the Type.
        /// After loading don't forget to call Render() extensions (<see cref="IContextedTemplateWrapperExtensions.Render(IContextedTemplateWrapper{IBase0ModelTemplate, ICodegenContext})"/>)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencies">Optional dependencies can be used and will be automatically injected if template constructor requires it</param>
        /// <returns></returns>
        IContextedTemplateWrapper<T, ICodegenContext> LoadTemplate<T>(params object[] dependencies) where T : IBaseTemplate;

        /// <summary>
        /// Renders to the <see cref="DefaultOutputFile"/> a <see cref="ICodegenTemplate"/> template that do not need any model.
        /// If you need a template that takes a model please use <see cref="LoadTemplate{T}(object[])"/> method.
        /// </summary>
        ICodegenContext RenderTemplate(ICodegenTemplate template);

        /// <summary>
        /// Renders to the <see cref="DefaultOutputFile"/> a <see cref="ICodegenStringTemplate"/> template that do not need any model.
        /// If you need a template that takes a model please use <see cref="LoadTemplate{T}(object[])"/> method.
        /// </summary>
        ICodegenContext RenderTemplate(ICodegenStringTemplate template);

        /// <summary>
        /// Renders a <see cref="ICodegenMultifileTemplate"/> template that do not need any model. 
        /// Template can render to multiple output files.
        /// If you need a template that takes a model please use <see cref="LoadTemplate{T}(object[])"/> method.
        /// </summary>
        ICodegenContext RenderTemplate(ICodegenMultifileTemplate template);

        DependencyContainer DependencyContainer { get; }
    }

    /// <summary>
    /// IMultiplefiletypeCodegenContext extends <see cref="ICodegenContext"/> by allowing each outputfile
    /// to be classified with a file type (enum <typeparamref name="FT"/> contains all possible types).
    /// Based on the different file types the context may take different actions for saving each file
    /// </summary>
    public interface IMultipleFiletypeCodegenContext<FT> : ICodegenContext
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        new ICodegenOutputFile<FT> this[string relativePath] { get; }
        ICodegenOutputFile<FT> this[string relativePath, FT fileType] { get; }
    }

    /// <summary>
    /// ICodegenContext<typeparamref name="O"/> extends <see cref="ICodegenContext"/> by allowing the the use of a custom implementation of OutputFile (custom writer of type <typeparamref name="O"/>)
    /// </summary>
    public interface ICustomWriterCodegenContext<O> : ICodegenContext
        where O : ICodegenOutputFile
    {
        new O this[string relativePath] { get; }
        new O DefaultOutputFile { get; }
    }
    
    /// <summary>
    /// Most generic interface of ICodegenContext, combining both <see cref="ICustomWriterCodegenContext{O}"/> and <see cref="IMultipleFiletypeCodegenContext{FT}"/>:
    /// - Keeps track of multiple output files and define how those outputs are saved.
    /// - Uses a custom implementation of OutputFile (custom writer of type <typeparamref name="O"/>)
    /// - Generates output files of different file types (<typeparamref name="FT"/> is an enum of the possible types)
    /// - The different types are used for something (e.g. the different types may use different actions for being added to the project file)
    /// </summary>
    public interface ICodegenContext<O, FT> : ICodegenContext, ICustomWriterCodegenContext<O>, IMultipleFiletypeCodegenContext<FT>
        where O : ICodegenOutputFile<FT>
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        new O this[string relativePath] { get; /*set;*/ }
        new O this[string relativePath, FT fileType] { get; }
    }
    
}
