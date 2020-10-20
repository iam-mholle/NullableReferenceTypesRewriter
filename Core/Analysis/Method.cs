using System;
using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Method
  {
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;
    private readonly Func<IReadOnlyCollection<Dependency>> _children;

    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children => _children();

    public Method (Func<IReadOnlyCollection<Dependency>> parents, Func<IReadOnlyCollection<Dependency>> children)
    {
      _parents = parents;
      _children = children;
    }
  }
}