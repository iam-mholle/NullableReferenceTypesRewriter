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

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (Method method)
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
               || HasNullOrEmptyBody (node))
             && (HasCanBeNullAttribute (node)
                 || NullUtilities.ReturnsNull (node, model));
    }

    private static bool HasNullOrEmptyBody (MethodDeclarationSyntax node)
    {
      return node.Body == null
             || node.Body.Statements.Count == 0;
    }

    private static bool HasCanBeNullAttribute (MemberDeclarationSyntax node)
    {
      return node.AttributeLists.SelectMany (list => list.Attributes)
          .Any (attr => attr.Name.ToString().Contains ("CanBeNull"));
    }
  }
}