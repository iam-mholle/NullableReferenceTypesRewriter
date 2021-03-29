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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class SharedCompilation
  {
    private Compilation _compilation;

    public SharedCompilation (Compilation compilation)
    {
      _compilation = compilation;
    }

    public void WriteChanges ()
    {
      foreach (var syntaxTree in _compilation.SyntaxTrees)
      {
        try
        {
          using var fileStream = new FileStream (syntaxTree.FilePath!, FileMode.Truncate);
          using var writer = new StreamWriter (fileStream, syntaxTree.Encoding);
          syntaxTree.GetRoot()!.WriteTo (writer);
        }
        catch (IOException ex)
        {
          throw new InvalidOperationException ($"Unable to write source file '{syntaxTree.FilePath}'.", ex);
        }
      }
    }

    public void UpdateCompilation ()
    {
      _compilation = CSharpCompilation.Create (
          _compilation.AssemblyName,
          _compilation.SyntaxTrees,
          _compilation.References,
          _compilation.Options as CSharpCompilationOptions);
    }

    public void UpdateSyntaxTree(SyntaxTree old, SyntaxTree @new)
    {
      // Console.WriteLine ($"Updated syntaxtree of {old.FilePath}");
      _compilation = _compilation.ReplaceSyntaxTree(old, @new);
      UpdateCompilation();
    }

    public SemanticModel GetSemanticModel (SyntaxTree tree)
    {
      return _compilation.GetSemanticModel (tree);
    }

    public SemanticModel GetSemanticModel (string filePath)
    {
      var tree = _compilation.SyntaxTrees.Single (t => t.FilePath == filePath);
      return GetSemanticModel (tree);
    }

    public BaseMethodDeclarationSyntax GetMethodDeclarationSyntax (string filePath, string signature)
    {
      // Console.WriteLine($"Querying the MethodDeclarationSyntax of '{signature}' in '{filePath}'.");
      return _compilation.SyntaxTrees
          .Where (t => t.FilePath == filePath)
          .SelectMany (
              t =>
                  t.GetRoot()
                      .DescendantNodes (_ => true)
                      .OfType<BaseMethodDeclarationSyntax>()
                      .Where (n => NullabilityTrimmingEquals(_compilation.GetSemanticModel (t).GetDeclaredSymbol (n)!.ToDisplayStringWithStaticModifier(),signature)))
          .Single();
    }

    public FieldDeclarationSyntax GetVariableDeclarationSyntax (string filePath, string signature)
    {
      return _compilation.SyntaxTrees
          .Where (t => t.FilePath == filePath)
          .SelectMany (
              t =>
                  t.GetRoot()
                      .DescendantNodes (_ => true)
                      .OfType<VariableDeclaratorSyntax>()
                      .Where (n => n.FirstAncestorOrSelf<FieldDeclarationSyntax>() != null)
                      .Where (n => NullabilityTrimmingEquals(_compilation.GetSemanticModel (t).GetDeclaredSymbol (n)!.ToDisplayStringWithStaticModifier(),signature)))
          .Single().FirstAncestorOrSelf<FieldDeclarationSyntax>()!;
    }

    public PropertyDeclarationSyntax GetPropertyDeclarationSyntax(string filePath, string signature)
    {
      // Console.WriteLine($"Querying the MethodDeclarationSyntax of '{signature}' in '{filePath}'.");
      return _compilation.SyntaxTrees
          .Where (t => t.FilePath == filePath)
          .SelectMany (
              t =>
                  t.GetRoot()
                      .DescendantNodes (_ => true)
                      .OfType<PropertyDeclarationSyntax>()
                      .Where (n => NullabilityTrimmingEquals(_compilation.GetSemanticModel (t).GetDeclaredSymbol (n)!.ToDisplayStringWithStaticModifier(),signature)))
          .Single();
    }

    public EventDeclarationSyntax? GetEventDeclarationSyntax(string filePath, string signature)
    {
      return _compilation.SyntaxTrees
          .Where (t => t.FilePath == filePath)
          .SelectMany (
              t =>
                  t.GetRoot()
                      .DescendantNodes (_ => true)
                      .OfType<EventDeclarationSyntax>()
                      .Where (n => NullabilityTrimmingEquals(_compilation.GetSemanticModel (t).GetDeclaredSymbol (n)!.ToDisplayStringWithStaticModifier(),signature)))
          .SingleOrDefault();
    }

    public EventFieldDeclarationSyntax? GetEventFieldDeclarationSyntax(string filePath, string signature)
    {
      return _compilation.SyntaxTrees
          .Where (t => t.FilePath == filePath)
          .SelectMany (
              t =>
                  t.GetRoot()
                      .DescendantNodes (_ => true)
                      .OfType<VariableDeclaratorSyntax>()
                      .Where(n => n.FirstAncestorOrSelf<EventFieldDeclarationSyntax>() != null)
                      .Where (n => NullabilityTrimmingEquals(_compilation.GetSemanticModel (t).GetDeclaredSymbol (n)!.ToDisplayStringWithStaticModifier(),signature)))
          .SingleOrDefault()?.FirstAncestorOrSelf<EventFieldDeclarationSyntax>();
    }

    private static bool NullabilityTrimmingEquals (string a, string b)
    {
      return a.Replace ("?", "") == b.Replace ("?", "");
    }
  }
}