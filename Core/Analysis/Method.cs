using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Method
  {
    private readonly SyntaxReference _methodSyntaxReference;
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;
    private readonly Func<IReadOnlyCollection<Dependency>> _children;

    public MethodDeclarationSyntax MethodDeclaration => (MethodDeclarationSyntax) _methodSyntaxReference.GetSyntax();
    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children => _children();

    public Method (MethodDeclarationSyntax methodDeclaration, Func<IReadOnlyCollection<Dependency>> parents, Func<IReadOnlyCollection<Dependency>> children)
    {
      _methodSyntaxReference = methodDeclaration.GetReference();
      _parents = parents;
      _children = children;
    }
  }
}