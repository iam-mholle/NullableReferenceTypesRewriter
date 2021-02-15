using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Rewriters;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Event : INode, IRewritable
  {
    private readonly string _signature;
    private readonly string _filePath;
    private readonly SharedCompilation _compilation;
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;

    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children { get; } = new Dependency[0];

    public EventFieldDeclarationSyntax? EventFieldDeclarationSyntax => _compilation.GetEventFieldDeclarationSyntax(_filePath, _signature);
    public EventDeclarationSyntax? EventDeclarationSyntax => _compilation.GetEventDeclarationSyntax(_filePath, _signature);
    public SyntaxNode RewritableSyntaxNode => (SyntaxNode?)EventDeclarationSyntax
                                              ?? EventFieldDeclarationSyntax
                                              ?? throw new InvalidOperationException("Not reachable.");

    public SemanticModel SemanticModel => _compilation.GetSemanticModel(_filePath);

    public Event(SharedCompilation compilation, IEventSymbol eventSymbol, Func<IReadOnlyCollection<Dependency>> parents)
    {
      _filePath = eventSymbol.DeclaringSyntaxReferences.Single().SyntaxTree.FilePath;
      _signature = eventSymbol.ToDisplayStringWithStaticModifier();
      _compilation = compilation;
      _parents = parents;
    }

    public void Rewrite(RewriterBase rewriter)
    {
      var originalEventDeclaration = RewritableSyntaxNode;
      var originalTree = originalEventDeclaration.SyntaxTree;

      var possiblyRewrittenNode = rewriter.Rewrite(this);

      if (originalEventDeclaration == possiblyRewrittenNode)
        return;

      var newRoot = originalTree.GetRoot().ReplaceNode(originalEventDeclaration, possiblyRewrittenNode);

      var newTree = originalTree.WithRootAndOptions(newRoot, originalTree.Options);

      _compilation.UpdateSyntaxTree(originalTree, newTree);
    }

    public override string ToString()
    {
      return _signature;
    }
  }
}
