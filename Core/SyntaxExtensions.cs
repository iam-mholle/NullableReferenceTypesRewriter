using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter
{
  public static class SyntaxExtensions
  {
    public static bool HasNotNullAttribute(this ParameterSyntax parameter)
      => parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.ToString().EndsWith("NotNull")));

    public static bool HasCanBeNullAttribute (this MemberDeclarationSyntax member) =>
        member.AttributeLists.SelectMany (list => list.Attributes)
            .Any (attr => attr.Name.ToString().Contains ("CanBeNull"));

    public static bool HasCanBeNullAttribute (this ParameterSyntax parameter) =>
        parameter.AttributeLists.SelectMany (list => list.Attributes)
            .Any (attr => attr.Name.ToString().Contains ("CanBeNull"));

    public static bool HasNonAutoGetter(this PropertyDeclarationSyntax property)
      => property.AccessorList != null
         && property.AccessorList.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.GetKeyword))
         && (property.AccessorList.Accessors.Single(a => a.Keyword.IsKind(SyntaxKind.GetKeyword)).Body != null
             || property.AccessorList.Accessors.Single(a => a.Keyword.IsKind(SyntaxKind.GetKeyword)).ExpressionBody != null);

    public static bool IsExpressionBodied(this PropertyDeclarationSyntax property)
      => property.ExpressionBody != null;

    public static bool IsValueType (this TypeSyntax declaration, SemanticModel semanticModel)
    {
      if (declaration is ArrayTypeSyntax)
        return false;

      var typeSymbol = semanticModel.GetTypeInfo (declaration).Type as INamedTypeSymbol;

      return typeSymbol == null || typeSymbol.IsValueType;
    }

    public static bool IsReferenceType(this TypeSyntax type, SemanticModel semanticModel)
      => semanticModel.GetTypeInfo(type).Type?.IsReferenceType ?? false;

    public static bool IsDefaultNullLiteral(this ParameterSyntax parameterSyntax)
      => parameterSyntax.Default is { Value: LiteralExpressionSyntax { Token: { Text: "null" } } };

    public static bool IsDefaultDefaultLiteral(this ParameterSyntax parameterSyntax)
      => parameterSyntax.Default is { Value: LiteralExpressionSyntax { Token: { Text: "default" } } };

    public static bool IsDefaultDefaultExpression(this ParameterSyntax parameterSyntax)
      => parameterSyntax.Default is { Value: DefaultExpressionSyntax _ };

    public static bool HasNullOrEmptyBody (this MethodDeclarationSyntax method)
      => method.Body == null || method.Body.Statements.Count == 0;

    public static bool IsInitializedToNull (this VariableDeclaratorSyntax variableDeclarator, SemanticModel semanticModel)
      => variableDeclarator.Initializer.IsInitializedToNull(semanticModel);

    public static bool IsInitializedToNotNull (this VariableDeclaratorSyntax variableDeclarator, SemanticModel semanticModel)
      => variableDeclarator.Initializer.IsInitializedToNotNull(semanticModel);

    public static bool IsInitializedToNull (this PropertyDeclarationSyntax propertyDeclarator, SemanticModel semanticModel)
      => propertyDeclarator.Initializer.IsInitializedToNull(semanticModel);

    public static bool IsInitializedToNotNull (this PropertyDeclarationSyntax propertyDeclarator, SemanticModel semanticModel)
      => propertyDeclarator.Initializer.IsInitializedToNotNull(semanticModel);

    public static bool IsInitializedToNull (this EqualsValueClauseSyntax? syntax, SemanticModel semanticModel)
    {
      if (syntax != null)
        return NullUtilities.CanBeNull(syntax.Value, semanticModel);

      return false;
    }

    public static bool IsInitializedToNotNull (this EqualsValueClauseSyntax? syntax, SemanticModel semanticModel)
    {
      if (syntax != null)
        return !NullUtilities.CanBeNull(syntax.Value, semanticModel);

      return false;
    }

    public static bool IsAbstract(this MemberDeclarationSyntax syntax)
      => syntax.Modifiers.Any(SyntaxKind.AbstractKeyword);

    public static bool IsAutoProperty(this PropertyDeclarationSyntax propertyDeclarationSyntax)
      => propertyDeclarationSyntax.AccessorList?.Accessors.All(a => a.Body is null) ?? false;
  }
}
