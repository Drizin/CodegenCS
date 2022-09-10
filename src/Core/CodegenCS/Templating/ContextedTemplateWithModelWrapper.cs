using CodegenCS.___InternalInterfaces___;
using System;

namespace CodegenCS
{
    /// <summary>
    /// This is a wrapper around the Template (wraps the Template Type).
    /// This wrapper is used to embed references to templates right from interpolated strings (or from other contexts) 
    /// and at the right time (during rendering) this wrapper will resolve the template (build and inject the required dependencies) and render it.
    /// When <see cref="ICodegenTextWriter"/> "reads" this object as an interpolated string it will resolve and render the template.
    /// </summary>
    /// <typeparam name="TTemplate">Type of the Template</typeparam>
    /// <typeparam name="TContext">Track if the template was invoked from a ICodegenTextWriter or from a ICodegenContext, allowing different extensions for each</typeparam>
    internal class ContextedTemplateWith0ModelWrapper<TTemplate, TContext> : IContextedTemplateWith0ModelWrapper<TTemplate, TContext>, __Hidden_IContextedTemplateWithModelWrapper
    where TTemplate : IBase0ModelTemplate
    {
        protected internal IContextedTemplateWrapper<TTemplate, TContext> _contextedTemplateWrapper;
        internal ContextedTemplateWith0ModelWrapper(IContextedTemplateWrapper<TTemplate, TContext> contextedTemplateWrapper)
        {
            _contextedTemplateWrapper = contextedTemplateWrapper;
        }

        void __Hidden_IContextedTemplateWithModelWrapper.Render(ICodegenTextWriter writer, DependencyContainer dependencyContainer)
        {
            var templateWrapper = (__Hidden_IContextedTemplateWrapper)_contextedTemplateWrapper;
            TTemplate template = (TTemplate)templateWrapper.CreateTemplateInstance(dependencyContainer);
            TemplateRenderer.Render((IBase0ModelTemplate)template, writer, dependencyContainer);
        }
    }

    /// <summary>
    /// This is a wrapper around both the Template (wraps the Template Type) and the model <typeparamref name="TModel"/> required by the template.
    /// This wrapper is used to embed references to templates (and it's models) right from interpolated strings (or from other contexts) 
    /// and at the right time (during rendering) this wrapper will resolve the template (build and inject the required dependencies) and render it.
    /// When <see cref="ICodegenTextWriter"/> "reads" this object as an interpolated string it will resolve and render the template.
    /// </summary>
    /// <typeparam name="TTemplate">Type of the Template</typeparam>
    /// <typeparam name="TContext">Track if the template was invoked from a ICodegenTextWriter or from a ICodegenContext, allowing different extensions for each</typeparam>
    /// <typeparam name="TModel"></typeparam>
    internal class ContextedTemplateWith1ModelWrapper<TTemplate, TContext, TModel> : IContextedTemplateWith1ModelWrapper<TTemplate, TContext, TModel>, __Hidden_IContextedTemplateWithModelWrapper
        where TTemplate : IBase1ModelTemplate<TModel>
    {
        protected internal IContextedTemplateWrapper<TTemplate, TContext> _contextedTemplateWrapper;
        protected internal Func<TModel> _getModel { get; set; }
        internal ContextedTemplateWith1ModelWrapper(IContextedTemplateWrapper<TTemplate, TContext> contextedTemplateWrapper)
        {
            _contextedTemplateWrapper = contextedTemplateWrapper;
        }

        void __Hidden_IContextedTemplateWithModelWrapper.Render(ICodegenTextWriter writer, DependencyContainer dependencyContainer)
        {
            var templateWrapper = (__Hidden_IContextedTemplateWrapper)_contextedTemplateWrapper;
            TTemplate template = (TTemplate)templateWrapper.CreateTemplateInstance(dependencyContainer);
            TemplateRenderer.Render((IBase1ModelTemplate<TModel>)template, writer, dependencyContainer, _getModel());
        }
    }

    /// <summary>
    /// This is a wrapper around both the Template (wraps the Template Type) and the models (<typeparamref name="TModel1"/> and <typeparamref name="TModel2"/>) required by the template.
    /// This wrapper is used to embed references to templates (and it's models) right from interpolated strings (or from other contexts) 
    /// and at the right time (during rendering) this wrapper will resolve the template (build and inject the required dependencies) and render it.
    /// When <see cref="ICodegenTextWriter"/> "reads" this object as an interpolated string it will resolve and render the template.
    /// </summary>
    /// <typeparam name="TTemplate">Type of the Template</typeparam>
    /// <typeparam name="TContext">Track if the template was invoked from a ICodegenTextWriter or from a ICodegenContext, allowing different extensions for each</typeparam>
    /// <typeparam name="TModel1"></typeparam>
    /// <typeparam name="TModel2"></typeparam>
    internal class ContextedTemplateWith2ModelWrapper<TTemplate, TContext, TModel1, TModel2> : IContextedTemplateWith2ModelWrapper<TTemplate, TContext, TModel1, TModel2>, __Hidden_IContextedTemplateWithModelWrapper
        where TTemplate : IBase2ModelTemplate<TModel1, TModel2>
    {
        protected internal IContextedTemplateWrapper<TTemplate, TContext> _contextedTemplateWrapper;
        protected internal Func<TModel1> _getModel1 { get; set; }
        protected internal Func<TModel2> _getModel2 { get; set; }
        internal ContextedTemplateWith2ModelWrapper(IContextedTemplateWrapper<TTemplate, TContext> contextedTemplateWrapper)
        {
            _contextedTemplateWrapper = contextedTemplateWrapper;
        }

        void __Hidden_IContextedTemplateWithModelWrapper.Render(ICodegenTextWriter writer, DependencyContainer dependencyContainer)
        {
            var templateWrapper = (__Hidden_IContextedTemplateWrapper)_contextedTemplateWrapper;
            TTemplate template = (TTemplate)templateWrapper.CreateTemplateInstance(dependencyContainer);
            TemplateRenderer.Render((IBase2ModelTemplate<TModel1, TModel2>)template, writer, dependencyContainer, _getModel1(), _getModel2());
        }
    }

}
#region "Internal/Hidden" Interfaces (those that should NOT be implemented by user code and shouldn't even be noticed) - most are just used for generics/extensions magic or for avoiding reflection
namespace CodegenCS.___InternalInterfaces___
{

    /// <inheritdoc cref="ContextedTemplateWith0ModelWrapper{TTemplate, TContext}" />
    public interface IContextedTemplateWith0ModelWrapper<out TTemplate, TContext>  { }

    /// <inheritdoc cref="ContextedTemplateWith1ModelWrapper{TTemplate, TContext, TModel}" />
    public interface IContextedTemplateWith1ModelWrapper<out TTemplate, TContext, TModel>  { }

    /// <inheritdoc cref="ContextedTemplateWith2ModelWrapper{TTemplate, TContext, TModel1, TModel2}" />
    public interface IContextedTemplateWith2ModelWrapper<out TTemplate, TContext, TModel1, TModel2>  { }


    /// <summary>
    /// "Hidden" interface just used to make internal calls without using reflection and without having to expose these methods/members in the exposed public interface.
    /// (casting to concrete class is not possible because of covariance, so using an interface - even without generic - is enough)
    /// As a hidden interface it should not exposed to user:
    /// - Should be explicitly-implemented only by the concrete class 
    /// - Should not be inherited by any other interface that is exposed to user
    /// </summary>
    interface __Hidden_IContextedTemplateWithModelWrapper
    {
        void Render(ICodegenTextWriter writer, DependencyContainer dependencyContainer);
    }
}
#endregion