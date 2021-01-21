using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;

namespace NullableReferenceTypesRewriter.Rewriters
{
  // TODO: Separate CanBeNullAttributeRewriter
  // TODO: Integrate properties.
  // TODO: Integrate events.
  // TODO: Document rewriter capabilities thoroughly.
  // TODO: Don't Annotate [NotNull] parameters or return types, output error instead.
  // TODO: Add PropertyNullReturnRewriter
  public abstract class RewriterBase : CSharpSyntaxRewriter
  {
    private readonly Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> _additionalRewrites;

    protected Method CurrentMethod = null!;
    protected Field CurrentField = null!;
    protected Property CurrentProperty = null!;

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

    public SyntaxNode Rewrite (Property property)
    {
      CurrentProperty = property;

      var rewritten = VisitPropertyDeclaration ((PropertyDeclarationSyntax) property.RewritableSyntaxNode);

      if (rewritten == null)
        throw new InvalidOperationException ("Could not rewrite property.");

      if (rewritten == property.RewritableSyntaxNode)
        return property.RewritableSyntaxNode;

      _additionalRewrites (this, GetAdditionalRewrites (property));

      return rewritten;
    }

    public SyntaxNode Rewrite (Method method)
    {
      CurrentMethod = method;

      var rewritten = method.RewritableSyntaxNode switch
      {
          MethodDeclarationSyntax m => VisitMethodDeclaration(m),
          ConstructorDeclarationSyntax c => VisitConstructorDeclaration(c),
          _ => throw new InvalidOperationException($"{method.RewritableSyntaxNode.GetType()} is not supported."),
      };

      if (rewritten == null)
        throw new InvalidOperationException ("Could not rewrite method.");

      if (rewritten == method.RewritableSyntaxNode)
        return method.RewritableSyntaxNode;

      _additionalRewrites (this, GetAdditionalRewrites (method));

      return rewritten;
    }

    protected virtual IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (INode node)
    {
      return Array.Empty<(IRewritable, RewriteCapability)>();
    }
  }
}