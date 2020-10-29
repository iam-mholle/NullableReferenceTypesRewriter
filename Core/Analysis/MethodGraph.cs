using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraph
  {
    private readonly Dictionary<string, Method> _methods = new Dictionary<string, Method>();
    private readonly Dictionary<string, List<Dependency>> _byFrom = new Dictionary<string, List<Dependency>>();
    private readonly Dictionary<string, List<Dependency>> _byTo = new Dictionary<string, List<Dependency>>();

    public void AddMethod (string methodSymbol, MethodDeclarationSyntax methodDeclarationSyntax)
    {
      var method = new Method (
          methodDeclarationSyntax,
          () =>
          {
            if (_byTo.TryGetValue (methodSymbol, out var parents))
              return parents.AsReadOnly();

            return new Dependency[0];
          },
          () =>
          {
            if (_byFrom.TryGetValue (methodSymbol, out var parents))
              return parents.AsReadOnly();

            return new Dependency[0];
          });

      _methods[methodSymbol] = method;
    }

    public void AddDependency (string fromMethodSymbol, string toMethodSymbol)
    {
      var dependency = new Dependency (
          () => _methods[fromMethodSymbol],
          () => _methods[toMethodSymbol]);

      if (!_byFrom.ContainsKey(fromMethodSymbol))
        _byFrom[fromMethodSymbol] = new List<Dependency>();

      _byFrom[fromMethodSymbol].Add (dependency);

      if (!_byTo.ContainsKey(toMethodSymbol))
        _byTo[toMethodSymbol] = new List<Dependency>();

      _byTo[toMethodSymbol].Add (dependency);
    }

    public Method GetMethod (string methodSymbol)
    {
      return _methods[methodSymbol];
    }
  }
}