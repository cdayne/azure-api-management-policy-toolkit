// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class GetAuthorizationContextTests
{
    class SimpleGetAuthorizationContext : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.GetAuthorizationContext(new GetAuthorizationContextConfig
            {
                ProviderId = "github-provider",
                AuthorizationId = "github-connection",
                ContextVariableName = "auth-context"
            });
        }

        public void Backend(IBackendContext context)
        {
            context.GetAuthorizationContext(new GetAuthorizationContextConfig
            {
                ProviderId = "github-provider",
                AuthorizationId = "github-connection",
                ContextVariableName = "auth-context"
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.GetAuthorizationContext(new GetAuthorizationContextConfig
            {
                ProviderId = "github-provider",
                AuthorizationId = "github-connection",
                ContextVariableName = "auth-context"
            });
        }

        public void OnError(IOnErrorContext context) { }
    }

    class IgnoreErrorGetAuthorizationContext : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.GetAuthorizationContext(new GetAuthorizationContextConfig
            {
                ProviderId = "github-provider",
                AuthorizationId = "github-connection",
                ContextVariableName = "auth-context",
                IgnoreError = true
            });
        }
    }

    [TestMethod]
    public void GetAuthorizationContext_Inbound_ShouldStoreDefaultAuthorization()
    {
        var test = new SimpleGetAuthorizationContext().AsTestDocument();

        test.RunInbound();

        var authorization = test.Context.Variables.Should().ContainKey("auth-context").WhoseValue
            .Should().BeOfType<Authorization>().Subject;
        authorization.AccessToken.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void GetAuthorizationContext_Outbound_ShouldStoreDefaultAuthorization()
    {
        var test = new SimpleGetAuthorizationContext().AsTestDocument();

        test.RunOutbound();

        test.Context.Variables.Should().ContainKey("auth-context")
            .WhoseValue.Should().BeOfType<Authorization>();
    }

    [TestMethod]
    public void GetAuthorizationContext_Backend_ShouldStoreDefaultAuthorization()
    {
        var test = new SimpleGetAuthorizationContext().AsTestDocument();

        test.RunBackend();

        test.Context.Variables.Should().ContainKey("auth-context")
            .WhoseValue.Should().BeOfType<Authorization>();
    }

    [TestMethod]
    public void GetAuthorizationContext_Inbound_WithAuthorizationProviderHook()
    {
        var test = new SimpleGetAuthorizationContext().AsTestDocument();
        var claims = new Dictionary<string, object> { ["scope"] = "repo" };

        test.SetupInbound()
            .GetAuthorizationContext()
            .WithAuthorizationProviderHook((providerId, authorizationId) =>
                new Authorization($"token-for-{providerId}-{authorizationId}", claims));

        test.RunInbound();

        var authorization = test.Context.Variables.Should().ContainKey("auth-context").WhoseValue
            .Should().BeOfType<Authorization>().Subject;
        authorization.AccessToken.Should().Be("token-for-github-provider-github-connection");
        authorization.Claims.Should().ContainKey("scope").WhoseValue.Should().Be("repo");
    }

    [TestMethod]
    public void GetAuthorizationContext_Inbound_ReturnsAuthorization()
    {
        var test = new SimpleGetAuthorizationContext().AsTestDocument();
        var authorization = new Authorization("static-token", new Dictionary<string, object>());

        test.SetupInbound().GetAuthorizationContext().ReturnsAuthorization(authorization);

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("auth-context")
            .WhoseValue.Should().Be(authorization);
    }

    [TestMethod]
    public void GetAuthorizationContext_Inbound_WithError_ShouldThrow()
    {
        var test = new SimpleGetAuthorizationContext().AsTestDocument();

        test.SetupInbound().GetAuthorizationContext().WithError("provider unavailable");

        var ex = Assert.ThrowsException<PolicyException>(() => test.RunInbound());
        ex.Policy.Should().Be("GetAuthorizationContext");
    }

    [TestMethod]
    public void GetAuthorizationContext_Inbound_IgnoreError_ShouldStoreNull()
    {
        var test = new IgnoreErrorGetAuthorizationContext().AsTestDocument();

        test.SetupInbound().GetAuthorizationContext().WithError("provider unavailable");

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("auth-context")
            .WhoseValue.Should().BeNull();
    }

    [TestMethod]
    public void GetAuthorizationContext_Inbound_Callback()
    {
        var test = new SimpleGetAuthorizationContext().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound().GetAuthorizationContext().WithCallback((context, config) =>
        {
            executedCallback = true;
            context.Variables[config.ContextVariableName] = "callback-value";
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
        test.Context.Variables.Should().ContainKey("auth-context")
            .WhoseValue.Should().Be("callback-value");
    }
}
