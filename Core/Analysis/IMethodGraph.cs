using System;
using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public interface IMethodGraph
  {
    INode GetNode (string uniqueMethodName);
    IReadOnlyCollection<INode> GetNodesWithoutChildren ();
    IReadOnlyCollection<INode> GetNodesWithoutParents ();
    void ForEachNode(Action<IRewritable> action, Func<INode, bool>? predicate = null);
  }
}