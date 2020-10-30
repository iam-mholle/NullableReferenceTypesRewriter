using System;

namespace NullableReferenceTypesRewriter.Analysis
{
  public interface IMethodGraph
  {
    public IMethod GetMethod (string uniqueMethodName);
  }
}