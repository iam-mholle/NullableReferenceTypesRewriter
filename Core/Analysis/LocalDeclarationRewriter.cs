using System;
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

      var typeInfo = CurrentMethod.SemanticModel.GetTypeInfo(type);

      var isNullable = node.Declaration.Variables
          .Where (variable => variable.Initializer != null)
          .Any (variable => NullUtilities.CanBeNull (variable.Initializer!.Value, CurrentMethod.SemanticModel));

      isNullable |= typeInfo.Type!.IsReferenceType
                    && node.Declaration.Variables.Any(v => v.Initializer is null);

      return isNullable
          ? node.WithDeclaration (node.Declaration.WithType (NullUtilities.ToNullableWithGenericsCheck (type)))
          : node;
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(Method method)
    {
      return method.Children
          .Select(p => p.To)
          .OfType<IRewritable>()
          .Select(r => (r, RewriteCapability.ParameterChange))
          .ToArray();
    }
  }
}
