using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public abstract class RewriterBase : CSharpSyntaxRewriter
  {
    private readonly Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> _additionalRewrites;

    protected Method CurrentMethod = null!;
    protected Field CurrentField = null!;

    protected RewriterBase (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
    {
      _additionalRewrites = additionalRewrites;
    }

    public SyntaxNode Rewrite (Field field)
    {
      CurrentField = field;

      var rewritten = VisitFieldDeclaration ((FieldDeclarationSyntax) field.RewritableSyntaxNode);

      if (rewritten == null)
        throw new InvalidOperationException ("Could not rewrite field.");

      if (rewritten == field.RewritableSyntaxNode)
        return field.RewritableSyntaxNode;

      _additionalRewrites (this, GetAdditionalRewrites (field));

      return rewritten;
    }

    public SyntaxNode Rewrite (Method method)
    {
      CurrentMethod = method;

      var rewritten = VisitMethodDeclaration ((MethodDeclarationSyntax) method.RewritableSyntaxNode);

      if (rewritten == null)
        throw new InvalidOperationException ("Could not rewrite method.");

      if (rewritten == method.RewritableSyntaxNode)
        return method.RewritableSyntaxNode;

      _additionalRewrites (this, GetAdditionalRewrites (method));

      return rewritten;
    }

    protected virtual IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (Field field)
    {
      return Array.Empty<(IRewritable, RewriteCapability)>();
    }

    protected virtual IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (Method method)
    {
      return Array.Empty<(IRewritable, RewriteCapability)>();
    }
  }
}