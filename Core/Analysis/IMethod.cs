using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public interface IMethod
  {
    public IReadOnlyCollection<Dependency> Parents { get; }
    public IReadOnlyCollection<Dependency> Children { get; }
  }
}