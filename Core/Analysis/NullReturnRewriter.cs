using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class NullReturnRewriter : RewriterBase
  {
    public NullReturnRewriter (Action<RewriterBase, IReadOnlyCollection<IRewritable>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var semanticModel = CurrentMethod.SemanticModel;

      if (MayReturnNull(node, semanticModel!))
      {
        return NullUtilities.ToNullReturning (node);
      }
      return node;
    }

    protected override IReadOnlyCollection<IRewritable> GetAdditionalRewrites (Method method)
    {
      return method.Parents.Select (p => p.From).OfType<IRewritable>().ToArray();
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