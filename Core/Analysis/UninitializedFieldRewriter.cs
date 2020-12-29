using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.ClassFields;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class UninitializedFieldRewriter : RewriterBase
  {
    public UninitializedFieldRewriter (Action<RewriterBase, IReadOnlyCollection<IRewritable>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitClassDeclaration (ClassDeclarationSyntax node)
    {
      var semanticModel = _currentField.SemanticModel;
      var fields = new FieldLocator (node, semanticModel).LocateFields();

      var uninitializedFields = new ConstructorInitializationFilter (node, fields).GetUnitializedFields();

      return new FieldNullableAnnotator (node, uninitializedFields).AnnotateFields();
    }
  }
}