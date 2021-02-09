using System.Text;
using Microsoft.CodeAnalysis;

namespace NullableReferenceTypesRewriter.Analysis
{
  public static class UniqueSymbolNameGenerator
  {
    public static string Generate (IMethodSymbol methodSymbol)
    {
      return methodSymbol.ToString();
    }

    public static string Generate (IFieldSymbol fieldSymbol)
    {
      return fieldSymbol.ToString();
    }

    public static string Generate (IPropertySymbol propertySymbol)
    {
      return propertySymbol.ToString();
    }

    public static string Generate (IEventSymbol eventSymbol)
    {
      return eventSymbol.ToString();
    }
  }
}