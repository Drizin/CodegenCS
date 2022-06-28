using CodegenCS.___InternalInterfaces___;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    /// <summary>
    /// Extensions to IContextedTemplateWrapper that will provide a Render(), Render{TModel}(model) or Render{TModel1, TModel2}(model2, model2) - according to the type of TTemplate
    /// If the IContextedTemplateWrapper was loaded by <see cref="ICodegenContext"/> or <see cref="ICodegenTextWriter"/> then the Render() will render immediately.
    /// If the IContextedTemplateWrapper was loaded by <see cref="InlineTemplateLoader"/> (<see cref="Template.Load{TTemplate}(object[])"/>) then the Render() will 
    /// just return another wrapper (IContextedTemplateWithModelWrapper) which during the real template rendering will do the real rendering
    /// </summary>
    public static class IContextedTemplateWrapperExtensions
    {
        #region Render() for templates that are defined using string interpolation (InlineTemplateLoader - static factory Template.Load<T>)
        /// <summary>
        /// Used to render a template that was loaded from inside another template using using string-interpolation and <see cref="Template.Load{TTemplate}(object[])"/> and that does not require any model.
        /// Despite the name Render() this actually builds a Lazy-Renderer-Wrapper (an object that will be rendered when <see cref="ICodegenTextWriter"/> is rendering some parent template and finds this embedded).
        /// </summary>
        public static IContextedTemplateWith0ModelWrapper<IBase0ModelTemplate, InlineTemplateLoader> Render
            (this IContextedTemplateWrapper<IBase0ModelTemplate, InlineTemplateLoader> self)
        {
            return new ContextedTemplateWith0ModelWrapper<IBase0ModelTemplate, InlineTemplateLoader>(self) { };
        }

        /// <summary>
        /// Used to render a template that was loaded from inside another template using using string-interpolation and <see cref="Template.Load{TTemplate}(object[])"/> and that does require one model of type <typeparamref name="TModel"/>
        /// Despite the name Render() this actually builds a Lazy-Renderer-Wrapper (an object that will be rendered when <see cref="ICodegenTextWriter"/> is rendering some parent template and finds this embedded).
        /// </summary>
        public static IContextedTemplateWith1ModelWrapper<IBase1ModelTemplate<TModel>, InlineTemplateLoader, TModel> Render<TModel>
            (this IContextedTemplateWrapper<IBase1ModelTemplate<TModel>, InlineTemplateLoader> self, TModel model)
        {
            return new ContextedTemplateWith1ModelWrapper<IBase1ModelTemplate<TModel>, InlineTemplateLoader, TModel>(self)
            {
                _getModel = () => model
            };
        }

        /// <summary>
        /// Used to render a template that was loaded from inside another template using using string-interpolation and <see cref="Template.Load{TTemplate}(object[])"/> and that does require two model (type <typeparamref name="TModel1"/> and <typeparamref name="TModel2"/>)
        /// Despite the name Render() this actually builds a Lazy-Renderer-Wrapper (an object that will be rendered when <see cref="ICodegenTextWriter"/> is rendering some parent template and finds this embedded).
        /// </summary>
        public static IContextedTemplateWith2ModelWrapper<IBase2ModelTemplate<TModel1, TModel2>, InlineTemplateLoader, TModel1, TModel2> Render<TModel1, TModel2>
            (this IContextedTemplateWrapper<IBase2ModelTemplate<TModel1, TModel2>, InlineTemplateLoader> self, TModel1 model1, TModel2 model2)
        {
            return new ContextedTemplateWith2ModelWrapper<IBase2ModelTemplate<TModel1, TModel2>, InlineTemplateLoader, TModel1, TModel2>(self)
            {
                _getModel1 = () => model1,
                _getModel2 = () => model2
            };
        }
        #endregion

        #region Render() for templates that were loaded using ICodegenTextWriter FluentAPI - after .LoadTemplate<TTemplate>.Render() it should get back the originalICodegenTextWriter
        /// <summary>
        /// Renders the template
        /// </summary>
        public static ICodegenTextWriter Render(this IContextedTemplateWrapper<IBase0ModelTemplate, ICodegenTextWriter> self)
        {
            var templateWrapper = (__Hidden_IContextedTemplateWrapper)self;
            var writer = templateWrapper.CodegenTextWriter;
            var template = (IBase0ModelTemplate)templateWrapper.CreateTemplateInstance(writer.DependencyContainer);
            TemplateRenderer.Render(template, writer, writer.DependencyContainer);
            return writer;
        }

        /// <summary>
        /// Renders the template
        /// </summary>
        public static ICodegenTextWriter Render<TModel>(this IContextedTemplateWrapper<IBase1ModelTemplate<TModel>, ICodegenTextWriter> self, TModel model)
        {
            var templateWrapper = (__Hidden_IContextedTemplateWrapper)self;
            var writer = templateWrapper.CodegenTextWriter;
            var template = (IBase1ModelTemplate<TModel>)templateWrapper.CreateTemplateInstance(writer.DependencyContainer);
            TemplateRenderer.Render(template, writer, writer.DependencyContainer, model);
            return writer;
        }

        /// <summary>
        /// Renders the template
        /// </summary>
        public static ICodegenTextWriter Render<TModel1, TModel2>(this IContextedTemplateWrapper<IBase2ModelTemplate<TModel1, TModel2>, ICodegenTextWriter> self, TModel1 model1, TModel2 model2)
        {
            var templateWrapper = (__Hidden_IContextedTemplateWrapper)self;
            var writer = templateWrapper.CodegenTextWriter;
            var template = (IBase2ModelTemplate<TModel1, TModel2>)templateWrapper.CreateTemplateInstance(writer.DependencyContainer);
            TemplateRenderer.Render(template, writer, writer.DependencyContainer, model1, model2);
            return writer;
        }
        #endregion

        #region Render() for templates that were loaded using ICodegenContext FluentAPI - after .LoadTemplate<TTemplate>.Render() it should get back the originalICodegenTextWriter
        /// <summary>
        /// Renders the template
        /// </summary>
        public static ICodegenContext Render(this IContextedTemplateWrapper<IBase0ModelTemplate, ICodegenContext> self)
        {
            var templateWrapper = (__Hidden_IContextedTemplateWrapper)self;
            var writer = templateWrapper.CodegenTextWriter;
            var template = (IBase0ModelTemplate)templateWrapper.CreateTemplateInstance(writer.DependencyContainer);
            TemplateRenderer.Render(template, writer, writer.DependencyContainer);
            return templateWrapper.CodegenContext;
        }

        /// <summary>
        /// Renders the template
        /// </summary>
        public static ICodegenContext Render<TModel>(this IContextedTemplateWrapper<IBase1ModelTemplate<TModel>, ICodegenContext> self, TModel model)
        {
            var templateWrapper = (__Hidden_IContextedTemplateWrapper)self;
            var writer = templateWrapper.CodegenTextWriter;
            var template = (IBase1ModelTemplate<TModel>)templateWrapper.CreateTemplateInstance(writer.DependencyContainer);
            TemplateRenderer.Render(template, writer, writer.DependencyContainer, model);
            return templateWrapper.CodegenContext;
        }

        /// <summary>
        /// Renders the template
        /// </summary>
        public static ICodegenContext Render<TModel1, TModel2>(this IContextedTemplateWrapper<IBase2ModelTemplate<TModel1, TModel2>, ICodegenContext> self, TModel1 model1, TModel2 model2)
        {
            var templateWrapper = (__Hidden_IContextedTemplateWrapper)self;
            var writer = templateWrapper.CodegenTextWriter;
            var template = (IBase2ModelTemplate<TModel1, TModel2>)templateWrapper.CreateTemplateInstance(writer.DependencyContainer);
            TemplateRenderer.Render(template, writer, writer.DependencyContainer, model1, model2);
            return templateWrapper.CodegenContext;
        }
        #endregion

    }
}
