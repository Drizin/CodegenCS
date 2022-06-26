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
        ICodegenContext RenderMultifileTemplate(ICodegenMultifileTemplate template);
        ICodegenContext RenderMultifileTemplate<TModel>(ICodegenMultifileTemplate<TModel> template, TModel model);
        ICodegenContext RenderMultifileTemplate<TModel1, TModel2>(ICodegenMultifileTemplate<TModel1, TModel2> template, TModel1 model1, TModel2 model2);
        ICodegenContext RenderMultifileTemplate<T>(params object[] otherDependencies) where T : class, ICodegenMultifileTemplate;
        ICodegenContext RenderMultifileTemplate<T, TModel>(TModel model, params object[] otherDependencies) where T : class, ICodegenMultifileTemplate<TModel>;
        ICodegenContext RenderMultifileTemplate<T, TModel1, TModel2>(TModel1 model1, TModel2 model2, params object[] otherDependencies) where T : class, ICodegenMultifileTemplate<TModel1, TModel2>;
        ICodegenContext RenderGenericTemplate(ICodegenGenericTemplate template);
        ICodegenContext RenderGenericTemplate<TModel>(ICodegenGenericTemplate<TModel> template, TModel model);
        ICodegenContext RenderGenericTemplate<TModel1, TModel2>(ICodegenGenericTemplate<TModel1, TModel2> template, TModel1 model1, TModel2 model2);
        ICodegenContext RenderGenericTemplate<T>(params object[] otherDependencies) where T : class, ICodegenGenericTemplate;
        ICodegenContext RenderGenericTemplate<T, TModel>(TModel model, params object[] otherDependencies) where T : class, ICodegenGenericTemplate<TModel>;
        ICodegenContext RenderGenericTemplate<T, TModel1, TModel2>(TModel1 model1, TModel2 model2, params object[] otherDependencies) where T : class, ICodegenGenericTemplate<TModel1, TModel2>;
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
