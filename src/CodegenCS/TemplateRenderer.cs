using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    public class TemplateRenderer
    {
        public static void Render(ICodegenTemplate templ, ICodegenTextWriter writer, DependencyContainer dependencyContainer)
        {
            if (typeof(ICodegenSinglefileTemplate).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenSinglefileTemplate)templ;
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
            if (typeof(ICodegenTextTemplate).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenTextTemplate)templ;
                FormattableString formattable = template.GetTemplate();
                writer.Write(formattable);
                return;
            }
            if (typeof(ICodegenGenericTemplate).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenGenericTemplate)templ;
                template.Render();
                return;
            }
        }
        public static void Render<TModel>(ICodegenTemplate<TModel> templ, ICodegenTextWriter writer, DependencyContainer dependencyContainer, TModel model)
        {
            if (typeof(ICodegenSinglefileTemplate<TModel>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenSinglefileTemplate<TModel>)templ;
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
            if (typeof(ICodegenTextTemplate<TModel>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenTextTemplate<TModel>)templ;
                FormattableString formattable = template.GetTemplate(model);
                writer.Write(formattable);
                return;
            }
            if (typeof(ICodegenGenericTemplate<TModel>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenGenericTemplate<TModel>)templ;
                template.Render(model);
                return;
            }
        }

        public static void Render<TModel1, TModel2>(ICodegenTemplate<TModel1, TModel2> templ, ICodegenTextWriter writer, DependencyContainer dependencyContainer, TModel1 model1, TModel2 model2)
        {
            if (typeof(ICodegenSinglefileTemplate<TModel1, TModel2>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenSinglefileTemplate<TModel1, TModel2>)templ;
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
            if (typeof(ICodegenTextTemplate<TModel1, TModel2>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenTextTemplate<TModel1, TModel2>)templ;
                FormattableString formattable = template.GetTemplate(model1, model2);
                writer.Write(formattable);
                return;
            }
            if (typeof(ICodegenGenericTemplate<TModel1, TModel2>).IsAssignableFrom(templ.GetType()))
            {
                var template = (ICodegenGenericTemplate<TModel1, TModel2>)templ;
                template.Render(model1, model2);
                return;
            }
        }
    }
}
