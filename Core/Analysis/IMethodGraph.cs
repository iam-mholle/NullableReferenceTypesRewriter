using System;

namespace NullableReferenceTypesRewriter.Analysis
{
  public interface IMethodGraph
  {
    public INode GetMethod (string uniqueMethodName);
  }
}