using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    /// <summary>
    /// All Templates should "inherit" from this interface.
    /// </summary>
    public interface ICodegenTemplate { }

    /// <summary>
    /// Any Template that does need some model(s) for the Rendering (and yet they can get other parameters through dependency-injection)
    /// </summary>
    public interface ICodegenTemplate<TModel> { }

    /// <summary>
    /// Any Template that does need some model(s) for the Rendering (and yet they can get other parameters through dependency-injection)
    /// </summary>
    public interface ICodegenTemplate<TModel1, TModel2> { }

    /// <summary>
    /// Templates that output into a single file just get a <see cref="ICodegenTextWriter"/> and will write directly into that writer
    /// </summary>
    public interface ICodegenSinglefileTemplate : ICodegenTemplate
    {
        void Render(ICodegenTextWriter writer);
    }
    public interface ICodegenSinglefileTemplate<TModel> : ICodegenTemplate<TModel>
    {
        void Render(ICodegenTextWriter writer, TModel model);
    }
    public interface ICodegenSinglefileTemplate<TModel1, TModel2> : ICodegenTemplate<TModel1, TModel2>
    {
        void Render(ICodegenTextWriter writer, TModel1 model1, TModel2 model2);
    }


    /// <summary>
    /// Templates that output into multiple files usually get a <see cref="ICodegenContext"/> which can manage multiple files
    /// </summary>
    public interface ICodegenMultifileTemplate : ICodegenTemplate
    {
        void Render(ICodegenContext context);
    }
    public interface ICodegenMultifileTemplate<TModel> : ICodegenTemplate<TModel>
    {
        void Render(ICodegenContext context, TModel model);
    }
    public interface ICodegenMultifileTemplate<TModel1, TModel2> : ICodegenTemplate<TModel1, TModel2>
    {
        void Render(ICodegenContext context, TModel1 model1, TModel2 model2);
    }

    /// <summary>
    /// Sometimes our templates can be as simple as a single block of text, in this case you can just return an interpolated string.
    /// Text templates can be outputting to the default <see cref="ICodegenOutputFile"/>, or they could be invoked explicitly as subtemplates of another template.
    /// Note that by default the <see cref="CodegenTextWriter"/> will apply some cool magic to the strings, like trimming left-paddings, and removing some empty lines.
    /// </summary>
    public interface ICodegenTextTemplate : ICodegenTemplate
    {
        FormattableString GetTemplate();
    }
    public interface ICodegenTextTemplate<TModel> : ICodegenTemplate<TModel>
    {
        FormattableString GetTemplate(TModel model);
    }
    public interface ICodegenTextTemplate<TModel1, TModel2> : ICodegenTemplate<TModel1, TModel2>
    {
        FormattableString GetTemplate(TModel1 model1, TModel2 model2);
    }

    /// <summary>
    /// If the template does not output anything to a <see cref="ICodegenTextWriter"/> or to a <see cref="ICodegenContext"/> you can just have a parameterless Render() method.
    /// Templates are always instantiated using <see cref="DependencyContainer"/> which can resolve dependencies in the constructor, so by using a parameterless Render() method
    /// you can still get any dependencies (ICodegenContext, ICodegenTextWriter, InputModels like database schemas, etc) and manage them on your own.
    /// </summary>
    public interface ICodegenGenericTemplate : ICodegenTemplate
    {
        void Render();
    }
    public interface ICodegenGenericTemplate<TModel> : ICodegenTemplate<TModel>
    {
        void Render(TModel model);
    }
    public interface ICodegenGenericTemplate<TModel1, TModel2> : ICodegenTemplate<TModel1, TModel2>
    {
        void Render(TModel1 model1, TModel2 model2);
    }

}
