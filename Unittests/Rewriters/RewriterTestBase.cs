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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Rewriters;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Rewriters
{
  public abstract class RewriterTestBase<TRewriter>
      where TRewriter : RewriterBase
  {
    protected enum WrapperType
    {
      Method,
      Property,
      Field,
      Event,
      EventField,
    }

    protected enum CompileIn
    {
      Class,
      Namespace,
    }

    protected void SimpleRewriteAssertion(string expected, string input, WrapperType wrapperType, CompileIn compileIn = CompileIn.Class, Func<IReadOnlyCollection<(IRewritable, RewriteCapability)>, bool>? deferredRewritesPredicate = null)
    {
      deferredRewritesPredicate ??= c => c.Count == 0;

          var (semantic, root) = compileIn switch
      {
          CompileIn.Class => CompiledSourceFileProvider.CompileInClass ("A", input),
          CompileIn.Namespace => CompiledSourceFileProvider.CompileInNameSpace("A", input),
          _ => throw new ArgumentOutOfRangeException(),
      };

      INode node = wrapperType switch
      {
          WrapperType.Method => CreateMethodWrapper((BaseMethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration) || n.IsKind (SyntaxKind.ConstructorDeclaration)), semantic),
          WrapperType.Field => CreateFieldWrapper((FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration)), semantic),
          WrapperType.Property => CreatePropertyWrapper((PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration)), semantic),
          WrapperType.Event => CreateEventWrapper((EventDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.EventDeclaration)), semantic),
          WrapperType.EventField => CreateEventFieldWrapper((EventFieldDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.EventFieldDeclaration)), semantic),
          _ => throw new ArgumentOutOfRangeException(),
      };

      var deferredRewrites = new List<(IRewritable, RewriteCapability)>();

      var sut = (RewriterBase) Activator.CreateInstance(typeof(TRewriter), (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>>) ((b, c) => deferredRewrites.AddRange(c)));
      var result = wrapperType switch
      {
          WrapperType.Method => sut.Rewrite((Method) node),
          WrapperType.Field => sut.Rewrite((Field) node),
          WrapperType.Property => sut.Rewrite((Property) node),
          WrapperType.Event => sut.Rewrite((Event) node),
          WrapperType.EventField => sut.Rewrite((Event) node),
          _ => throw new ArgumentOutOfRangeException(),
      };

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
      Assert.That(deferredRewritesPredicate(deferredRewrites), Is.True);
    }

    protected void SimpleUnchangedAssertion(string input, WrapperType wrapperType, CompileIn compileIn = CompileIn.Class)
    {
      var (semantic, root) = compileIn switch
      {
          CompileIn.Class => CompiledSourceFileProvider.CompileInClass ("A", input),
          CompileIn.Namespace => CompiledSourceFileProvider.CompileInNameSpace("A", input),
          _ => throw new ArgumentOutOfRangeException(),
      };

      INode node = wrapperType switch
      {
          WrapperType.Method => CreateMethodWrapper((BaseMethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration) || n.IsKind (SyntaxKind.ConstructorDeclaration)), semantic),
          WrapperType.Field => CreateFieldWrapper((FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration)), semantic),
          WrapperType.Property => CreatePropertyWrapper((PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration)), semantic),
          WrapperType.Event => CreateEventWrapper((EventDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.EventDeclaration)), semantic),
          WrapperType.EventField => CreateEventFieldWrapper((EventFieldDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.EventFieldDeclaration)), semantic),
          _ => throw new ArgumentOutOfRangeException(),
      };

      var sut = (RewriterBase) Activator.CreateInstance(typeof(TRewriter), (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>>) ((b, c) => {}));
      var result = wrapperType switch
      {
          WrapperType.Method => sut.Rewrite((Method) node),
          WrapperType.Field => sut.Rewrite((Field) node),
          WrapperType.Property => sut.Rewrite((Property) node),
          WrapperType.Event => sut.Rewrite((Event) node),
          WrapperType.EventField => sut.Rewrite((Event) node),
          _ => throw new ArgumentOutOfRangeException(),
      };

      Assert.That (result, Is.SameAs(wrapperType switch
          {
              WrapperType.Method => (CSharpSyntaxNode)((Method) node).MethodDeclaration,
              WrapperType.Field => (CSharpSyntaxNode)((Field) node).FieldDeclarationSyntax,
              WrapperType.Property => (CSharpSyntaxNode)((Property) node).PropertyDeclarationSyntax,
              WrapperType.Event => (CSharpSyntaxNode)((Event) node).EventDeclarationSyntax!,
              WrapperType.EventField => (CSharpSyntaxNode)((Event) node).EventFieldDeclarationSyntax!,
              _ => throw new ArgumentOutOfRangeException(),
          }));
    }

    protected Method CreateMethodWrapper (
        BaseMethodDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null,
        Func<IReadOnlyCollection<Dependency>>? children = null)
    {
      return new Method(
          new SharedCompilation(semanticModel.Compilation),
          (IMethodSymbol) ModelExtensions.GetDeclaredSymbol(semanticModel, syntax)!,
          parents ?? Array.Empty<Dependency>,
          children ?? Array.Empty<Dependency>);
    }

    protected Property CreatePropertyWrapper (
        PropertyDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null,
        Func<IReadOnlyCollection<Dependency>>? children = null)
    {
      return new Property(
          new SharedCompilation(semanticModel.Compilation),
          (IPropertySymbol) ModelExtensions.GetDeclaredSymbol(semanticModel, syntax)!,
          parents ?? Array.Empty<Dependency>,
          children ?? Array.Empty<Dependency>);
    }

    protected Field CreateFieldWrapper (
        FieldDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null)
    {
      return new Field(
          new SharedCompilation(semanticModel.Compilation),
          (IFieldSymbol) ModelExtensions.GetDeclaredSymbol(semanticModel, syntax.Declaration.Variables.First())!,
          parents ?? Array.Empty<Dependency>);
    }

    protected Event CreateEventFieldWrapper (
        EventFieldDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null)
    {
      return new Event(
          new SharedCompilation(semanticModel.Compilation),
          (IEventSymbol) ModelExtensions.GetDeclaredSymbol(semanticModel, syntax.Declaration.Variables.First())!,
          parents ?? Array.Empty<Dependency>);
    }

    protected Event CreateEventWrapper (
        EventDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null)
    {
      return new Event(
          new SharedCompilation(semanticModel.Compilation),
          (IEventSymbol) ModelExtensions.GetDeclaredSymbol(semanticModel, syntax)!,
          parents ?? Array.Empty<Dependency>);
    }
  }
}
