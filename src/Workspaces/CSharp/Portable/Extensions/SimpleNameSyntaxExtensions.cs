﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Extensions
{
    internal static class SimpleNameSyntaxExtensions
    {
        public static ExpressionSyntax GetLeftSideOfDot(this SimpleNameSyntax name)
        {
            Contract.Requires(name.IsMemberAccessExpressionName() || name.IsRightSideOfQualifiedName() || name.IsParentKind(SyntaxKind.NameMemberCref));
            if (name.IsMemberAccessExpressionName())
            {
                var conditionalAccess = name.GetParentConditionalAccessExpression();
                if (conditionalAccess != null)
                {
                    return conditionalAccess.Expression;
                }
                else
                {
                    return ((MemberAccessExpressionSyntax)name.Parent).Expression;
                }
            }
            else if (name.IsRightSideOfQualifiedName())
            {
                return ((QualifiedNameSyntax)name.Parent).Left;
            }
            else
            {
                return ((QualifiedCrefSyntax)name.Parent.Parent).Container;
            }
        }

        // Returns true if this looks like a possible type name that is on it's own (i.e. not after
        // a dot).  This function is not exhaustive and additional checks may be added if they are
        // believed to be valuable.
        public static bool LooksLikeStandaloneTypeName(this SimpleNameSyntax simpleName)
        {
            if (simpleName == null)
            {
                return false;
            }

            // Isn't stand-alone if it's on the right of a dot/arrow
            if (simpleName.IsRightSideOfDotOrArrow())
            {
                return false;
            }

            // type names can't be invoked.
            if (simpleName.IsParentKind(SyntaxKind.InvocationExpression) &&
                ((InvocationExpressionSyntax)simpleName.Parent).Expression == simpleName)
            {
                return false;
            }

            // type names can't be indexed into.
            if (simpleName.IsParentKind(SyntaxKind.ElementAccessExpression) &&
                ((ElementAccessExpressionSyntax)simpleName.Parent).Expression == simpleName)
            {
                return false;
            }

            if (simpleName.IsVar)
            {
                // 'var' is much more likely to represent a keyword rather than "a possible type name".
                return false;
            }

            // Looks good.  However, feel free to add additional checks if this function is too
            // lenient in some circumstances.
            return true;
        }
    }
}
