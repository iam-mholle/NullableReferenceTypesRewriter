using System.Linq;
using Microsoft.CodeAnalysis;
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
      var compilation = CompiledSourceFileProvider.CompileInNameSpace ("Test", @"
public class A
{
  public object DoStuff()
  {
    return new B().DoMore();
  }
}
public class B
{
  public object DoMore()
  {
    return new object();
  }
}
");
      var builder = new MethodGraphBuilder(new SharedCompilation(compilation.Item1.Compilation));

      // builder.SetSemanticModel (compilation.Item1);
      builder.Visit (compilation.Item2);
      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var methodOfB = builder.Graph.GetNode ("Test.B.DoMore()");

      Assert.That (methodOfA, Is.Not.Null);
      Assert.That (methodOfB, Is.Not.Null);
      Assert.That (methodOfA.Children, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Parents, Has.Length.EqualTo (0));
      Assert.That (methodOfB.Children, Has.Length.EqualTo (0));
      Assert.That (methodOfB.Parents, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Children.First().To, Is.SameAs(methodOfB));
      Assert.That (methodOfB.Parents.First().From, Is.SameAs(methodOfA));
    }

    [Test]
    public void Test_1 ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace ("Test", @"
public class A
{
  private string _field = string.Empty;

  public string DoStuff()
  {
    return _field;
  }
}
");
      var builder = new MethodGraphBuilder(new SharedCompilation(compilation.Item1.Compilation));

      // builder.SetSemanticModel (compilation.Item1);
      builder.Visit (compilation.Item2);
      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var r = (methodOfA.Children.Single().To as Field)!.FieldDeclarationSyntax;
    }
  }
}