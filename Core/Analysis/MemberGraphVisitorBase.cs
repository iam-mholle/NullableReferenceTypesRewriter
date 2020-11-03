namespace NullableReferenceTypesRewriter.Analysis
{
  public abstract class MemberGraphVisitorBase
  {
    public abstract void VisitMethod (Method method);
    public abstract void VisitExternalMethod (ExternalMethod externalMethod);
    public abstract void VisitField (Field field);
  }
}