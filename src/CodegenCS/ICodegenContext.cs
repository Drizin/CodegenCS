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
        CodegenOutputFile this[string relativePath] { get; }
        List<string> Errors { get; }
        void SaveFiles(string outputFolder);
        CodegenOutputFile DefaultOutputFile { get; }
    }

    /// <summary>
    /// IMultiplefiletypeCodegenContext extends <see cref="ICodegenContext"/> by allowing each outputfile
    /// to be classified with a file type (enum <typeparamref name="FT"/> contains all possible types).
    /// Based on the different file types the context may take different actions for saving each file
    /// </summary>
    public interface IMultipleFiletypeCodegenContext<FT> : ICodegenContext
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        new CodegenOutputFile<FT> this[string relativePath] { get; }
        CodegenOutputFile<FT> this[string relativePath, FT fileType] { get; }
    }

    /// <summary>
    /// ICodegenContext<typeparamref name="O"/> extends <see cref="ICodegenContext"/> by allowing the the use of a custom implementation of OutputFile (custom writer of type <typeparamref name="O"/>)
    /// </summary>
    public interface ICustomWriterCodegenContext<O> : ICodegenContext
        where O : CodegenOutputFile
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
        where O : CodegenOutputFile<FT>
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        new O this[string relativePath] { get; /*set;*/ }
        new O this[string relativePath, FT fileType] { get; }
    }
    
}
