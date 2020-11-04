using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public abstract class RewriterBase : CSharpSyntaxRewriter
  {
    private readonly Action<RewriterBase, IReadOnlyCollection<Dependency>> _additionalRewrites;

    protected RewriterBase (Action<RewriterBase, IReadOnlyCollection<Dependency>> additionalRewrites)
    {
      _additionalRewrites = additionalRewrites;
    }

    public SyntaxNode Rewrite (Field field)
    {
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
      var rewritten = VisitMethodDeclaration ((MethodDeclarationSyntax) method.RewritableSyntaxNode);

      if (rewritten == null)
        throw new InvalidOperationException ("Could not rewrite method.");

      if (rewritten == method.RewritableSyntaxNode)
        return method.RewritableSyntaxNode;

      _additionalRewrites (this, GetAdditionalRewrites (method));

      return rewritten;
    }

    protected virtual IReadOnlyCollection<Dependency> GetAdditionalRewrites (Field field)
    {
      return new Dependency[0];
    }

    protected virtual IReadOnlyCollection<Dependency> GetAdditionalRewrites (Method method)
    {
      return new Dependency[0];
    }
  }
}