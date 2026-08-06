// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class SetQueryParameterTests
{
    class SimpleSetQueryParameter : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.SetQueryParameter("inbound", "inbound-value");
        }

        public void Backend(IBackendContext context)
        {
            context.SetQueryParameter("backend", "backend-value");
        }

        public void Outbound(IOutboundContext context)
        {
            context.SetQueryParameter("outbound", "outbound-value");
        }

        public void OnError(IOnErrorContext context)
        {
            context.SetQueryParameter("onerror", "onerror-value");
        }
    }

    class MultiSetQueryParameter : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.SetQueryParameter("a", "value-a");
            context.SetQueryParameter("b", "value-b");
        }
    }

    [TestMethod]
    public void SetQueryParameter_Inbound_ShouldOverwriteExistingQueryParameter()
    {
        var test = new SimpleSetQueryParameter().AsTestDocument();
        test.Context.Request.Url.Query["inbound"] = ["existing"];

        test.RunInbound();

        test.Context.Request.Url.Query.Should().ContainKey("inbound")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("inbound-value");
    }

    [TestMethod]
    public void SetQueryParameter_Backend_ShouldSetRequestQueryParameter()
    {
        var test = new SimpleSetQueryParameter().AsTestDocument();

        test.RunBackend();

        test.Context.Request.Url.Query.Should().ContainKey("backend")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("backend-value");
    }

    [TestMethod]
    public void SetQueryParameter_Outbound_ShouldSetRequestQueryParameter()
    {
        var test = new SimpleSetQueryParameter().AsTestDocument();

        test.RunOutbound();

        test.Context.Request.Url.Query.Should().ContainKey("outbound")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("outbound-value");
    }

    [TestMethod]
    public void SetQueryParameter_OnError_ShouldSetRequestQueryParameter()
    {
        var test = new SimpleSetQueryParameter().AsTestDocument();

        test.RunOnError();

        test.Context.Request.Url.Query.Should().ContainKey("onerror")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("onerror-value");
    }

    [TestMethod]
    public void SetQueryParameter_Inbound_Callback()
    {
        var test = new SimpleSetQueryParameter().AsTestDocument();
        var callbackExecuted = false;

        test.SetupInbound().SetQueryParameter().WithCallback((context, name, values) =>
        {
            callbackExecuted = true;
            context.Request.Url.Query[name] = [values[0] + "-callback"];
        });

        test.RunInbound();

        callbackExecuted.Should().BeTrue();
        test.Context.Request.Url.Query.Should().ContainKey("inbound")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("inbound-value-callback");
    }

    [TestMethod]
    public void SetQueryParameter_Inbound_PredicateCallback()
    {
        var test = new MultiSetQueryParameter().AsTestDocument();
        var matchedCount = 0;

        test.SetupInbound()
            .SetQueryParameter((_, name, _) => name == "b")
            .WithCallback((context, name, values) =>
            {
                matchedCount++;
                context.Request.Url.Query[name] = [values[0] + "-callback"];
            });

        test.RunInbound();

        matchedCount.Should().Be(1);
        test.Context.Request.Url.Query.Should().ContainKey("a")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("value-a");
        test.Context.Request.Url.Query.Should().ContainKey("b")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("value-b-callback");
    }
}
