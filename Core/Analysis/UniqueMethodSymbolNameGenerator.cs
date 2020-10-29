using System.Text;
using Microsoft.CodeAnalysis;

namespace NullableReferenceTypesRewriter.Analysis
{
  public static class UniqueMethodSymbolNameGenerator
  {
    public static string Generate (IMethodSymbol methodSymbol)
    {
      return methodSymbol.ToString();
    }
  }
}