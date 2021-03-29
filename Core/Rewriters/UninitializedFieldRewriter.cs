// Copyright (c) rubicon IT GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  /// <summary>
  /// Specs:<br/>
  /// - Value-type fields are ignored<br/>
  /// - Nullable reference type fields are ignored<br/>
  /// - Fields containing at least one initialization to a nullable value are rewritten to be of a nullable reference type<br/>
  /// - Uninitialized fields are rewritten to return a nullable reference type
  /// </summary>
  public class UninitializedFieldRewriter : RewriterBase
  {
    public UninitializedFieldRewriter (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitFieldDeclaration (FieldDeclarationSyntax node)
    {
      if (IsValueType(SemanticModel, node))
        return node;

      if (IsNullable(SemanticModel, node))
        return node;

      if (node.Declaration.Variables.All(d => d.IsInitializedToNotNull(SemanticModel)))
        return node;

      var classSyntax = (TypeDeclarationSyntax) node.Ancestors().First(a => a.IsKind(SyntaxKind.ClassDeclaration) || a.IsKind(SyntaxKind.StructDeclaration));

      if (node.Declaration.Variables.Any(d => d.IsInitializedToNull(SemanticModel)))
        return ToNullable(node);

      var symbol = SemanticModel.GetDeclaredSymbol(node.Declaration.Variables.First());

      var constructors = symbol!.IsStatic switch
      {
          true => classSyntax.ChildNodes()
              .Where(n => n.IsKind(SyntaxKind.ConstructorDeclaration))
              .Cast<ConstructorDeclarationSyntax>()
              .Where(c => c.Modifiers.Any(SyntaxKind.StaticKeyword))
              .ToArray(),
          false => classSyntax.ChildNodes()
              .Where(n => n.IsKind(SyntaxKind.ConstructorDeclaration))
              .Cast<ConstructorDeclarationSyntax>()
              .Where(c => !c.Modifiers.Any(SyntaxKind.StaticKeyword))
              .ToArray(),
      };

      var isInitializedToNotNull = constructors.All(c => VariableInitializedToNotNullInCtorChain(SemanticModel, c, node.Declaration.Variables.First()));

      if (constructors.Length == 0 || !isInitializedToNotNull)
        return ToNullable(node);

      return node;

      SyntaxNode? ToNullable(FieldDeclarationSyntax node) => node.WithDeclaration(node.Declaration.WithType(NullUtilities.ToNullable(node.Declaration.Type)));
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode method)
    {
      return base.GetAdditionalRewrites(method)
          .Concat(
              method.Parents
                  .Select(p => p.From)
                  .OfType<IRewritable>()
                  .Select(r => (r, RewriteCapability.ReturnValueChange)))
          .ToArray();
    }

    private bool VariableInitializedToNotNullInCtorChain(SemanticModel semanticModel, ConstructorDeclarationSyntax constructor, VariableDeclaratorSyntax variable)
    {
      var isInitializedToNotNullInCurrent = VariableInitializedToNotNull(semanticModel, constructor, variable);

      if (isInitializedToNotNullInCurrent)
        return true;

      if (constructor.Initializer is null || constructor.Initializer.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword))
        return false;

      var thisConstructorSymbol = semanticModel.GetSymbolInfo(constructor.Initializer).Symbol ?? throw new InvalidOperationException();
      var thisConstructorSyntax = (ConstructorDeclarationSyntax) thisConstructorSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

      return VariableInitializedToNotNullInCtorChain(semanticModel, thisConstructorSyntax, variable);
    }

    private bool VariableInitializedToNotNull(SemanticModel semanticModel, ConstructorDeclarationSyntax constructor, VariableDeclaratorSyntax variable)
    {
      var assignments = constructor.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().ToArray();

      var variableSymbol = semanticModel.GetDeclaredSymbol(variable);
      var fieldAssignments = assignments.Where(a => SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(a.Left).Symbol, variableSymbol)).ToArray();

      if (fieldAssignments.Length == 0)
      {
        return false;
      }

      return fieldAssignments.All(a => !NullUtilities.CanBeNull(a.Right, semanticModel));
    }

    private bool IsValueType(SemanticModel semanticModel, FieldDeclarationSyntax declaration)
      => declaration.Declaration.Type.IsValueType(semanticModel);

    private bool IsNullable(SemanticModel semanticModel, FieldDeclarationSyntax syntax)
      => (semanticModel.GetDeclaredSymbol(syntax.Declaration.Variables.First()) as IFieldSymbol)?.Type.NullableAnnotation == NullableAnnotation.Annotated;
  }
}