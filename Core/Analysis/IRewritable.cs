using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NullableReferenceTypesRewriter.Analysis
{
  public interface IRewritable
  {
    SyntaxNode RewritableSyntaxNode { get; }
    void Rewrite (CSharpSyntaxRewriter rewriter);
  }
}