// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class CorsTests
{
    class SimpleCors : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Cors(new CorsConfig
            {
                AllowCredentials = true,
                TerminateUnmatchedRequest = "false",
                AllowedOrigins = ["https://contoso.com"],
                AllowedMethods = ["GET", "POST"],
                AllowedHeaders = ["X-Test", "Content-Type"],
                ExposeHeaders = ["X-Expose"],
                PreflightResultMaxAge = 60
            });
        }
    }

    class WildcardCors : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Cors(new CorsConfig
            {
                AllowedOrigins = ["*"],
                AllowedHeaders = ["*"],
                AllowedMethods = ["GET"]
            });
        }
    }

    class MultiCors : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Cors(new CorsConfig
            {
                AllowedOrigins = ["https://a.example"],
                AllowedHeaders = ["X-A"]
            });
            context.Cors(new CorsConfig
            {
                AllowedOrigins = ["https://b.example"],
                AllowedHeaders = ["X-B"]
            });
        }
    }

    [TestMethod]
    public void Cors_Inbound_ShouldSetCorsHeadersForMatchingOrigin()
    {
        var test = new SimpleCors().AsTestDocument();
        test.Context.Request.Headers["Origin"] = ["https://contoso.com"];

        test.RunInbound();

        test.Context.Response.Headers.Should().ContainKey("Access-Control-Allow-Origin")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("https://contoso.com");
        test.Context.Response.Headers.Should().ContainKey("Access-Control-Allow-Credentials")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("true");
        test.Context.Response.Headers.Should().ContainKey("Access-Control-Allow-Methods")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("GET,POST");
        test.Context.Response.Headers.Should().ContainKey("Access-Control-Allow-Headers")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("X-Test,Content-Type");
        test.Context.Response.Headers.Should().ContainKey("Access-Control-Expose-Headers")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("X-Expose");
        test.Context.Response.Headers.Should().ContainKey("Access-Control-Max-Age")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("60");
    }

    [TestMethod]
    public void Cors_Inbound_ShouldUseWildcardOrigin()
    {
        var test = new WildcardCors().AsTestDocument();
        test.Context.Request.Headers["Origin"] = ["https://any.example"];

        test.RunInbound();

        test.Context.Response.Headers.Should().ContainKey("Access-Control-Allow-Origin")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("*");
    }

    [TestMethod]
    public void Cors_Inbound_Callback()
    {
        var test = new SimpleCors().AsTestDocument();
        test.Context.Request.Headers["Origin"] = ["https://contoso.com"];
        var callbackExecuted = false;

        test.SetupInbound().Cors().WithCallback((context, config) =>
        {
            callbackExecuted = true;
            context.Response.Headers["X-Cors-Callback"] = [config.AllowedOrigins.Single()];
        });

        test.RunInbound();

        callbackExecuted.Should().BeTrue();
        test.Context.Response.Headers.Should().ContainKey("X-Cors-Callback")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("https://contoso.com");
    }

    [TestMethod]
    public void Cors_Inbound_PredicateCallback()
    {
        var test = new MultiCors().AsTestDocument();
        test.Context.Request.Headers["Origin"] = ["https://b.example"];
        var matchedCount = 0;

        test.SetupInbound()
            .Cors((_, config) => config.AllowedOrigins.Contains("https://b.example"))
            .WithCallback((context, config) =>
            {
                matchedCount++;
                context.Response.Headers["X-Cors-Matched"] = [config.AllowedOrigins.Single()];
            });

        test.RunInbound();

        matchedCount.Should().Be(1);
        test.Context.Response.Headers.Should().ContainKey("X-Cors-Matched")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("https://b.example");
    }
}
