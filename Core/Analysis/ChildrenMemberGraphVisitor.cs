using System;

namespace NullableReferenceTypesRewriter.Analysis
{
  public abstract class ChildrenMemberGraphVisitor : MemberGraphVisitorBase
  {
    public override void VisitMethod (Method method)
    {
      foreach (var child in method.Children)
      {
        child.To.Accept (this);
      }
    }

    public override void VisitExternalMethod (ExternalMethod externalMethod)
    {
    }

    public override void VisitField (Field field)
    {
    }
  }
}