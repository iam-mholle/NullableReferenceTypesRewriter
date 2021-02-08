using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Rewriters;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Rewriters
{
  [TestFixture]
  public class InheritancePropertyRewriterTest : RewriterTestBase<InheritancePropertyRewriter>
  {
    [Test]
    public void Override_Nullable_Nullable()
    {
      //language=C#
      const string expected = @"
  public virtual string? SomeProperty { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeBase
{
  public virtual string SomeProperty { get; set; }
}
public class SomeDerived : SomeBase
{
  public override string? SomeProperty { get; set; }
}
");
      Property property = null!;
      var derivedSyntax = (PropertyDeclarationSyntax) root.DescendantNodes().Last(n => n.IsKind(SyntaxKind.PropertyDeclaration));
      var derivedProperty = CreatePropertyWrapper(derivedSyntax, semantic);
      var baseSyntax = (PropertyDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.PropertyDeclaration));
      var dependency = new Dependency(() => property, () => derivedProperty, DependencyType.Inheritance);
      property = CreatePropertyWrapper(baseSyntax, semantic, null, () => new[] { dependency });
      var sut = new InheritancePropertyRewriter((b, c) => { });

      var result = sut.Rewrite(property);

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
    }

    [Test]
    public void Override_NonNullable_Unchanged()
    {
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeBase
{
  public virtual string SomeProperty { get; set; }
}
public class SomeDerived : SomeBase
{
  public override string SomeProperty { get; set; }
}
");
      Property property = null!;
      var derivedSyntax = (PropertyDeclarationSyntax) root.DescendantNodes().Last(n => n.IsKind(SyntaxKind.PropertyDeclaration));
      var derivedProperty = CreatePropertyWrapper(derivedSyntax, semantic);
      var baseSyntax = (PropertyDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.PropertyDeclaration));
      var dependency = new Dependency(() => property, () => derivedProperty, DependencyType.Inheritance);
      property = CreatePropertyWrapper(baseSyntax, semantic, null, () => new[] { dependency });
      var sut = new InheritancePropertyRewriter((b, c) => { });

      var result = sut.Rewrite(property);

      Assert.That(result, Is.SameAs(baseSyntax));
    }

    [Test]
    public void MultipleOverrides_OneNullable_Nullable()
    {
      //language=C#
      const string expected = @"
  public virtual string? SomeProperty { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeBase
{
  public virtual string SomeProperty { get; set; }
}
public class SomeDerived1 : SomeBase
{
  public override string? SomeProperty { get; set; }
}
public class SomeDerived2 : SomeBase
{
  public override string SomeProperty { get; set; }
}
");
      Property property = null!;
      var derivedSyntax1 = (PropertyDeclarationSyntax) root.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ElementAt(1);
      var derivedProperty1 = CreatePropertyWrapper(derivedSyntax1, semantic);
      var dependency1 = new Dependency(() => property, () => derivedProperty1, DependencyType.Inheritance);
      var derivedSyntax2 = (PropertyDeclarationSyntax) root.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ElementAt(2);
      var derivedProperty2 = CreatePropertyWrapper(derivedSyntax2, semantic);
      var dependency2 = new Dependency(() => property, () => derivedProperty2, DependencyType.Inheritance);
      var baseSyntax = (PropertyDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.PropertyDeclaration));
      property = CreatePropertyWrapper(baseSyntax, semantic, null, () => new[] { dependency1, dependency2 });
      var sut = new InheritancePropertyRewriter((b, c) => { });

      var result = sut.Rewrite(property);

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
    }
  }
}
