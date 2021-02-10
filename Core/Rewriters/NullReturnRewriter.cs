using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  // TODO: Does not propagate to interfaces.
  /// <summary>
  /// Specs:<br/>
  /// - Methods with a body returning a nullable value are rewritten to nullable reference type<br/>
  /// </summary>
  public class NullReturnRewriter : RewriterBase
  {
    public NullReturnRewriter (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var semanticModel = CurrentMethod.SemanticModel;

      if (MayReturnNull(node, semanticModel!))
      {
        return NullUtilities.ToNullReturning (semanticModel, node);
      }
      return node;
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (INode method)
    {
      return method.Parents
          .Select (p => p.From)
          .OfType<IRewritable>()
          .Select(r => (r, RewriteCapability.ReturnValueChange))
          .ToArray();
    }

    private static bool MayReturnNull (MethodDeclarationSyntax node, SemanticModel model)
    {
      return !(NullUtilities.ReturnsVoid (node)
               || node.HasNullOrEmptyBody ())
             && (node.HasCanBeNullAttribute()
                 || NullUtilities.ReturnsNull (node, model));
    }
  }
}