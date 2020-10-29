using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraphBuilder : CSharpSyntaxWalker
  {
    private readonly SemanticModel _semanticModel;
    private MethodGraph _graph;

    public MethodGraphBuilder (SemanticModel semanticModel)
    {
      _semanticModel = semanticModel;
      _graph = new MethodGraph();
    }

    public override void VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var symbol = _semanticModel.GetDeclaredSymbol (node);
      _graph.AddMethod(UniqueMethodSymbolNameGenerator.Generate(symbol), node);

      base.VisitMethodDeclaration (node);
    }
  }
}