// Copyright (c) rubicon IT GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

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
