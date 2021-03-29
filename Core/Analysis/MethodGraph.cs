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

    public void AddEvent(string uniqueName, IEventSymbol symbol)
    {
      var @event = new Event(_compilation, symbol, CreateParentGetter(uniqueName));

      _members[uniqueName] = @event;
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