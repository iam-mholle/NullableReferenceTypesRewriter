using System;
using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public abstract class ParentMemberGraphVisitor : MemberGraphVisitorBase
  {
    private readonly Stack<Method> _visitedMethods = new Stack<Method>();

    public override void VisitMethod (Method method)
    {
      if (_visitedMethods.Contains(method))
      {
        return;
      }

      _visitedMethods.Push(method);

      VisitParents (method);

      _visitedMethods.Pop();
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