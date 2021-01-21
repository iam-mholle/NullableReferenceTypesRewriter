using NullableReferenceTypesRewriter.Rewriters;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Rewriters
{
  [TestFixture]
  public class PropertyNullReturnRewriterTest : RewriterTestBase<PropertyNullReturnRewriter>
  {
    [Test]
    public void ExpressionBodied_NullReturning_Nullable()
      => SimpleRewriteAssertion(
          /*language=C#*/ @"public string? Test => null;",
          /*language=C#*/ @"public string Test => null;",
          WrapperType.Property);

    [Test]
    public void Getter_NullReturning_Nullable()
      => SimpleRewriteAssertion(
          /*language=C#*/ @"public string? Test { get { return null; } }",
          /*language=C#*/ @"public string Test { get { return null; } }",
          WrapperType.Property);

    [Test]
    public void ExpressionBodied_NonNullReturning_Unchanged()
      => SimpleUnchangedAssertion(
          /*language=C#*/ @"public string Test => ""some string"";",
          WrapperType.Property);

    [Test]
    public void Getter_NonNullReturning_Unchanged()
      => SimpleUnchangedAssertion(
          /*language=C#*/ @"public string Test { get { return ""some string""; } }",
          WrapperType.Property);
  }
}
