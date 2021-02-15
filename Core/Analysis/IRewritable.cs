using Microsoft.CodeAnalysis;
using NullableReferenceTypesRewriter.Rewriters;

namespace NullableReferenceTypesRewriter.Analysis
{
  public interface IRewritable
  {
    SyntaxNode RewritableSyntaxNode { get; }
    void Rewrite (RewriterBase rewriter);
  }
}