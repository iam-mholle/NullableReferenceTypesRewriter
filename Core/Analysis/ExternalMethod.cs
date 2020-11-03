using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class ExternalMethod : INode
  {
    private readonly Func<IReadOnlyCollection<Dependency>> _parents;

    public IMethodSymbol Symbol { get; }
    public IReadOnlyCollection<Dependency> Parents => _parents();
    public IReadOnlyCollection<Dependency> Children { get; } = new Dependency[0];

    public ExternalMethod (IMethodSymbol symbol, Func<IReadOnlyCollection<Dependency>> parents)
    {
      Symbol = symbol;
      _parents = parents;
    }
  }
}