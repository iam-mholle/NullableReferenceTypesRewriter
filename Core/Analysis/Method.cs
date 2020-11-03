using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Method : INode, IRewritable
  {
    private readonly SyntaxReference _methodSyntaxReference;
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;
    private readonly Func<IReadOnlyCollection<Dependency>> _children;

    public MethodDeclarationSyntax MethodDeclaration => (MethodDeclarationSyntax) _methodSyntaxReference.GetSyntax();
    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children => _children();
    public SyntaxNode RewritableSyntaxNode { get; private set; }

    public Method (MethodDeclarationSyntax methodDeclaration, Func<IReadOnlyCollection<Dependency>> parents, Func<IReadOnlyCollection<Dependency>> children)
    {
      RewritableSyntaxNode = methodDeclaration;
      _methodSyntaxReference = methodDeclaration.GetReference();
      _parents = parents;
      _children = children;
    }

    public void Rewrite (CSharpSyntaxRewriter rewriter)
    {
      RewritableSyntaxNode = (MethodDeclarationSyntax) (rewriter.VisitMethodDeclaration ((MethodDeclarationSyntax) RewritableSyntaxNode)
                                              ?? throw new InvalidOperationException ($"Could not rewrite {_methodSyntaxReference.GetSyntax()}."));
    }
  }
}