using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  public class MethodArgumentRewriter : RewriterBase
  {
    public MethodArgumentRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      return VisitBaseMethodDeclaration(node, false);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
      return VisitBaseMethodDeclaration(node, true);
    }

    public SyntaxNode? VisitBaseMethodDeclaration(BaseMethodDeclarationSyntax node, bool isCtor)
    {
      var symbol = (IMethodSymbol) ModelExtensions.GetDeclaredSymbol(SemanticModel, node)!;
      var parents = CurrentNode.Parents.Select(p => p.From).ToArray();
      var parametersToAnnotate = new List<int>();
      for (var i = 0; i < node.ParameterList.Parameters.Count; i++)
      {
        foreach (var parent in parents)
        {
          if (parent is Method parentMethod)
          {
            var isArgumentPossiblyNull = IsArgumentPossiblyNull(symbol, isCtor, parentMethod, i);
            if (isArgumentPossiblyNull)
            {
              parametersToAnnotate.Add(i);
            }
          }
        }
      }

      if (parametersToAnnotate.Count == 0)
        return node;

      var newList = node.ParameterList;

      foreach (var parameterIndex in parametersToAnnotate)
      {
        var existingParameter = newList.Parameters[parameterIndex];

        if (existingParameter.HasNotNullAttribute())
        {
          Console.WriteLine($"ERROR: Trying to annotate NotNull parameter {existingParameter.ToString()} in {CurrentNode}");
        }
        else
        {
          newList = newList.ReplaceNode(
              existingParameter,
              existingParameter.WithType(NullUtilities.ToNullableWithGenericsCheck(SemanticModel, node, existingParameter.Type!)));
        }
      }

      return node.WithParameterList(newList);
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

    private bool IsArgumentPossiblyNull(IMethodSymbol symbol, bool isCtor, Method method, int argumentIndex)
    {
      var syntax = method.MethodDeclaration;

      if (syntax.Body is null)
        return false;

      return isCtor switch
      {
          false => IsInvocationArgumentNullable(symbol, method, syntax, argumentIndex),
          true => IsObjectCreationArgumentNullable(symbol, method, syntax, argumentIndex),
      };
    }

    private bool IsInvocationArgumentNullable(ISymbol symbol, Method method, BaseMethodDeclarationSyntax syntax, int argumentIndex)
    {
      var invocations = syntax.Body!.DescendantNodes()
          .Where(n => n.IsKind(SyntaxKind.InvocationExpression))
          .Cast<InvocationExpressionSyntax>()
          .Where(i => SymbolEqualityComparer.Default.Equals(method.SemanticModel.GetSymbolInfo(i.Expression).Symbol, symbol))
          .ToArray();

      return invocations.Any(i =>
      {
        if (i.ArgumentList.Arguments.Count <= argumentIndex)
        {
          return false;
        }

        return NullUtilities.CanBeNull (i.ArgumentList.Arguments[argumentIndex].Expression, method.SemanticModel);
      });
    }

    private bool IsObjectCreationArgumentNullable(ISymbol symbol, Method method, BaseMethodDeclarationSyntax syntax, int argumentIndex)
    {
      var creations = syntax.Body!.DescendantNodes()
          .Where(n => n.IsKind(SyntaxKind.ObjectCreationExpression))
          .Cast<ObjectCreationExpressionSyntax>()
          .Where(i => SymbolEqualityComparer.Default.Equals(method.SemanticModel.GetSymbolInfo(i).Symbol, symbol))
          .ToArray();

      return creations.Any(i =>
      {
        if (i.ArgumentList is null || i.ArgumentList.Arguments.Count <= argumentIndex)
        {
          return false;
        }

        return NullUtilities.CanBeNull (i.ArgumentList.Arguments[argumentIndex].Expression, method.SemanticModel);
      });
    }
  }
}
