using Microsoft.CodeAnalysis;

namespace NullableReferenceTypesRewriter
{
  public static class SymbolExtensions
  {
    public static string ToDisplayStringWithStaticModifier(this ISymbol symbol)
    {
      if (symbol.IsStatic)
        return "static " + symbol.ToDisplayString();

      return symbol.ToDisplayString();
    }
  }
}
