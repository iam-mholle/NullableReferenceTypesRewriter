using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter
{
  public static class SyntaxExtensions
  {
    public static bool HasNotNullAttribute(this ParameterSyntax parameter)
      => parameter.AttributeLists.Any(l => l.Attributes.Any(a => a.ToString().EndsWith("NotNull")));
  }
}
