using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  public class CanBeNullRewriter : RewriterBase
  {
    public CanBeNullRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      var resultNode = node;

      if (node.HasCanBeNullAttribute())
      {
        resultNode = resultNode.WithReturnType(NullUtilities.ToNullable(node.ReturnType));
      }

      var resultParameterList = node.ParameterList;

      foreach (var (parameter, index) in node.ParameterList.Parameters.Select((p, i) => (p, i)))
      {
        if (parameter.Type is {} && parameter.Type.IsValueType(SemanticModel))
          continue;

        if (parameter.HasCanBeNullAttribute())
        {
          resultParameterList = resultParameterList.ReplaceNode(resultParameterList.Parameters[index].Type!, NullUtilities.ToNullable(parameter.Type!));
        }
      }

      if (resultParameterList != node.ParameterList)
      {
        resultNode = resultNode.WithParameterList(resultParameterList);
      }

      return resultNode;
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
      var resultParameterList = node.ParameterList;

      foreach (var (parameter, index) in node.ParameterList.Parameters.Select((p, i) => (p, i)))
      {
        if (parameter.Type is {} && parameter.Type.IsValueType(SemanticModel))
          continue;

        if (parameter.HasCanBeNullAttribute())
        {
          resultParameterList = resultParameterList.ReplaceNode(resultParameterList.Parameters[index].Type!, NullUtilities.ToNullable(parameter.Type!));
        }
      }

      if (resultParameterList != node.ParameterList)
      {
        return node.WithParameterList(resultParameterList);
      }

      return node;
    }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
      if (node.HasCanBeNullAttribute())
        return node.WithType(NullUtilities.ToNullable(node.Type));

      return node;
    }

    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
      if (node.HasCanBeNullAttribute())
        return node.WithDeclaration(node.Declaration.WithType(NullUtilities.ToNullable(node.Declaration.Type)));

      return node;
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode node)
    {
      var children = node.Children.Select(d => d.To);
      var parents = node.Parents.Select(d => d.From);
      return children.Concat(parents)
          .OfType<IRewritable>()
          .Select(n => (n, RewriteCapability.ParameterChange | RewriteCapability.ReturnValueChange))
          .ToArray();
    }
  }
}
