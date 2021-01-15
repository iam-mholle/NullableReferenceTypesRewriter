using System;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Dependency
  {
    private readonly Func<INode> _from;
    private readonly Func<INode> _to;

    public INode From => _from();
    public INode To => _to();
    public DependencyType DependencyType { get; }

    public Dependency (Func<INode> from, Func<INode> to, DependencyType dependencyType)
    {
      DependencyType = dependencyType;
      _from = @from;
      _to = to;
    }
  }
}