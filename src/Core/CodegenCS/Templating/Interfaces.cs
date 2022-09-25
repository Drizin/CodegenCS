using System;
using CodegenCS.___InternalInterfaces___;

namespace CodegenCS
{

    /// <summary>
    /// Templates that output into a single file and do not require any model
    /// </summary>
    public interface ICodegenTemplate : IBaseSinglefileTemplate, IBase0ModelTemplate
    {
        void Render(ICodegenTextWriter writer);
    }

    /// <summary>
    /// Templates that output into a single file and require a single model
    /// </summary>
    public interface ICodegenTemplate<TModel> : IBaseSinglefileTemplate, IBase1ModelTemplate<TModel>
    {
        void Render(ICodegenTextWriter writer, TModel model);
    }

    /// <summary>
    /// Templates that output into a single file and require two models
    /// </summary>
    public interface ICodegenTemplate<TModel1, TModel2> : IBaseSinglefileTemplate, IBase2ModelTemplate<TModel1, TModel2>
    {
        void Render(ICodegenTextWriter writer, TModel1 model1, TModel2 model2);
    }


    /// <summary>
    /// Templates that output into multiple files and do not require any model
    /// </summary>
    public interface ICodegenMultifileTemplate : IBaseMultifileTemplate, IBase0ModelTemplate
    {
        void Render(ICodegenContext context);
    }
    /// <summary>
    /// Templates that output into multiple files and require a single model
    /// </summary>
    public interface ICodegenMultifileTemplate<TModel> : IBaseMultifileTemplate, IBase1ModelTemplate<TModel>
    {
        void Render(ICodegenContext context, TModel model);
    }
    /// <summary>
    /// Templates that output into multiple files and require two models
    /// </summary>
    public interface ICodegenMultifileTemplate<TModel1, TModel2> : IBaseMultifileTemplate, IBase2ModelTemplate<TModel1, TModel2>
    {
        void Render(ICodegenContext context, TModel1 model1, TModel2 model2);
    }

    /// <summary>
    /// Templates that return a single block of text and do not require any model
    /// </summary>
    public interface ICodegenStringTemplate : IBaseStringTemplate, IBase0ModelTemplate
    {
        FormattableString Render();
    }
    /// <summary>
    /// Templates that return a single block of text and require a single model
    /// </summary>
    public interface ICodegenStringTemplate<TModel> : IBaseStringTemplate, IBase1ModelTemplate<TModel>
    {
        FormattableString Render(TModel model);
    }
    /// <summary>
    /// Templates that return a single block of text and require two models
    /// </summary>
    public interface ICodegenStringTemplate<TModel1, TModel2> : IBaseStringTemplate, IBase2ModelTemplate<TModel1, TModel2>
    {
        FormattableString Render(TModel1 model1, TModel2 model2);
    }
}

#region "Internal/Hidden" Interfaces (those that should NOT be implemented by user code and shouldn't even be noticed) - most are just used for generics/extensions magic or for avoiding reflection
namespace CodegenCS.___InternalInterfaces___
{
    /// <summary>
    /// All Templates implement some interface that inherit from this
    /// </summary>
    public interface IBaseTemplate { }

    /// <summary>
    /// Templates that do not require any model
    /// </summary>
    public interface IBase0ModelTemplate : IBaseTemplate { }

    /// <summary>
    /// Templates that require a single model
    /// </summary>
    public interface IBase1ModelTemplate<TModel> : IBaseTemplate { }


    /// <summary>
    /// Templates that require two models
    /// </summary>
    public interface IBase2ModelTemplate<TModel1, TModel2> : IBaseTemplate { }

    /// <summary>
    /// Templates that output into a single file (either by expecting and writing into a <see cref="ICodegenTextWriter"/> or by returning a string like <see cref="IBaseStringTemplate"/> )
    /// </summary>
    public interface IBaseSinglefileTemplate : IBaseTemplate { }

    /// <summary>
    /// Templates that output into a multiple files (and therefore they take a <see cref="ICodegenContext"/> and will decide how to name each output)
    /// </summary>
    public interface IBaseMultifileTemplate : IBaseTemplate { }

    /// <summary>
    /// Templates that can be as simple as a single block of text, in this case they just return an interpolated string.
    /// </summary>
    public interface IBaseStringTemplate : IBaseTemplate, IBaseSinglefileTemplate { }
}
#endregion
