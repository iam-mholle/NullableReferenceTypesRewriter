﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class LocalDeclarationRewriter : RewriterBase
  {
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

      var type = node.Declaration.Type;

      var isNullable = node.Declaration.Variables
          .Where (variable => variable.Initializer != null)
          .Any (variable => NullUtilities.CanBeNull (variable.Initializer!.Value, CurrentMethod.SemanticModel));

      return isNullable
          ? node.WithDeclaration (node.Declaration.WithType (NullUtilities.ToNullable (type)))
          : node;
    }
  }
}