using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraph
  {
    private readonly Dictionary<string, Method> _methods = new Dictionary<string, Method>();
    private readonly Dictionary<string, List<Dependency>> _byFrom = new Dictionary<string, List<Dependency>>();
    private readonly Dictionary<string, List<Dependency>> _byTo = new Dictionary<string, List<Dependency>>();

    public void AddMethod (string uniqueSignature)
    {
      var method = new Method (
          () =>
          {
            if (_byTo.TryGetValue (uniqueSignature, out var parents))
              return parents.AsReadOnly();

            return new Dependency[0];
          },
          () =>
          {
            if (_byFrom.TryGetValue (uniqueSignature, out var parents))
              return parents.AsReadOnly();

            return new Dependency[0];
          });

      _methods[uniqueSignature] = method;
    }

    public void AddDependency (string fromUniqueSignature, string toUniqueSignature)
    {
      var dependency = new Dependency (
          () => _methods[fromUniqueSignature],
          () => _methods[toUniqueSignature]);

      if (!_byFrom.ContainsKey(fromUniqueSignature))
        _byFrom[fromUniqueSignature] = new List<Dependency>();

      _byFrom[fromUniqueSignature].Add (dependency);

      if (!_byTo.ContainsKey(toUniqueSignature))
        _byTo[toUniqueSignature] = new List<Dependency>();

      _byTo[toUniqueSignature].Add (dependency);
    }

    public Method GetMethod (string uniqueSignature)
    {
      return _methods[uniqueSignature];
    }
  }
}