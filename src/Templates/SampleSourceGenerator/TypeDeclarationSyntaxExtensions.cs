using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleSourceGenerator
{
    public static class TypeDeclarationSyntaxExtensions
    {
        const char NESTED_CLASS_DELIMITER = '+';
        const char NAMESPACE_CLASS_DELIMITER = '.';
        const char TYPEPARAMETER_CLASS_DELIMITER = '`';

        public enum TypeNameFormat
        {
            /// <summary>
            /// Namespace and Type
            /// </summary>
            FULL,

            /// <summary>
            /// Only namespace
            /// </summary>
            ONLY_NAMESPACE,

            /// <summary>
            /// Only type
            /// </summary>
            ONLY_TYPE
        }
        public static string GetTypeName(this TypeDeclarationSyntax source, TypeNameFormat format = TypeNameFormat.FULL)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var namespaces = new LinkedList<NamespaceDeclarationSyntax>();
            var types = new LinkedList<TypeDeclarationSyntax>();
            for (var parent = source.Parent; parent is object; parent = parent.Parent)
            {
                if (parent is NamespaceDeclarationSyntax @namespace)
                {
                    namespaces.AddFirst(@namespace);
                }
                else if (parent is TypeDeclarationSyntax type)
                {
                    types.AddFirst(type);
                }
            }

            var result = new StringBuilder();
            if (format != TypeNameFormat.ONLY_TYPE)
            {
                for (var item = namespaces.First; item is object; item = item.Next)
                {
                    result.Append(item.Value.Name).Append(NAMESPACE_CLASS_DELIMITER);
                }
            }
            if (format != TypeNameFormat.ONLY_NAMESPACE)
            {
                for (var item = types.First; item is object; item = item.Next)
                {
                    var type = item.Value;
                    AppendName(result, type);
                    result.Append(NESTED_CLASS_DELIMITER);
                }
                AppendName(result, source);
            }

            return result.ToString().TrimEnd('.');
        }

        static void AppendName(StringBuilder builder, TypeDeclarationSyntax type)
        {
            builder.Append(type.Identifier.Text);
            var typeArguments = type.TypeParameterList?.ChildNodes()
                .Count(node => node is TypeParameterSyntax) ?? 0;
            if (typeArguments != 0)
                builder.Append(TYPEPARAMETER_CLASS_DELIMITER).Append(typeArguments);
        }
    }
}
