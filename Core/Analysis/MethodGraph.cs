using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraph : IMethodGraph
  {
    private readonly SharedCompilation _compilation;
    private readonly Dictionary<string, INode> _members = new Dictionary<string, INode>();
    private readonly Dictionary<string, List<Dependency>> _byFrom = new Dictionary<string, List<Dependency>>();
    private readonly Dictionary<string, List<Dependency>> _byTo = new Dictionary<string, List<Dependency>>();

    public MethodGraph (SharedCompilation compilation)
    {
      _compilation = compilation;
    }

    public void AddMethod (string uniqueName, IMethodSymbol methodSymbol)
    {
      var method = new Method (
          _compilation,
          methodSymbol,
          CreateParentGetter (uniqueName),
          CreateChildrenGetter (uniqueName));

      _members[uniqueName] = method;
    }

    public void AddExternalMethod (string uniqueName, IMethodSymbol methodSymbol)
    {
      var method = new ExternalMethod (methodSymbol, CreateParentGetter (uniqueName));

      _members[uniqueName] = method;
    }

    public void AddField (string uniqueName, IFieldSymbol fieldSymbol)
    {
      var field = new Field (_compilation, fieldSymbol, CreateParentGetter (uniqueName));

      _members[uniqueName] = field;
    }

    public void AddProperty(string uniqueName, IPropertySymbol symbol)
    {
      var property = new Property(_compilation, symbol, CreateParentGetter(uniqueName), CreateChildrenGetter(uniqueName));

      _members[uniqueName] = property;
    }

    public void AddDependency (string fromMethodSymbol, string toMethodSymbol, DependencyType dependencyType)
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
          },
          dependencyType);

      if (!_byFrom.ContainsKey(fromMethodSymbol))
        _byFrom[fromMethodSymbol] = new List<Dependency>();

      _byFrom[fromMethodSymbol].Add (dependency);

      if (!_byTo.ContainsKey(toMethodSymbol))
        _byTo[toMethodSymbol] = new List<Dependency>();

      _byTo[toMethodSymbol].Add (dependency);
    }

    public INode GetNode (string uniqueName)
    {
      return _members[uniqueName];
    }

    public IReadOnlyCollection<INode> GetNodesWithoutChildren ()
    {
      return _members.Values.Where (n => n.Children.Count == 0).ToArray();
    }

    public IReadOnlyCollection<INode> GetNodesWithoutParents ()
    {
      return _members.Values.Where (n => n.Parents.Count == 0).ToArray();
    }

    public void ForEachNode(Action<IRewritable> action, Func<INode, bool>? predicate = null)
    {
      predicate ??= _ => true;

      foreach (var member in _members.Select(m => m.Value).OfType<IRewritable>())
      {
        if (predicate((INode) member))
        {
          action(member);
        }
      }
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