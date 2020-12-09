using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class TestVisitor : ParentMemberGraphVisitor
  {
    private RewriterBase _nullReturnRewriter;

    public TestVisitor (Action<RewriterBase, IReadOnlyCollection<Dependency>> additionalRewrites)
    {
      _nullReturnRewriter = new NullReturnRewriter(additionalRewrites);
    }

    public override void VisitMethod (Method method)
    {
      // Console.WriteLine ("method: " + method.MethodDeclaration.ToString());
      method.Rewrite (_nullReturnRewriter);
      base.VisitMethod (method);
    }

    public override void VisitField (Field field)
    {
      // Console.WriteLine ("field: " + field.FieldDeclarationSyntax.ToString());
      base.VisitField (field);
    }

    public override void VisitExternalMethod (ExternalMethod externalMethod)
    {
      // Console.WriteLine ("external: " + externalMethod.Symbol.ToString());
      base.VisitExternalMethod (externalMethod);
    }
  }
}