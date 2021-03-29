// Copyright (c) rubicon IT GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Rewriters;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Analysis
{
  [TestFixture]
  public class MethodGraphTest
  {
    [Test]
    public void MethodDependenciesBetweenDifferentClasses ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
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
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var methodOfB = builder.Graph.GetNode ("Test.B.DoMore()");
      var ctorOfB = builder.Graph.GetNode ("Test.B.B()");

      Assert.That (methodOfA, Is.Not.Null);
      Assert.That (methodOfB, Is.Not.Null);
      Assert.That (methodOfA.Children, Has.Length.EqualTo (2));
      Assert.That (methodOfA.Parents, Has.Length.EqualTo (0));
      Assert.That (methodOfB.Children, Has.Length.EqualTo (1));
      Assert.That (methodOfB.Parents, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Children.First().To, Is.SameAs (methodOfB));
      Assert.That (methodOfA.Children.Last().To, Is.SameAs (ctorOfB));
      Assert.That (methodOfB.Parents.First().From, Is.SameAs (methodOfA));
      Assert.That (methodOfA.ToString(), Is.EqualTo ("Test.A.DoStuff()"));
    }

    [Test]
    public void FieldDependencyInSameClass ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public class A
{
  private string _field = string.Empty;

  public string DoStuff()
  {
    return _field;
  }
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var fieldOfA = builder.Graph.GetNode ("Test.A._field");
      Assert.That (methodOfA, Is.Not.Null);
      Assert.That (methodOfA.Children, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Parents, Has.Length.EqualTo (0));
      Assert.That (fieldOfA.Children, Has.Length.EqualTo (0));
      Assert.That (fieldOfA.Parents, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Children.First().To, Is.SameAs (fieldOfA));
      Assert.That (fieldOfA.ToString(), Is.EqualTo("Test.A._field"));
    }

    [Test]
    public void ExternalMethodDependency ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public class A
{
  public string DoStuff()
  {
    return Array.Empty<string>();
  }
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var externalMethod = builder.Graph.GetNode ("System.Array.Empty<string>()");
      Assert.That (methodOfA, Is.Not.Null);
      Assert.That (methodOfA.Children, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Parents, Has.Length.EqualTo (0));
      Assert.That (externalMethod.Children, Has.Length.EqualTo (0));
      Assert.That (externalMethod.Parents, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Children.First().To, Is.SameAs (externalMethod));
      Assert.That (externalMethod.ToString(), Is.EqualTo("static System.Array.Empty<string>()"));
    }

    [Test]
    public void PropertyDependency ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public class A
{
  public string DoStuff()
  {
    return SomeProperty;
  }
  
  public string SomeProperty => string.Empty;
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var propertyOfA = builder.Graph.GetNode ("Test.A.SomeProperty");
      Assert.That (methodOfA, Is.Not.Null.And.TypeOf<Method>());
      Assert.That (propertyOfA, Is.Not.Null.And.TypeOf<Property>());
      Assert.That (methodOfA.Children, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Parents, Has.Length.EqualTo (0));
      Assert.That (propertyOfA.Children, Has.Length.EqualTo (0));
      Assert.That (propertyOfA.Parents, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Children.First().To, Is.SameAs (propertyOfA));
      Assert.That (propertyOfA.Parents.First().From, Is.SameAs (methodOfA));
      Assert.That (propertyOfA.ToString(), Is.EqualTo ("Test.A.SomeProperty"));
    }

    [Test]
    public void EventFieldDependency ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public class A
{
  public void DoStuff()
  {
    SomeEvent += () => {};
  }
  
  public event Action SomeEvent;
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var eventOfA = builder.Graph.GetNode ("Test.A.SomeEvent");
      Assert.That (methodOfA, Is.Not.Null.And.TypeOf<Method>());
      Assert.That (eventOfA, Is.Not.Null.And.TypeOf<Event>());
      Assert.That (methodOfA.Children, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Parents, Has.Length.EqualTo (0));
      Assert.That (eventOfA.Children, Has.Length.EqualTo (0));
      Assert.That (eventOfA.Parents, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Children.First().To, Is.SameAs (eventOfA));
      Assert.That (eventOfA.Parents.First().From, Is.SameAs (methodOfA));
      Assert.That (eventOfA.ToString(), Is.EqualTo ("Test.A.SomeEvent"));
    }

    [Test]
    public void EventDependency ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public class A
{
  public void DoStuff()
  {
    SomeEvent += () => {};
  }
  
  public event Action SomeEvent
  {
    add { throw new NotImplementedException(); }
    remove { throw new NotImplementedException(); }
  }
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var eventOfA = builder.Graph.GetNode ("Test.A.SomeEvent");
      Assert.That (methodOfA, Is.Not.Null.And.TypeOf<Method>());
      Assert.That (eventOfA, Is.Not.Null.And.TypeOf<Event>());
      Assert.That (methodOfA.Children, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Parents, Has.Length.EqualTo (0));
      Assert.That (eventOfA.Children, Has.Length.EqualTo (0));
      Assert.That (eventOfA.Parents, Has.Length.EqualTo (1));
      Assert.That (methodOfA.Children.First().To, Is.SameAs (eventOfA));
      Assert.That (eventOfA.Parents.First().From, Is.SameAs (methodOfA));
    }

    [Test]
    public void InterfaceImplementation_SingleInterface ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public interface IA
{
  string DoStuff();
}
public class A : IA
{
  public string DoStuff()
  {
    return Array.Empty<string>();
  }
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var interfaceMethod = builder.Graph.GetNode ("Test.IA.DoStuff()");
      Assert.That (methodOfA, Is.Not.Null);
      Assert.That (interfaceMethod, Is.Not.Null);
      Assert.That (methodOfA.Parents, Has.One.Items);
      Assert.That (methodOfA.Parents.First().From, Is.SameAs(interfaceMethod));
      Assert.That (interfaceMethod.Children, Has.One.Items);
      Assert.That (interfaceMethod.Children.First().To, Is.SameAs (methodOfA));
    }

    [Test]
    public void InterfaceImplementation_MultipleInterfaces ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public interface IA
{
  string DoStuff();
}
public interface IB
{
  string DoStuff();
}
public class A : IA, IB
{
  public string DoStuff()
  {
    return Array.Empty<string>();
  }
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var methodOfA = builder.Graph.GetNode ("Test.A.DoStuff()");
      var interfaceAMethod = builder.Graph.GetNode ("Test.IA.DoStuff()");
      var interfaceBMethod = builder.Graph.GetNode ("Test.IB.DoStuff()");
      Assert.That (methodOfA, Is.Not.Null);
      Assert.That (interfaceAMethod, Is.Not.Null);
      Assert.That (interfaceBMethod, Is.Not.Null);
      Assert.That (methodOfA.Parents, Has.Exactly(2).Items);
      Assert.That (methodOfA.Parents.Select(p => p.From), Contains.Item (interfaceAMethod));
      Assert.That (methodOfA.Parents.Select(p => p.From), Contains.Item (interfaceBMethod));
    }

    [Test]
    public void Inheritance_OverriddenMethod ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public class A
{
  public abstract string DoStuff();
}
public class B : A
{
  public override string DoStuff()
  {
    return Array.Empty<string>();
  }
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var baseMethod = builder.Graph.GetNode ("Test.A.DoStuff()");
      var method = builder.Graph.GetNode ("Test.B.DoStuff()");
      Assert.That (baseMethod, Is.Not.Null);
      Assert.That (method, Is.Not.Null);
      Assert.That (baseMethod.Children, Has.One.Items);
      Assert.That (method.Parents, Has.One.Items);
      Assert.That (baseMethod.Children.First().To, Is.SameAs (method));
      Assert.That (method.Parents.First().From, Is.SameAs (baseMethod));
    }

    [Test]
    public void Inheritance_OverriddenOverriddenMethod ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public class A
{
  public abstract string DoStuff();
}
public class B : A
{
  public override string DoStuff()
  {
    return ""intermediate"";
  }
}
public class C : B
{
  public override string DoStuff()
  {
    return Array.Empty<string>();
  }
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var aMethod = builder.Graph.GetNode ("Test.A.DoStuff()");
      var bMethod = builder.Graph.GetNode ("Test.B.DoStuff()");
      var cMethod = builder.Graph.GetNode ("Test.C.DoStuff()");
      Assert.That (aMethod, Is.Not.Null);
      Assert.That (bMethod, Is.Not.Null);
      Assert.That (cMethod, Is.Not.Null);
      Assert.That (aMethod.Children, Has.One.Items);
      Assert.That (bMethod.Parents, Has.One.Items);
      Assert.That (bMethod.Children, Has.One.Items);
      Assert.That (cMethod.Parents, Has.One.Items);
      Assert.That (aMethod.Children.First().To, Is.SameAs (bMethod));
      Assert.That (bMethod.Parents.First().From, Is.SameAs (aMethod));
      Assert.That (bMethod.Children.First().To, Is.SameAs (cMethod));
      Assert.That (cMethod.Parents.First().From, Is.SameAs (bMethod));
    }

    [Test]
    public void Inheritance_OverriddenVirtualMethod ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
public class A
{
  public virtual string DoStuff()
  {
    return ""test"";
  }
}
public class B : A
{
  public override string DoStuff()
  {
    return Array.Empty<string>();
  }
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));

      builder.Visit (compilation.Item2);

      var baseMethod = builder.Graph.GetNode ("Test.A.DoStuff()");
      var method = builder.Graph.GetNode ("Test.B.DoStuff()");
      Assert.That (baseMethod, Is.Not.Null);
      Assert.That (method, Is.Not.Null);
      Assert.That (baseMethod.Children, Has.One.Items);
      Assert.That (method.Parents, Has.One.Items);
      Assert.That (baseMethod.Children.First().To, Is.SameAs (method));
      Assert.That (method.Parents.First().From, Is.SameAs (baseMethod));
    }

    [Test]
    public void RewriterYieldingAdditionalRewrites ()
    {
      var compilation = CompiledSourceFileProvider.CompileInNameSpace (
          "Test",
          //language=C#
          @"
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
    return new C().DoMore();
  }
}
public class C
{
  public object? DoMore()
  {
    return new object();
  }
}
");
      var builder = new MethodGraphBuilder (new SharedCompilation (compilation.Item1.Compilation));
      builder.Visit (compilation.Item2);
      var additionalRewrites = new List<(IRewritable, RewriteCapability)>();
      var graph = builder.Graph;
      var aMethod = (Method) graph.GetNode ("Test.A.DoStuff()");
      var bMethod = (Method) graph.GetNode ("Test.B.DoMore()");
      var rewriter = new RewriterYieldingParentMethodsAsAdditionalRewrites ((r, d) => additionalRewrites.AddRange(d));

      bMethod.Rewrite (rewriter);

      Assert.That (additionalRewrites, Has.One.Items);
    }

    private class RewriterYieldingParentMethodsAsAdditionalRewrites : RewriterBase
    {
      public RewriterYieldingParentMethodsAsAdditionalRewrites (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
          : base(additionalRewrites)
      {
      }

      public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node) => node.WithLeadingTrivia (SyntaxTriviaList.Create (new SyntaxTrivia()));

      protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode method) =>
          method.Parents
              .Select(p => p.From)
              .OfType<IRewritable>()
              .Select(r => (r, RewriteCapability.ReturnValueChange))
              .ToArray();
    }
  }
}