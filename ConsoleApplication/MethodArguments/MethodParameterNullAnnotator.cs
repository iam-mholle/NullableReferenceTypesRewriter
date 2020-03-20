﻿// Copyright (c) rubicon IT GmbH
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.ConsoleApplication.Utilities;

namespace NullableReferenceTypesRewriter.ConsoleApplication.MethodArguments
{
  public class MethodParameterNullAnnotator : CSharpSyntaxRewriter
  {
    private readonly IReadOnlyCollection<ParameterSyntax> _nullableParameters;

    public MethodParameterNullAnnotator (IReadOnlyCollection<ParameterSyntax> nullableParameters)
    {
      _nullableParameters = nullableParameters;
    }

    public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var newParameters = node.ParameterList.Parameters;

      foreach (var parameter in node.ParameterList.Parameters)
      {
        if (_nullableParameters.Contains (parameter))
        {
          if (parameter.Type == null)
            continue;

          var toReplace = newParameters.SingleOrDefault (param => param.Identifier.ToString() == parameter.Identifier.ToString());
          if (toReplace.Type != null)
            newParameters = newParameters.Replace (toReplace, toReplace.WithType (NullUtilities.MakeNullable (toReplace.Type)));
        }
      }

      return node.WithParameterList (node.ParameterList.WithParameters (newParameters));
    }
  }
}