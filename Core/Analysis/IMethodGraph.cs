using System;
using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public interface IMethodGraph
  {
    INode GetNode (string uniqueMethodName);
    IReadOnlyCollection<INode> GetNodesWithoutChildren ();
    IReadOnlyCollection<INode> GetNodesWithoutParents ();
  }
}