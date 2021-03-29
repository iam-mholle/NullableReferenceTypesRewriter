// Copyright (c) rubicon IT GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;

namespace NullableReferenceTypesRewriter.Rewriters
{
  // TODO: Document rewriter capabilities thoroughly.
  // TODO: out Parameter rewriter
  // TODO: Don't Annotate [NotNull] parameters or return types, output error instead.
  public abstract class RewriterBase : CSharpSyntaxRewriter
  {
    private readonly Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> _additionalRewrites;

    private INode? _deferredRewritesScope = null;
    private List<(IRewritable rewritable, RewriteCapability capability)>? _deferredRewrites = null;

    protected Method CurrentMethod = null!;
    protected Field CurrentField = null!;
    protected Property CurrentProperty = null!;
    protected Event CurrentEvent = null!;

    protected SemanticModel SemanticModel => CurrentMethod?.SemanticModel
                                             ?? CurrentField?.SemanticModel
                                             ?? CurrentProperty?.SemanticModel
                                             ?? CurrentEvent?.SemanticModel
                                             ?? throw new InvalidOperationException("Not reachable.");

    protected INode CurrentNode => (INode?) CurrentMethod
                                   ?? (INode?) CurrentProperty
                                   ?? (INode?) CurrentField
                                   ?? CurrentEvent;

    protected RewriterBase (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
    {
      _additionalRewrites = additionalRewrites;
    }

    public SyntaxNode Rewrite (Field field)
    {
      CurrentField = field;

      EnsureCleanDeferredRewritables();

      try
      {
        var rewritten = VisitFieldDeclaration ((FieldDeclarationSyntax) field.RewritableSyntaxNode);

        if (rewritten == null)
          throw new InvalidOperationException ("Could not rewrite field.");

        if (rewritten == field.RewritableSyntaxNode)
          return field.RewritableSyntaxNode;

        _additionalRewrites (this, GetAdditionalRewrites (field));

        return rewritten;
      }
      finally
      {
        CurrentField = null!;
      }
    }

    public SyntaxNode Rewrite (Property property)
    {
      CurrentProperty = property;

      EnsureCleanDeferredRewritables();

      try
      {
        var rewritten = VisitPropertyDeclaration ((PropertyDeclarationSyntax) property.RewritableSyntaxNode);

        if (rewritten == null)
          throw new InvalidOperationException ("Could not rewrite property.");

        if (rewritten == property.RewritableSyntaxNode)
          return property.RewritableSyntaxNode;

        _additionalRewrites (this, GetAdditionalRewrites (property));

        return rewritten;
      }
      finally
      {
        CurrentProperty = null!;
      }
    }

    public SyntaxNode Rewrite (Method method)
    {
      CurrentMethod = method;

      EnsureCleanDeferredRewritables();

      try
      {
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
      finally
      {
        CurrentMethod = null!;
      }
    }

    public SyntaxNode Rewrite(Event @event)
    {
      CurrentEvent = @event;

      EnsureCleanDeferredRewritables();

      try
      {
        var rewritten = @event.RewritableSyntaxNode switch
        {
            EventDeclarationSyntax e => VisitEventDeclaration(e),
            EventFieldDeclarationSyntax f => VisitEventFieldDeclaration(f),
            _ => throw new InvalidOperationException($"{@event.RewritableSyntaxNode.GetType()} is not supported."),
        };

        if (rewritten == null)
          throw new InvalidOperationException ("Could not rewrite event.");

        if (rewritten == @event.RewritableSyntaxNode)
          return @event.RewritableSyntaxNode;

        _additionalRewrites (this, GetAdditionalRewrites (@event));

        return rewritten;
      }
      finally
      {
        CurrentEvent = null!;
      }
    }

    protected virtual IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (INode node)
    {
      return ((IReadOnlyCollection<(IRewritable, RewriteCapability)>?) _deferredRewrites)
             ?? Array.Empty<(IRewritable, RewriteCapability)>();
    }

    protected void AddDeferredRewrite(IRewritable rewritable, RewriteCapability capability)
    {
      _deferredRewrites?.Add((rewritable, capability));
    }

    private void EnsureCleanDeferredRewritables()
    {
      if (_deferredRewritesScope != CurrentNode)
      {
        _deferredRewritesScope = CurrentNode;
        _deferredRewrites = new List<(IRewritable rewritable, RewriteCapability capability)>();
      }
    }
  }
}
