using System;
using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public abstract class ChildrenMemberGraphVisitor : MemberGraphVisitorBase
  {
    private readonly Stack<Method> _visitedMethods = new Stack<Method>();

    public override void VisitMethod (Method method)
    {
      if (_visitedMethods.Contains(method))
      {
        return;
      }

      _visitedMethods.Push (method);

      foreach (var child in method.Children)
      {
        child.To.Accept (this);
      }

      _visitedMethods.Pop();
    }

    public override void VisitExternalMethod (ExternalMethod externalMethod)
    {
    }

    public override void VisitField (Field field)
    {
    }
  }
}