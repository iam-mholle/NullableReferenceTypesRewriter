using System;

namespace NullableReferenceTypesRewriter.Analysis
{
  [Flags]
  public enum RewriteCapability
  {
    ParameterChange = 1^2,
    ReturnValueChange = 2^2,
  }
}
