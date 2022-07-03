using CodegenCS.___InternalInterfaces___;

namespace CodegenCS
{
    /// <summary>
    /// This is used as context (TContext) of <see cref="IContextedTemplateWrapper{TTemplate, TContext}"/>, 
    /// allowing <see cref="IContextedTemplateWrapper{TTemplate, TContext}"/> to have extensions according to the context (<see cref="InlineTemplateLoader"/>, <see cref="ICodegenContext"/> or <see cref="ICodegenTextWriter"/>).
    /// </summary>
    public class InlineTemplateLoader
    {
    }

    /// <summary>
    /// Inside the interpolated strings of any template we can just call Template.Load{TTemplate}().Render(model).
    /// Requires "using CodegenCS;"
    /// </summary>
    public partial class Template
    {
        /// <summary>
        /// Loads a template by the Type (class name). Allow to pass dependencies required by the template.
        /// </summary>
        /// <typeparam name="TTemplate">Template type</typeparam>
        /// <param name="dependencies">Optional dependencies that will be injected if the template constructor requires it</param>
        public static IContextedTemplateWrapper<TTemplate, InlineTemplateLoader> Load<TTemplate>(params object[] dependencies) where TTemplate : IBaseTemplate
        {
            return new ContextedTemplateWrapper<TTemplate, InlineTemplateLoader>(typeof(TTemplate), dependencies);
        }
    }
}
