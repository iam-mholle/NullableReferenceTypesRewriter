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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NullableReferenceTypesRewriter.Utilities
{
  public static class NullUtilities
  {
    public static bool ReturnsNull (MethodDeclarationSyntax node, SemanticModel semanticModel)
    {
      return node.Body?.Statements != null
             && ReturnsNull(node.Body.Statements, semanticModel);
    }

    public static bool ReturnsNull (IEnumerable<StatementSyntax> statements, SemanticModel semanticModel)
    {
      var statementSyntaxes = statements as StatementSyntax[] ?? statements.ToArray();
      var statementSyntaxesWithoutLocalFunctions = statementSyntaxes.Where (s => !(s is LocalFunctionStatementSyntax)).ToArray();
      var returnStatements = semanticModel.AnalyzeControlFlow (
              statementSyntaxesWithoutLocalFunctions.First(),
              statementSyntaxesWithoutLocalFunctions.Last())
          ?.ReturnStatements;

      return returnStatements!.Value.Any (
          stmt => stmt is ReturnStatementSyntax returnStatement
                  && CanBeNull (returnStatement.Expression!, semanticModel));
    }

    public static bool CanBeNull (ExpressionSyntax expression, SemanticModel semanticModel)
    {
      var typeInfo = semanticModel.GetTypeInfo (expression);

      return typeInfo.Nullability.FlowState switch
      {
          NullableFlowState.MaybeNull => true,
          _ => false
      };
    }

    public static bool ReturnsVoid (MethodDeclarationSyntax node)
    {
      return node.ReturnType is PredefinedTypeSyntax type
             && type.Keyword.Kind() == SyntaxKind.VoidKeyword;
    }

    public static MethodDeclarationSyntax ToNullReturning (SemanticModel semanticModel, MethodDeclarationSyntax method)
    {
      if (ShouldAnnotateType(semanticModel, method, method.ReturnType))
        return method.WithReturnType (ToNullable (method.ReturnType));

      return method;
    }

    public static TypeSyntax ToNullableWithGenericsCheck (SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax, TypeSyntax typeSyntax)
    {
      if (ShouldAnnotateType(semanticModel, methodDeclarationSyntax, typeSyntax))
        return ToNullable(typeSyntax);

      return typeSyntax;
    }

    public static TypeSyntax ToNullableWithGenericsCheck (SemanticModel semanticModel, TypeDeclarationSyntax classDeclarationSyntax, TypeSyntax typeSyntax)
    {
      if (ShouldAnnotateType(semanticModel, classDeclarationSyntax, typeSyntax))
        return ToNullable(typeSyntax);

      return typeSyntax;
    }

    public static TypeSyntax ToNullable(TypeSyntax typeSyntax)
    {
      if (typeSyntax is NullableTypeSyntax)
        return typeSyntax;
      var nullable = NullableType (typeSyntax.WithoutTrailingTrivia());
      return nullable
          .WithTrailingTrivia (typeSyntax.GetTrailingTrivia());
    }

    private static bool IsGenericParameter(MethodDeclarationSyntax method, TypeSyntax typeSyntax)
    {
      var containingClasses = method.Ancestors()
          .Where(a => a.IsKind(SyntaxKind.ClassDeclaration))
          .OfType<ClassDeclarationSyntax>();

      return IsGenericMethodParameter(method, typeSyntax)
             || containingClasses.Any(c => IsGenericClassParameter(c, typeSyntax));
    }

    private static bool IsGenericMethodParameter(MethodDeclarationSyntax method, TypeSyntax typeSyntax)
    {
      return method.TypeParameterList is { } && method.TypeParameterList.Parameters.Any(tp => typeSyntax.ToString().Equals(tp.ToString()));
    }

    private static bool IsGenericClassParameter(TypeDeclarationSyntax @class, TypeSyntax typeSyntax)
    {
      return @class.TypeParameterList is { } && @class.TypeParameterList.Parameters.Any(tp => typeSyntax.ToString().Equals(tp.ToString()));
    }

    private static IReadOnlyCollection<TypeParameterConstraintSyntax> GetConstraints(MethodDeclarationSyntax methodDeclarationSyntax, TypeSyntax typeSyntax)
    {
      var containingClasses = methodDeclarationSyntax.Ancestors()
          .Where(a => a.IsKind(SyntaxKind.ClassDeclaration))
          .OfType<ClassDeclarationSyntax>();
      var classClauses = containingClasses.SelectMany(c => c.ConstraintClauses);

      return methodDeclarationSyntax.ConstraintClauses.Concat(classClauses).Where(clause => clause.Name.ToString() == typeSyntax.ToString()).SelectMany(c => c.Constraints).ToArray();
    }

    private static IReadOnlyCollection<TypeParameterConstraintSyntax> GetConstraints(TypeDeclarationSyntax classDeclarationSyntax, TypeSyntax typeSyntax)
    {
      return classDeclarationSyntax.ConstraintClauses.Where(clause => clause.Name.ToString() == typeSyntax.ToString()).SelectMany(c => c.Constraints).ToArray();
    }

    private static bool ShouldAnnotateType(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax, TypeSyntax typeSyntax)
    {
      if (IsGenericParameter(methodDeclarationSyntax, typeSyntax))
      {
        var constraints = GetConstraints(methodDeclarationSyntax, typeSyntax);
        if (constraints.Any(c => c is ClassOrStructConstraintSyntax { ClassOrStructKeyword: { Value: "class" } })
            || constraints.OfType<TypeConstraintSyntax>().Any(c => semanticModel.GetTypeInfo(c.Type).Type!.IsReferenceType))
        {
          return true;
        }

        return false;
      }

      return true;
    }

    private static bool ShouldAnnotateType(SemanticModel semanticModel, TypeDeclarationSyntax classDeclarationSyntax, TypeSyntax typeSyntax)
    {
      if (IsGenericClassParameter(classDeclarationSyntax, typeSyntax))
      {
        var constraints = GetConstraints(classDeclarationSyntax, typeSyntax);
        if (constraints.Any(c => c is ClassOrStructConstraintSyntax { ClassOrStructKeyword: { Value: "class" } })
            || constraints.OfType<TypeConstraintSyntax>().Any(c => semanticModel.GetTypeInfo(c.Type).Type!.IsReferenceType))
        {
          return true;
        }

        return false;
      }

      return true;
    }
  }
}