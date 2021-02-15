using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Rewriters;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Property : INode, IRewritable
  {
    private readonly SharedCompilation _compilation;
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;
    private readonly Func<IReadOnlyCollection<Dependency>> _children;
    private readonly string _filePath;
    private readonly string _signature;
    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children => _children();

    public SyntaxNode RewritableSyntaxNode => PropertyDeclarationSyntax;
    public PropertyDeclarationSyntax PropertyDeclarationSyntax => _compilation.GetPropertyDeclarationSyntax(_filePath, _signature);

    public SemanticModel SemanticModel => _compilation.GetSemanticModel (_filePath);
    public void Rewrite(RewriterBase rewriter)
    {
      var originalFieldDeclaration = PropertyDeclarationSyntax;
      var originalTree = originalFieldDeclaration.SyntaxTree;

      var possiblyRewrittenNode = rewriter.Rewrite (this);

      if (originalFieldDeclaration == possiblyRewrittenNode)
        return;

      var newRoot = originalTree.GetRoot().ReplaceNode (originalFieldDeclaration, possiblyRewrittenNode);

      var newTree = originalTree.WithRootAndOptions (newRoot, originalTree.Options);

      _compilation.UpdateSyntaxTree (originalTree, newTree);
    }

    public Property(SharedCompilation compilation, IPropertySymbol propertySymbol, Func<IReadOnlyCollection<Dependency>> parents, Func<IReadOnlyCollection<Dependency>> children)
    {
      _filePath = propertySymbol.DeclaringSyntaxReferences.Single().SyntaxTree.FilePath;
      _signature = propertySymbol.ToDisplayStringWithStaticModifier();
      _compilation = compilation;
      _parents = parents;
      _children = children;
    }

    public override string ToString ()
    {
      return _signature;
    }
  }
}
