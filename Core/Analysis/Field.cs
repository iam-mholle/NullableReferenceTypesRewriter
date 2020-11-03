using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Field : INode
  {
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;

    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children { get; } = new Dependency[0];
    public FieldDeclarationSyntax FieldDeclarationSyntax { get; }

    public Field (FieldDeclarationSyntax fieldDeclarationSyntax, Func<IReadOnlyCollection<Dependency>> parents)
    {
      FieldDeclarationSyntax = fieldDeclarationSyntax;
      _parents = parents;
    }
  }
}