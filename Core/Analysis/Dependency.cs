using System;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Dependency
  {
    private readonly Func<Method> _from;
    private readonly Func<Method> _to;

    public Method From => _from();
    public Method To => _to();

    public Dependency (Func<Method> from, Func<Method> to)
    {
      _from = @from;
      _to = to;
    }
  }
}