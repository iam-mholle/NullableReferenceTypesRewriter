using System.Linq;
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

    public MethodDeclarationSyntax GetMethodDeclarationSyntax (string filePath, string signature)
    {
      return _compilation.SyntaxTrees
          .Where (t => t.FilePath == filePath)
          .SelectMany (
              t =>
                  t.GetRoot()
                      .DescendantNodes (_ => true)
                      .OfType<MethodDeclarationSyntax>()
                      .Where (n => _compilation.GetSemanticModel (t).GetDeclaredSymbol (n).ToDisplayString() == signature))
          .Single();
    }
  }
}