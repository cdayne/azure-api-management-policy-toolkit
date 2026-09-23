// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Analyzers;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Analyzers.Test;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Test.Analyzers;

[TestClass]
public class TypeUsedTests
{
    public static Task VerifyAsync(string source, params DiagnosticResult[] diags)
    {
        return new BaseAnalyzerTest<TypeUsedAnalyzer>(source, diags).RunAsync();
    }

    [TestMethod]
    public async Task Should()
    {
        await VerifyAsync(
            """
            public static class ExpressionLibrary
            {
                [Expression]
                public static string Method(IExpressionContext context)
                { 
                    if(context.Request.Headers.TryGetValue("Authorization", out var value))
                    {
                        return value[0];
                    } else 
                    {
                        return "";
                    }
                }

                public static string Good(IExpressionContext context)
                { 
                    return "test".GetType().FullName;
                }
            }
            """
        );
    }

    [TestMethod]
    public async Task ShouldAllowDictionaryExtensions()
    {
        await VerifyAsync(
            """
            public static class ExpressionLibrary
            {
                [Expression]
                public static string Header(IExpressionContext context)
                    => context.Request.Headers.GetValueOrDefault("Authorization", "");

                [Expression]
                public static string Query(IExpressionContext context)
                    => context.Request.Body.AsFormUrlEncodedContent().ToQueryString();
            }
            """
        );
    }

    [TestMethod]
    public async Task ShouldAllowJwtAndBasicAuthExtensions()
    {
        await VerifyAsync(
            """
            public static class ExpressionLibrary
            {
                [Expression]
                public static bool IsAdmin(IExpressionContext context)
                {
                    var jwt = context.Request.Headers.GetValueOrDefault("Authorization", "").AsJwt();
                    return jwt != null && jwt.Claims.GetValueOrDefault("role", "") == "admin";
                }

                [Expression]
                public static string User(IExpressionContext context)
                    => context.Request.Headers.GetValueOrDefault("Authorization", "").TryParseBasic(out var credentials)
                        ? credentials.Password
                        : "";
            }
            """
        );
    }

    [TestMethod]
    public async Task ShouldAllowRegexGroupCollectionMembers()
    {
        await VerifyAsync(
            """
            public static class ExpressionLibrary
            {
                [Expression]
                public static string Method(IExpressionContext context)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(context.Request.Url.Path, "(?<id>[0-9]+)");
                    return match.Groups.Count > 1 ? match.Groups[1].Value + match.Groups["id"].Value : "";
                }
            }
            """
        );
    }

    [TestMethod]
    public async Task ShouldReportDisallowedGroupCollectionMember()
    {
        await VerifyAsync(
            """
            public static class ExpressionLibrary
            {
                [Expression]
                public static object Method(IExpressionContext context)
                    => System.Text.RegularExpressions.Regex.Match(context.Request.Url.Path, "[0-9]+").Groups.Keys;
            }
            """,
            DiagnosticResult
                .CompilerError(Rules.TypeUsed.DisallowedMember.Id)
                .WithSpan(10, 12, 10, 102)
                .WithArguments("Keys")
        );
    }

    [TestMethod]
    public async Task ShouldAllowNamespaceQualifiedMemberAccess()
    {
        await VerifyAsync(
            """
            public static class ExpressionLibrary
            {
                [Expression]
                public static string Method(IExpressionContext context) => System.String.Empty;
            }
            """
        );
    }
}
