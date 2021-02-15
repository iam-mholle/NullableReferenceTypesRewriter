using System;

namespace NullableReferenceTypesRewriter.Analysis
{
  [Flags]
  public enum RewriteCapability
  {
    ParameterChange = 0b01,
    ReturnValueChange = 0b10,
  }
}
