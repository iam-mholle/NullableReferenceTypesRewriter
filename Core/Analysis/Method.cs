using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Rewriters;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Method : INode, IRewritable
  {
    private readonly string _signature;
    private readonly string _filePath;
    private readonly SharedCompilation _compilation;
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;
    private readonly Func<IReadOnlyCollection<Dependency>> _children;

    public BaseMethodDeclarationSyntax MethodDeclaration => _compilation.GetMethodDeclarationSyntax(_filePath, _signature);
    public SemanticModel SemanticModel => _compilation.GetSemanticModel (_filePath);
    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children => _children();

    public SyntaxNode RewritableSyntaxNode => MethodDeclaration;

    public Method (
        SharedCompilation compilation,
        IMethodSymbol methodSymbol,
        Func<IReadOnlyCollection<Dependency>> parents,
        Func<IReadOnlyCollection<Dependency>> children)
    {
      _filePath = methodSymbol.DeclaringSyntaxReferences.Single().SyntaxTree.FilePath;
      _signature = methodSymbol.ToDisplayStringWithStaticModifier();
      _compilation = compilation;
      _parents = parents;
      _children = children;
    }

    public void Rewrite (RewriterBase rewriter)
    {
      var originalMethodDeclaration = MethodDeclaration;
      var originalTree = originalMethodDeclaration.SyntaxTree;

      var possiblyRewrittenNode = rewriter.Rewrite (this);

      if (originalMethodDeclaration == possiblyRewrittenNode)
        return;

      var newRoot = originalTree.GetRoot().ReplaceNode (originalMethodDeclaration, possiblyRewrittenNode);

      var newTree = originalTree.WithRootAndOptions (newRoot, originalTree.Options);

      _compilation.UpdateSyntaxTree (originalTree, newTree);
    }

    public override string ToString ()
    {
      return _signature;
    }
  }
}