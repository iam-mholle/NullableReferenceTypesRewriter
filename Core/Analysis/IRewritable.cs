using Microsoft.CodeAnalysis;

namespace NullableReferenceTypesRewriter.Analysis
{
  public interface IRewritable
  {
    SyntaxNode RewritableSyntaxNode { get; }
    void Rewrite (RewriterBase rewriter);
  }
}