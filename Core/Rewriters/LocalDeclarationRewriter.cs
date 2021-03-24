﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  public class LocalDeclarationRewriter : RewriterBase
  {
    // TODO: handle default(T) initializer.
    public LocalDeclarationRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
      if (node.Declaration.Type.IsVar)
        return node;

      if (node.Declaration.Type is NullableTypeSyntax)
        return node;

      if (node.Declaration.Type.IsValueType(SemanticModel))
        return node;

      var type = node.Declaration.Type;

      var typeInfo = SemanticModel.GetTypeInfo(type);

      var isNullable = node.Declaration.Variables
          .Where (variable => variable.Initializer != null)
          .Any (variable => NullUtilities.CanBeNull (variable.Initializer!.Value, SemanticModel));

      isNullable |= typeInfo.Type!.IsReferenceType
                    && node.Declaration.Variables.Any(v => v.Initializer is null);

      return isNullable
          ? node.WithDeclaration (node.Declaration.WithType (NullUtilities.ToNullable (type)))
          : node;
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode method)
    {
      return base.GetAdditionalRewrites(method)
          .Concat(
              method.Children
                  .Select(p => p.To)
                  .OfType<IRewritable>()
                  .Select(r => (r, RewriteCapability.ParameterChange)))
          .ToArray();
    }
  }
}
