using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraph : IMethodGraph
  {
    private readonly Dictionary<string, IMethod> _methods = new Dictionary<string, IMethod>();
    private readonly Dictionary<string, List<Dependency>> _byFrom = new Dictionary<string, List<Dependency>>();
    private readonly Dictionary<string, List<Dependency>> _byTo = new Dictionary<string, List<Dependency>>();

    public void AddMethod (string methodSymbol, MethodDeclarationSyntax methodDeclarationSyntax)
    {
      var method = new Method (
          methodDeclarationSyntax,
          () =>
          {
            if (_byTo.TryGetValue (methodSymbol, out var parents))
              return parents.Where(p => p.From != null && p.To != null).ToArray();

            return new Dependency[0];
          },
          () =>
          {
            if (_byFrom.TryGetValue (methodSymbol, out var parents))
              return parents.Where(p => p.From != null && p.To != null).ToArray();

            return new Dependency[0];
          });

      _methods[methodSymbol] = method;
    }

    public void AddExternalMethod (string uniqueName, IMethodSymbol methodSymbol)
    {
      var method = new ExternalMethod (
          methodSymbol,
          () =>
          {
            if (_byTo.TryGetValue (uniqueName, out var parents))
              return parents.Where(p => p.From != null && p.To != null).ToArray();

            return new Dependency[0];
          });

      _methods[uniqueName] = method;
    }

    public void AddDependency (string fromMethodSymbol, string toMethodSymbol)
    {
      var dependency = new Dependency (
          () =>
          {
            if (_methods.TryGetValue (fromMethodSymbol, out var from))
              return from;

            return null!;
          },
          () =>
          {
            if (_methods.TryGetValue (toMethodSymbol, out var to))
              return to;

            return null!;
          });

      if (!_byFrom.ContainsKey(fromMethodSymbol))
        _byFrom[fromMethodSymbol] = new List<Dependency>();

      _byFrom[fromMethodSymbol].Add (dependency);

      if (!_byTo.ContainsKey(toMethodSymbol))
        _byTo[toMethodSymbol] = new List<Dependency>();

      _byTo[toMethodSymbol].Add (dependency);
    }

    public IMethod GetMethod (string methodSymbol)
    {
      return _methods[methodSymbol];
    }
  }
}