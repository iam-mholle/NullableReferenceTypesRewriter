using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraphBuilder : CSharpSyntaxWalker
  {
    private SemanticModel? _semanticModel;
    private Document? _document;
    private readonly MethodGraph _graph;

    public IMethodGraph Graph => _graph;

    public MethodGraphBuilder()
    {
      _graph = new MethodGraph();
    }

    public void SetDocument (Document document)
    {
      _document = document;
      _semanticModel = document.GetSemanticModelAsync().GetAwaiter().GetResult();
    }

    public override void VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var symbol = _semanticModel.GetDeclaredSymbol (node);
      _graph.AddMethod(UniqueSymbolNameGenerator.Generate(symbol), node, _document!);

      base.VisitMethodDeclaration (node);
    }

    public override void VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      var containingMethodDeclaration = node.FirstAncestorOrSelf<MethodDeclarationSyntax> ();

      var symbolInfoCandidate = _semanticModel.GetSymbolInfo (node.Expression);
      if (symbolInfoCandidate.Symbol is IMethodSymbol invokedMethodSymbol && containingMethodDeclaration != null)
      {
        var containingMethodSymbol = _semanticModel.GetDeclaredSymbol (containingMethodDeclaration);

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
        var symbol = (IFieldSymbol) _semanticModel.GetDeclaredSymbol (declaredField);
        // TODO: Do I want to pass in node here?
        _graph.AddField (UniqueSymbolNameGenerator.Generate (symbol), node);
      }

      base.VisitFieldDeclaration (node);
    }
  }
}