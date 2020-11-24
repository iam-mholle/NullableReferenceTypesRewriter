using System;
using System.Collections.Generic;

namespace NullableReferenceTypesRewriter.Analysis
{
  public abstract class ChildrenMemberGraphVisitor : MemberGraphVisitorBase
  {
    private readonly Stack<Method> _visitedMethods = new Stack<Method>();

    public override void VisitMethod (Method method)
    {
      foreach (var child in method.Children)
      {
        if (child.To is Method childMethod)
        {
          if (_visitedMethods.Contains(childMethod))
          {
            continue;
          }
          _visitedMethods.Push (childMethod);
        }

        child.To.Accept (this);

        if (child.To is Method)
        {
          _visitedMethods.Pop();
        }
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