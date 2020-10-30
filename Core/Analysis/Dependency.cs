using System;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class Dependency
  {
    private readonly Func<IMethod> _from;
    private readonly Func<IMethod> _to;

    public IMethod From => _from();
    public IMethod To => _to();

    public Dependency (Func<IMethod> from, Func<IMethod> to)
    {
      _from = @from;
      _to = to;
    }
  }
}