using CodegenCS.___InternalInterfaces___;
using System;

namespace CodegenCS
{
    internal class TemplateRenderer
    {
        internal static void Render(IBase0ModelTemplate templ, ICodegenTextWriter writer, DependencyContainer dependencyContainer)
        {
            if (typeof(ICodegenTemplate).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenTemplate)templ;
                template.Render(writer);
                return;
            }
            if (typeof(ICodegenMultifileTemplate).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenMultifileTemplate)templ;
                ICodegenContext context = (ICodegenContext)dependencyContainer.Resolve(typeof(ICodegenContext));
                template.Render(context);
                return;
            }
            if (typeof(ICodegenStringTemplate).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenStringTemplate)templ;
                FormattableString formattable = template.GetTemplate();
                writer.Write(formattable);
                return;
            }
        }
        internal static void Render<TModel>(IBase1ModelTemplate<TModel> templ, ICodegenTextWriter writer, DependencyContainer dependencyContainer, TModel model)
        {
            if (typeof(ICodegenTemplate<TModel>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenTemplate<TModel>)templ;
                template.Render(writer, model);
                return;
            }
            if (typeof(ICodegenMultifileTemplate<TModel>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenMultifileTemplate<TModel>)templ;
                ICodegenContext context = (ICodegenContext)dependencyContainer.Resolve(typeof(ICodegenContext));
                template.Render(context, model);
                return;
            }
            if (typeof(ICodegenStringTemplate<TModel>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenStringTemplate<TModel>)templ;
                FormattableString formattable = template.GetTemplate(model);
                writer.Write(formattable);
                return;
            }
        }

        internal static void Render<TModel1, TModel2>(IBase2ModelTemplate<TModel1, TModel2> templ, ICodegenTextWriter writer, DependencyContainer dependencyContainer, TModel1 model1, TModel2 model2)
        {
            if (typeof(ICodegenTemplate<TModel1, TModel2>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenTemplate<TModel1, TModel2>)templ;
                template.Render(writer, model1, model2);
                return;
            }
            if (typeof(ICodegenMultifileTemplate<TModel1, TModel2>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenMultifileTemplate<TModel1, TModel2>)templ;
                ICodegenContext context = (ICodegenContext)dependencyContainer.Resolve(typeof(ICodegenContext));
                template.Render(context, model1, model2);
                return;
            }
            if (typeof(ICodegenStringTemplate<TModel1, TModel2>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenStringTemplate<TModel1, TModel2>)templ;
                FormattableString formattable = template.GetTemplate(model1, model2);
                writer.Write(formattable);
                return;
            }
        }
    }
}
