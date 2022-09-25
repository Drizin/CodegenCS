using System;
using CodegenCS.___InternalInterfaces___;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;

namespace CodegenCS
{
    /// <summary>
    /// This is a wrapper around Templates (wraps the Template Type). 
    /// This wrapper is used to embed references to templates right from interpolated strings (or from other contexts) 
    /// and at the right time (during rendering) this wrapper will resolve the template (build and inject the required dependencies) and can be invoked to render it.
    /// You should use .Render() extensions (<see cref="IContextedTemplateWrapperExtensions.Render(IContextedTemplateWrapper{IBase0ModelTemplate, ICodegenContext})" />) /> and (if template requires) provide the model(s)
    /// </summary>
    /// <typeparam name="TTemplate">Type of the Template</typeparam>
    /// <typeparam name="TContext">Track if the template was invoked from a ICodegenTextWriter or from a ICodegenContext, allowing different extensions for each</typeparam>
    internal class ContextedTemplateWrapper<TTemplate, TContext> : IContextedTemplateWrapper<TTemplate, TContext>, __Hidden_IContextedTemplateWrapper
    {
        protected internal Type _templateType { get; set; }
        protected internal object[] _dependencies { get; set; }
        protected internal ICodegenTextWriter CodegenTextWriter { get; internal set; }
        protected internal ICodegenContext CodegenContext { get; internal set; }

        internal ContextedTemplateWrapper(Type templateType, params object[] dependencies)
        {
            _templateType = templateType;
            _dependencies = dependencies;
        }
        internal TTemplate CreateTemplateInstance(DependencyContainer dependencyContainer)
        {
            return (TTemplate) dependencyContainer.Resolve(_templateType, _dependencies);
        }

        object __Hidden_IContextedTemplateWrapper.CreateTemplateInstance(DependencyContainer dependencyContainer)
        {
            return CreateTemplateInstance(dependencyContainer);
        }
        Type __Hidden_IContextedTemplateWrapper.TemplateType => _templateType;

        ICodegenTextWriter __Hidden_IContextedTemplateWrapper.CodegenTextWriter => CodegenTextWriter;

        ICodegenContext __Hidden_IContextedTemplateWrapper.CodegenContext => CodegenContext;
    }

}

#region "Internal/Hidden" Interfaces (those that should NOT be implemented by user code and shouldn't even be noticed) - most are just used for generics/extensions magic or for avoiding reflection
namespace CodegenCS.___InternalInterfaces___
{
    /// <inheritdoc cref="ContextedTemplateWrapper{TTemplate, TContext}" />
    public interface IContextedTemplateWrapper<out TTemplate, TContext> { }


    /// <summary>
    /// "Hidden" interface just used to make internal calls without using reflection and without having to expose these methods/members in the exposed public interface.
    /// (casting to concrete class is not possible because of covariance, so using an interface - even without generic - is enough)
    /// As a hidden interface it should not exposed to user:
    /// - Should be explicitly-implemented only by the concrete class 
    /// - Should not be inherited by any other interface that is exposed to user
    /// </summary>
    interface __Hidden_IContextedTemplateWrapper
    {
        object CreateTemplateInstance(DependencyContainer dependencyContainer);
        Type TemplateType { get; }
        ICodegenTextWriter CodegenTextWriter { get; }
        ICodegenContext CodegenContext { get; }
    }
}
#endregion
