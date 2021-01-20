using Microsoft.CodeAnalysis;

namespace NullableReferenceTypesRewriter
{
  public static class SymbolExtensions
  {
    public static string ToDisplayStringWithStaticModifier(this ISymbol methodSymbol)
    {
      if (methodSymbol.IsStatic)
        return "static " + methodSymbol.ToDisplayString();

      return methodSymbol.ToDisplayString();
    }
  }
}
