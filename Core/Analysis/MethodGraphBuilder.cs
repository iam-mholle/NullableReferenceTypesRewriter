using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraphBuilder : CSharpSyntaxWalker
  {
    private readonly SharedCompilation _compilation;
    private readonly MethodGraph _graph;

    public IMethodGraph Graph => _graph;

    public MethodGraphBuilder(SharedCompilation compilation)
    {
      _compilation = compilation;
      _graph = new MethodGraph(compilation);
    }

    public override void VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var symbol = GetSemanticModel(node).GetDeclaredSymbol (node);
      _graph.AddMethod(UniqueSymbolNameGenerator.Generate(symbol), symbol);

      base.VisitMethodDeclaration (node);
    }

    private SemanticModel GetSemanticModel (SyntaxNode node)
    {
      return _compilation.GetSemanticModel (node.SyntaxTree);
    }

    public override void VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      var containingMethodDeclaration = node.FirstAncestorOrSelf<MethodDeclarationSyntax> ();

      var symbolInfoCandidate = GetSemanticModel(node).GetSymbolInfo (node.Expression);
      if (symbolInfoCandidate.Symbol is IMethodSymbol invokedMethodSymbol && containingMethodDeclaration != null)
      {
        var containingMethodSymbol = GetSemanticModel(node).GetDeclaredSymbol (containingMethodDeclaration);

        if (invokedMethodSymbol.DeclaringSyntaxReferences.IsEmpty)
        {
          _graph.AddExternalMethod (UniqueSymbolNameGenerator.Generate (invokedMethodSymbol), invokedMethodSymbol);
        }

        _graph.AddDependency (
            UniqueSymbolNameGenerator.Generate (containingMethodSymbol),
            UniqueSymbolNameGenerator.Generate (invokedMethodSymbol));
      }

      base.VisitInvocationExpression (node);
    }

    public override void VisitFieldDeclaration (FieldDeclarationSyntax node)
    {
      foreach (var declaredField in node.Declaration.Variables)
      {
        var symbol = (IFieldSymbol) GetSemanticModel(node).GetDeclaredSymbol (declaredField);
        // TODO: Do I want to pass in node here?
        _graph.AddField (UniqueSymbolNameGenerator.Generate (symbol), symbol);
      }

      base.VisitFieldDeclaration (node);
    }
  }
}