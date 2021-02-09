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
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Rewriters;

namespace NullableReferenceTypesRewriter.ConsoleApplication
{
  internal class Program
  {
    public static async Task Main (string[] args)
    {
      if (args.Length != 2)
      {
        Console.WriteLine ("Please supply a solution directory and a project name to convert.");
        Console.WriteLine ("eg.:     nrtRewriter \"C:\\Develop\\MyCode\\MyCode.sln\" Core.Utils");
      }

      var solutionPath = args[0];
      var projectName = args[1];

      var solution = await LoadSolutionSpace (solutionPath);
      var project = LoadProject (solution, projectName);
      var compilation = project.GetCompilationAsync().GetAwaiter().GetResult();
      var sharedCompilation = new SharedCompilation (compilation!);

      var graphBuilder = new MethodGraphBuilder(sharedCompilation);

      foreach (var document in project.Documents)
      {
        var syntax = await document.GetSyntaxRootAsync()
                     ?? throw new ArgumentException ($"Document '{document.FilePath}' does not support providing a syntax tree.");

        graphBuilder.Visit (syntax);
      }

      var graph = graphBuilder.Graph;
      var queue = new List<(RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>)>();

      Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites = (b, c) => queue.Add((b, c));

      var nullReturnRewriter = new NullReturnRewriter(additionalRewrites);
      var castExpressionRewriter = new CastExpressionRewriter (additionalRewrites);
      var localDeclarationRewriter = new LocalDeclarationRewriter (additionalRewrites);
      var methodArgumentRewriter = new MethodArgumentRewriter (additionalRewrites);
      var uninitializedFieldRewriter = new UninitializedFieldRewriter (additionalRewrites);
      var inheritanceParameterRewriter = new InheritanceParameterRewriter (additionalRewrites);
      var inheritanceReturnRewriter = new InheritanceReturnRewriter (additionalRewrites);
      var defaultParameterRewriter = new DefaultParameterRewriter (additionalRewrites);
      var uninitializedPropertyRewriter = new UninitializedPropertyRewriter(additionalRewrites);
      var propertyNullReturnRewriter = new PropertyNullReturnRewriter(additionalRewrites);
      var inheritancePropertyRewriter = new InheritancePropertyRewriter(additionalRewrites);

      graph.ForEachNode(n =>
      {
        n.Rewrite(nullReturnRewriter);
        n.Rewrite(castExpressionRewriter);
        n.Rewrite(localDeclarationRewriter);
        n.Rewrite(methodArgumentRewriter);
        n.Rewrite(uninitializedFieldRewriter);
        n.Rewrite(inheritanceParameterRewriter);
        n.Rewrite(inheritanceReturnRewriter);
        n.Rewrite(defaultParameterRewriter);
        n.Rewrite(uninitializedPropertyRewriter);
        n.Rewrite(propertyNullReturnRewriter);
        n.Rewrite(inheritancePropertyRewriter);
      }, n => n.GetType() != typeof(ExternalMethod));

      for (var i = 0; i < queue.Count; i++)
      {
        var item = queue[i];

        foreach (var (node, rewriteCapability) in item.Item2)
        {
          if (rewriteCapability == RewriteCapability.ParameterChange)
          {
            node.Rewrite(defaultParameterRewriter);
            node.Rewrite(castExpressionRewriter);
            node.Rewrite(localDeclarationRewriter);
            node.Rewrite(methodArgumentRewriter);
            node.Rewrite(inheritanceParameterRewriter);
          }
          else if (rewriteCapability == RewriteCapability.ReturnValueChange)
          {
            node.Rewrite(inheritanceReturnRewriter);
            node.Rewrite(localDeclarationRewriter);
            node.Rewrite(propertyNullReturnRewriter);
            node.Rewrite(inheritancePropertyRewriter);
          }
        }
      }

      sharedCompilation.WriteChanges();
    }

    private static Project LoadProject (Solution solution, string projectName)
    {
      var project = solution.Projects.Single (p => p.Name == projectName);
      var compilationOptions = project.CompilationOptions as CSharpCompilationOptions;

      compilationOptions = compilationOptions?.WithNullableContextOptions (NullableContextOptions.Enable);
      if (compilationOptions != null)
        project = project.WithCompilationOptions (compilationOptions);

      return project;
    }

    private static Task<Solution> LoadSolutionSpace (string solutionPath)
    {
      var msBuild = MSBuildLocator.QueryVisualStudioInstances().First();
      MSBuildLocator.RegisterInstance (msBuild);
      using var workspace = MSBuildWorkspace.Create();
      workspace.WorkspaceFailed += (sender, args) => { Console.WriteLine (args.Diagnostic); };

      return workspace.OpenSolutionAsync (solutionPath);
    }
  }
}