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

    public override void VisitProperty(Property property)
    {
      VisitParents(property);
    }

    private void VisitParents (INode node)
    {
      foreach (var parent in node.Parents)
      {
        if (parent.From is Method childMethod)
        {
          if (_visitedMethods.Contains(childMethod))
          {
            continue;
          }
          _visitedMethods.Push (childMethod);
        }

        parent.From.Accept (this);

        if (parent.From is Method)
        {
          _visitedMethods.Pop();
        }
      }
    }
  }
}