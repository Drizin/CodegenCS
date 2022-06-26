using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    public class Include
    {
        /// <summary>
        /// Include a Template inside interpolated strings.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static EmbeddedTemplate Template<T>(params object[] otherDependencies) where T : class, ICodegenTemplate
        {
            return new EmbeddedTemplate()
            {
                TemplateType = typeof(T),
                OtherDependencies = otherDependencies
            };
        }

        //TODO: All render calls of templates with models should be probably be broken into 2 chained calls - one for resolving dependency, and another for the invocation.
        // e.g. instead of Include.Template<TemplateType, TModel>(TModel model, params otherDependencies) we would use Include.Template<TemplateType>(anyDeps).Render(model) - so TModel can be infered by the template
        // and in CodegenTextWriter we can throw if the interpolated object is a resolved-but-not-invoked template.
        public static EmbeddedTemplate<TModel> Template<T, TModel>(TModel model, params object[] otherDependencies) where T : class, ICodegenTemplate<TModel>
        {
            return new EmbeddedTemplate<TModel>()
            {
                TemplateType = typeof(T),
                OtherDependencies = otherDependencies,
                Model = () => model
            };
        }

        public static EmbeddedTemplate<TModel> Template<T, TModel>(Func<TModel> model, params object[] otherDependencies) where T : class, ICodegenTemplate<TModel>
        {
            return new EmbeddedTemplate<TModel>()
            {
                TemplateType = typeof(T),
                OtherDependencies = otherDependencies,
                Model = model
            };
        }

        public static EmbeddedTemplate<TModel1, TModel2> Template<T, TModel1, TModel2>(TModel1 model1, TModel2 model2, params object[] otherDependencies) where T : class, ICodegenTemplate<TModel1, TModel2>
        {
            return new EmbeddedTemplate<TModel1, TModel2>()
            {
                TemplateType = typeof(T),
                OtherDependencies = otherDependencies,
                Model1 = () => model1,
                Model2 = () => model2
            };
        }

        public static EmbeddedTemplate<TModel1, TModel2> Template<T, TModel1, TModel2>(Func<TModel1> model1, Func<TModel2> model2, params object[] otherDependencies) where T : class, ICodegenTemplate<TModel1, TModel2>
        {
            return new EmbeddedTemplate<TModel1, TModel2>()
            {
                TemplateType = typeof(T),
                OtherDependencies = otherDependencies,
                Model1 = model1,
                Model2 = model2
            };
        }

        public class EmbeddedTemplate
        {
            public Type TemplateType { get; set; }
            public object[] OtherDependencies { get; set; }

            public virtual void Render(ICodegenTextWriter writer, DependencyContainer dependencyContainer)
            {
                var templateInstance = (ICodegenTemplate)dependencyContainer.Resolve(this.TemplateType, this.OtherDependencies);
                TemplateRenderer.Render(templateInstance, writer, dependencyContainer);
            }
        }
        public class EmbeddedTemplate<TModel> : EmbeddedTemplate
        {
            public Func<TModel> Model { get; set; }
            public override void Render(ICodegenTextWriter writer, DependencyContainer dependencyContainer)
            {
                var templateInstance = (ICodegenTemplate<TModel>)dependencyContainer.Resolve(this.TemplateType, this.OtherDependencies);
                TemplateRenderer.Render<TModel>(templateInstance, writer, dependencyContainer, this.Model());
            }
        }
        public class EmbeddedTemplate<TModel1, TModel2> : EmbeddedTemplate
        {
            public Func<TModel1> Model1 { get; set; }
            public Func<TModel2> Model2 { get; set; }
            public override void Render(ICodegenTextWriter writer, DependencyContainer dependencyContainer)
            {
                var templateInstance = (ICodegenTemplate<TModel1, TModel2>)dependencyContainer.Resolve(this.TemplateType, this.OtherDependencies);
                TemplateRenderer.Render<TModel1, TModel2>(templateInstance, writer, dependencyContainer, this.Model1(), this.Model2());
            }
        }


    }
}
