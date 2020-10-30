using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraphBuilder : CSharpSyntaxWalker
  {
    private SemanticModel? _semanticModel;
    private readonly MethodGraph _graph;

    public MethodGraphBuilder()
    {
      _graph = new MethodGraph();
    }

    public void SetSemanticModel (SemanticModel semanticModel)
    {
      _semanticModel = semanticModel;
    }

    public override void VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var symbol = _semanticModel.GetDeclaredSymbol (node);
      _graph.AddMethod(UniqueMethodSymbolNameGenerator.Generate(symbol), node);

      base.VisitMethodDeclaration (node);
    }

    public override void VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      var containingMethodDeclaration = node.FirstAncestorOrSelf<MethodDeclarationSyntax> ();

      var symbolInfoCandidate = _semanticModel.GetSymbolInfo (node.Expression);
      if (symbolInfoCandidate.Symbol is IMethodSymbol invokedMethodSymbol && containingMethodDeclaration != null)
      {
        var containingMethodSymbol = _semanticModel.GetDeclaredSymbol (containingMethodDeclaration);
        _graph.AddDependency (
            UniqueMethodSymbolNameGenerator.Generate (containingMethodSymbol),
            UniqueMethodSymbolNameGenerator.Generate (invokedMethodSymbol));
      }

      base.VisitInvocationExpression (node);
    }
  }
}