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
        public static EmbeddedTemplate Template<T>(params object[] args) where T : class, ICodegenTemplate
        {
            return new EmbeddedTemplate()
            {
                TemplateType = typeof(T),
                Arguments = args
            };
        }
        public class EmbeddedTemplate
        {
            public Type TemplateType { get; set; }
            public object[] Arguments;
        }

    }
}
