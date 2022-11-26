using CodegenCS.___InternalInterfaces___;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using System;
using System.Collections.Generic;
using System.Text;
using SaveFilesResult = global::CodegenCS.IO.SaveFilesResult;

namespace CodegenCS
{
    /// <summary>
    /// ICodegenContext keeps track of multiple output files and define how those outputs are saved.
    /// </summary>
    public interface ICodegenContext
    {
        /// <summary>
        /// If your template finds any error you can just append the errors here in this list <br />
        /// </summary>
        List<string> Errors { get; }

        /// <summary>
        /// Saves all files in the outputFolder. <br />
        /// According to the RelativePath of each file they may be saved in different folders
        /// </summary>
        /// <param name="outputFolder">Can be absolute or relative</param>
        [Obsolete("Please use CodegenCS.IO extension SaveToFolder()")]
        SaveFilesResult SaveFiles(string outputFolder);

        /// <summary>
        /// Paths of the Output files (as defined during creation using <see cref="this[string]"/> indexer)
        /// </summary>
        HashSet<string> OutputFilesPaths { get; }

        /// <summary>
        /// Output files (created using <see cref="this[string]"/> indexer)
        /// </summary>
        IReadOnlyList<ICodegenOutputFile> OutputFiles { get; }

        /// <summary>
        /// When a <see cref="ICodegenOutputFile"/> is renamed it should notify the <see cref="ICodegenContext"/> by calling this method.
        /// </summary>
        /// <returns>True if context could successfuly find and rename the file</returns>
        bool OnOutputFileRenamed(string oldRelativePath, string newRelativePath, ICodegenOutputFile outputFile);

        /// <summary>
        /// Output files are indexed by their relative path. <br />
        /// If context doesn't have an OutputFile with this relative path, a new one will automatically be created
        /// </summary>
        ICodegenOutputFile this[string relativePath] { get; }

        /// <summary>
        /// Default Output file if we write a single-file template (<see cref="ICodegenTemplate"/> or <see cref="ICodegenStringTemplate"/>) to a Context (that could potentially manage multiple outputs)
        /// </summary>
        ICodegenOutputFile DefaultOutputFile { get; }

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
    public interface ICodegenContext<FT> : ICodegenContext
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        new ICodegenOutputFile<FT> this[string relativePath] { get; }
        ICodegenOutputFile<FT> this[string relativePath, FT fileType] { get; }
        new IReadOnlyList<ICodegenOutputFile<FT>> OutputFiles { get; }
        new ICodegenOutputFile<FT> DefaultOutputFile { get; }
}

    /// <summary>
    /// ICodegenContext<typeparamref name="O"/> extends <see cref="ICodegenContext"/> by allowing the the use of a custom implementation of OutputFile (custom writer of type <typeparamref name="O"/>)
    /// </summary>
    public interface ICustomWriterCodegenContext<O> : ICodegenContext
        where O : ICodegenOutputFile
    {
        new O this[string relativePath] { get; }
        new IReadOnlyList<O> OutputFiles { get; }
        new O DefaultOutputFile { get; }
    }

    /// <summary>
    /// Most generic interface of ICodegenContext, combining both <see cref="ICustomWriterCodegenContext{O}"/> and <see cref="ICodegenContext{FT}"/>:
    /// - Keeps track of multiple output files and define how those outputs are saved.
    /// - Uses a custom implementation of OutputFile (custom writer of type <typeparamref name="O"/>)
    /// - Generates output files of different file types (<typeparamref name="FT"/> is an enum of the possible types)
    /// - The different types are used for something (e.g. the different types may use different actions for being added to the project file)
    /// </summary>
    public interface ICodegenContext<FT, O> : ICodegenContext<FT>, ICustomWriterCodegenContext<O>
        where O : ICodegenOutputFile<FT>
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        new O this[string relativePath] { get; }
        new O this[string relativePath, FT fileType] { get; }
        new IReadOnlyList<O> OutputFiles { get; }
        new O DefaultOutputFile { get; }

    }
    
}
