using System;

namespace NullableReferenceTypesRewriter.Analysis
{
  public abstract class ParentMemberGraphVisitor : MemberGraphVisitorBase
  {
    public override void VisitMethod (Method method)
    {
      VisitParents (method);
    }

    public override void VisitExternalMethod (ExternalMethod externalMethod)
    {
      VisitParents (externalMethod);
    }

    public override void VisitField (Field field)
    {
      VisitParents (field);
    }

    private void VisitParents (INode node)
    {
      foreach (var parent in node.Parents)
      {
        parent.From.Accept (this);
      }
    }
  }
}