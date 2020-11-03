using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Field : INode, IRewritable
  {
    private readonly SyntaxReference _fieldSyntaxReference;
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;

    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children { get; } = new Dependency[0];

    public FieldDeclarationSyntax FieldDeclarationSyntax => (FieldDeclarationSyntax) _fieldSyntaxReference.GetSyntax();
    public SyntaxNode RewritableSyntaxNode { get; private set; }

    public Field (FieldDeclarationSyntax fieldDeclarationSyntax, Func<IReadOnlyCollection<Dependency>> parents)
    {
      RewritableSyntaxNode = fieldDeclarationSyntax;
      _fieldSyntaxReference = fieldDeclarationSyntax.GetReference();
      _parents = parents;
    }

    public void Rewrite (CSharpSyntaxRewriter rewriter)
    {
      RewritableSyntaxNode = rewriter.VisitFieldDeclaration ((FieldDeclarationSyntax)RewritableSyntaxNode)
                   ?? throw new InvalidOperationException ($"Could not rewrite {_fieldSyntaxReference.GetSyntax().ToString()}.");
    }

    public void Accept (MemberGraphVisitorBase visitor) => visitor.VisitField (this);
  }
}