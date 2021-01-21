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
      var visitor = new TestVisitor((rewriter, collection) => queue.Add((rewriter, collection)));

      foreach (var node in graph.GetNodesWithoutChildren())
      {
        node.Accept (visitor);
      }

      for (var i = 0; i < queue.Count; i++)
      {
        var item = queue[i];

        foreach (var (node, rewriteCapability) in item.Item2)
        {
          if (rewriteCapability == RewriteCapability.ParameterChange)
          {
            node.Rewrite(new DefaultParameterRewriter((b, c) => queue.Add((b, c))));
            node.Rewrite(new CastExpressionRewriter((b, c) => queue.Add((b, c))));
            node.Rewrite(new LocalDeclarationRewriter((b, c) => queue.Add((b, c))));
            node.Rewrite(new MethodArgumentRewriter((b, c) => queue.Add((b, c))));
            node.Rewrite(new InheritanceParameterRewriter((b, c) => queue.Add((b, c))));
          }
          else if (rewriteCapability == RewriteCapability.ReturnValueChange)
          {
            node.Rewrite(new InheritanceReturnRewriter((b, c) => queue.Add((b, c))));
            node.Rewrite(new LocalDeclarationRewriter((b, c) => queue.Add((b, c))));
            node.Rewrite(new PropertyNullReturnRewriter((b, c) => queue.Add((b, c))));
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