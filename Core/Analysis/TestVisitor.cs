using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class TestVisitor : ParentMemberGraphVisitor
  {
    private readonly RewriterBase _nullReturnRewriter;
    private readonly RewriterBase _castExpressionRewriter;
    private readonly RewriterBase _localDeclarationRewriter;
    private readonly RewriterBase _methodArgumentRewriter;
    private readonly RewriterBase _uninitializedFieldRewriter;
    private readonly RewriterBase _inheritanceParameterRewriter;
    private readonly RewriterBase _inheritanceReturnRewriter;
    private readonly RewriterBase _defaultParameterRewriter;

    public TestVisitor (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
    {
      _nullReturnRewriter = new NullReturnRewriter(additionalRewrites);
      _castExpressionRewriter = new CastExpressionRewriter (additionalRewrites);
      _localDeclarationRewriter = new LocalDeclarationRewriter (additionalRewrites);
      _methodArgumentRewriter = new MethodArgumentRewriter (additionalRewrites);
      _uninitializedFieldRewriter = new UninitializedFieldRewriter (additionalRewrites);
      _inheritanceParameterRewriter = new InheritanceParameterRewriter (additionalRewrites);
      _inheritanceReturnRewriter = new InheritanceReturnRewriter (additionalRewrites);
      _defaultParameterRewriter = new DefaultParameterRewriter (additionalRewrites);
    }

    public override void VisitMethod (Method method)
    {
      // Console.WriteLine ("method: " + method.MethodDeclaration.ToString());
      method.Rewrite (_nullReturnRewriter);
      method.Rewrite (_castExpressionRewriter);
      method.Rewrite (_localDeclarationRewriter);
      method.Rewrite (_methodArgumentRewriter);
      method.Rewrite (_inheritanceParameterRewriter);
      method.Rewrite (_inheritanceReturnRewriter);
      method.Rewrite (_defaultParameterRewriter);
      base.VisitMethod (method);
    }

    public override void VisitField (Field field)
    {
      // Console.WriteLine ("field: " + field.FieldDeclarationSyntax.ToString());
      field.Rewrite (_uninitializedFieldRewriter);
      base.VisitField (field);
    }

    public override void VisitExternalMethod (ExternalMethod externalMethod)
    {
      // Console.WriteLine ("external: " + externalMethod.Symbol.ToString());
      base.VisitExternalMethod (externalMethod);
    }
  }
}