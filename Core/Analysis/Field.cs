using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Rewriters;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Field : INode, IRewritable
  {
    private readonly string _signature;
    private readonly string _filePath;
    private readonly SharedCompilation _compilation;
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;

    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children { get; } = new Dependency[0];

    public FieldDeclarationSyntax FieldDeclarationSyntax => _compilation.GetVariableDeclarationSyntax(_filePath, _signature);
    public SyntaxNode RewritableSyntaxNode => FieldDeclarationSyntax;
    public SemanticModel SemanticModel => _compilation.GetSemanticModel (_filePath);

    public Field (SharedCompilation compilation, IFieldSymbol fieldSymbol, Func<IReadOnlyCollection<Dependency>> parents)
    {
      _filePath = fieldSymbol.DeclaringSyntaxReferences.Single().SyntaxTree.FilePath;
      _signature = fieldSymbol.ToDisplayStringWithStaticModifier();
      _compilation = compilation;
      _parents = parents;
    }

    public void Rewrite (RewriterBase rewriter)
    {
      var originalFieldDeclaration = FieldDeclarationSyntax;
      var originalTree = originalFieldDeclaration.SyntaxTree;

      var possiblyRewrittenNode = rewriter.Rewrite (this);

      if (originalFieldDeclaration == possiblyRewrittenNode)
        return;

      var newRoot = originalTree.GetRoot().ReplaceNode (originalFieldDeclaration, possiblyRewrittenNode);

      var newTree = originalTree.WithRootAndOptions (newRoot, originalTree.Options);

      _compilation.UpdateSyntaxTree (originalTree, newTree);
    }

    public override string ToString ()
    {
      return _signature;
    }
  }
}