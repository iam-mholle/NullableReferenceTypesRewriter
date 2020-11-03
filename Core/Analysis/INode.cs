using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public interface INode
  {
    public IReadOnlyCollection<Dependency> Parents { get; }
    public IReadOnlyCollection<Dependency> Children { get; }
    public void Accept (MemberGraphVisitorBase visitor);
  }
}