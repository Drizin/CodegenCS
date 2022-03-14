using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace SampleSourceGenerator
{
    internal class MySyntaxReceiver : ISyntaxReceiver
    {
        public IList<ClassDeclarationSyntax> ClassesToProcess { get; } = new List<ClassDeclarationSyntax>();

        public IList<MethodDeclarationSyntax> MethodsToProcess { get; } = new List<MethodDeclarationSyntax>();

        /// <summary>
        /// Called for every syntax node in the compilation. Will store the classes we're interested in
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // any field with at least one attribute is a candidate for being cloneable
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                // we could be excluding by classDeclarationSyntax.Identifier
                // or by searching for some attribute like classDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(c => c.Name == "FullAttributeName")
                ClassesToProcess.Add(classDeclarationSyntax);
            }

            if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax && methodDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword) && methodDeclarationSyntax.Body==null)
            {
                MethodsToProcess.Add(methodDeclarationSyntax);
            }
        }

    }
}
