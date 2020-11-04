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
    private readonly SemanticModel _semanticModel;

    public NullReturnRewriter (Action<RewriterBase, IReadOnlyCollection<Dependency>> additionalRewrites, SemanticModel semanticModel)
        : base(additionalRewrites)
    {
      _semanticModel = semanticModel;
    }

    public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      if (MayReturnNull(node, _semanticModel))
      {
        return NullUtilities.ToNullReturning (node);
      }
      return node;
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