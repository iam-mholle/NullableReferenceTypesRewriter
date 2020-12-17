using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public abstract class RewriterBase : CSharpSyntaxRewriter
  {
    private readonly Action<RewriterBase, IReadOnlyCollection<IRewritable>> _additionalRewrites;

    protected Method _currentMethod = null!;
    protected Field _currentField = null!;

    protected RewriterBase (Action<RewriterBase, IReadOnlyCollection<IRewritable>> additionalRewrites)
    {
      _additionalRewrites = additionalRewrites;
    }

    public SyntaxNode Rewrite (Field field)
    {
      _currentField = field;

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
      _currentMethod = method;

      var rewritten = VisitMethodDeclaration ((MethodDeclarationSyntax) method.RewritableSyntaxNode);

      if (rewritten == null)
        throw new InvalidOperationException ("Could not rewrite method.");

      if (rewritten == method.RewritableSyntaxNode)
        return method.RewritableSyntaxNode;

      _additionalRewrites (this, GetAdditionalRewrites (method));

      return rewritten;
    }

    protected virtual IReadOnlyCollection<IRewritable> GetAdditionalRewrites (Field field)
    {
      return Array.Empty<IRewritable>();
    }

    protected virtual IReadOnlyCollection<IRewritable> GetAdditionalRewrites (Method method)
    {
      return Array.Empty<IRewritable>();
    }
  }
}