// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class SetQueryParameterIfNotExistTests
{
    class SimpleSetQueryParameterIfNotExist : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.SetQueryParameterIfNotExist("inbound", "inbound-value");
        }

        public void Backend(IBackendContext context)
        {
            context.SetQueryParameterIfNotExist("backend", "backend-value");
        }

        public void Outbound(IOutboundContext context)
        {
            context.SetQueryParameterIfNotExist("outbound", "outbound-value");
        }

        public void OnError(IOnErrorContext context)
        {
            context.SetQueryParameterIfNotExist("onerror", "onerror-value");
        }
    }

    class MultiSetQueryParameterIfNotExist : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.SetQueryParameterIfNotExist("a", "value-a");
            context.SetQueryParameterIfNotExist("b", "value-b");
        }
    }

    [TestMethod]
    public void SetQueryParameterIfNotExist_Inbound_ShouldPreserveExistingQueryParameter()
    {
        var test = new SimpleSetQueryParameterIfNotExist().AsTestDocument();
        test.Context.Request.Url.Query["inbound"] = ["existing"];

        test.RunInbound();

        test.Context.Request.Url.Query.Should().ContainKey("inbound")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("existing");
    }

    [TestMethod]
    public void SetQueryParameterIfNotExist_Backend_ShouldSetRequestQueryParameterWhenMissing()
    {
        var test = new SimpleSetQueryParameterIfNotExist().AsTestDocument();

        test.RunBackend();

        test.Context.Request.Url.Query.Should().ContainKey("backend")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("backend-value");
    }

    [TestMethod]
    public void SetQueryParameterIfNotExist_Outbound_ShouldSetRequestQueryParameterWhenMissing()
    {
        var test = new SimpleSetQueryParameterIfNotExist().AsTestDocument();

        test.RunOutbound();

        test.Context.Request.Url.Query.Should().ContainKey("outbound")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("outbound-value");
    }

    [TestMethod]
    public void SetQueryParameterIfNotExist_OnError_ShouldPreserveExistingQueryParameter()
    {
        var test = new SimpleSetQueryParameterIfNotExist().AsTestDocument();
        test.Context.Request.Url.Query["onerror"] = ["existing"];

        test.RunOnError();

        test.Context.Request.Url.Query.Should().ContainKey("onerror")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("existing");
    }

    [TestMethod]
    public void SetQueryParameterIfNotExist_Inbound_Callback()
    {
        var test = new SimpleSetQueryParameterIfNotExist().AsTestDocument();
        var callbackExecuted = false;

        test.SetupInbound().SetQueryParameterIfNotExist().WithCallback((context, name, values) =>
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
    public void SetQueryParameterIfNotExist_Inbound_PredicateCallback()
    {
        var test = new MultiSetQueryParameterIfNotExist().AsTestDocument();
        var matchedCount = 0;

        test.SetupInbound()
            .SetQueryParameterIfNotExist((_, name, _) => name == "b")
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
