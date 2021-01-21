using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter
{
  public static class SyntaxExtensions
  {
    public static bool HasNotNullAttribute(this ParameterSyntax parameter)
      => parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.ToString().EndsWith("NotNull")));

    public static bool HasNonAutoGetter(this PropertyDeclarationSyntax property)
      => property.AccessorList != null
         && property.AccessorList.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.GetKeyword))
         && (property.AccessorList.Accessors.Single(a => a.Keyword.IsKind(SyntaxKind.GetKeyword)).Body != null
             || property.AccessorList.Accessors.Single(a => a.Keyword.IsKind(SyntaxKind.GetKeyword)).ExpressionBody != null);

    public static bool IsExpressionBodied(this PropertyDeclarationSyntax property)
      => property.ExpressionBody != null;
  }
}
