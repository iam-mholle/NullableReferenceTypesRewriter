using System;
using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class ExternalMethod : IMethod
  {
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;

    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children { get; } = new Dependency[0];

    public ExternalMethod (Func<IReadOnlyCollection<Dependency>> parents)
    {
      _parents = parents;
    }
  }
}