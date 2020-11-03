using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraph : IMethodGraph
  {
    private readonly Dictionary<string, INode> _members = new Dictionary<string, INode>();
    private readonly Dictionary<string, List<Dependency>> _byFrom = new Dictionary<string, List<Dependency>>();
    private readonly Dictionary<string, List<Dependency>> _byTo = new Dictionary<string, List<Dependency>>();

    public void AddMethod (string methodSymbol, MethodDeclarationSyntax methodDeclarationSyntax)
    {
      var method = new Method (
          methodDeclarationSyntax,
          CreateParentGetter (methodSymbol),
          CreateChildrenGetter (methodSymbol));

      _members[methodSymbol] = method;
    }

    public void AddExternalMethod (string uniqueName, IMethodSymbol methodSymbol)
    {
      var method = new ExternalMethod (methodSymbol, CreateParentGetter (uniqueName));

      _members[uniqueName] = method;
    }

    public void AddField (string uniqueName, FieldDeclarationSyntax fieldDeclarationSyntax)
    {
      var field = new Field (fieldDeclarationSyntax, CreateParentGetter (uniqueName));

      _members[uniqueName] = field;
    }

    public void AddDependency (string fromMethodSymbol, string toMethodSymbol)
    {
      var dependency = new Dependency (
          () =>
          {
            if (_members.TryGetValue (fromMethodSymbol, out var from))
              return from;

            return null!;
          },
          () =>
          {
            if (_members.TryGetValue (toMethodSymbol, out var to))
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

    public INode GetMethod (string methodSymbol)
    {
      return _members[methodSymbol];
    }

    private Func<IReadOnlyCollection<Dependency>> CreateParentGetter (string key)
    {
      return () =>
      {
        if (_byTo.TryGetValue (key, out var parents))
          return parents.Where (p => p.From != null && p.To != null).ToArray();

        return new Dependency[0];
      };
    }

    private Func<IReadOnlyCollection<Dependency>> CreateChildrenGetter (string key)
    {
      return () =>
      {
        if (_byFrom.TryGetValue (key, out var children))
          return children.Where (p => p.From != null && p.To != null).ToArray();

        return new Dependency[0];
      };
    }
  }
}