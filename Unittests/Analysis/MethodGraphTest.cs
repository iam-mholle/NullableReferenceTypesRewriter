using NullableReferenceTypesRewriter.Analysis;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Analysis
{
  [TestFixture]
  public class MethodGraphTest
  {
    [Test]
    public void Test ()
    {
      var mg = new MethodGraph();
      mg.AddMethod ("A");
      mg.AddMethod ("B");
      mg.AddMethod ("C");
      mg.AddMethod ("D");
      mg.AddDependency("A", "D");
      mg.AddDependency("A", "B");
      mg.AddDependency("A", "C");
      mg.AddDependency("B", "C");
    }
  }
}