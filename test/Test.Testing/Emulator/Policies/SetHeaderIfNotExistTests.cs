// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class SetHeaderIfNotExistTests
{
    class SimpleSetHeaderIfNotExist : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.SetHeaderIfNotExist("X-Inbound", "inbound-value");
        }

        public void Backend(IBackendContext context)
        {
            context.SetHeaderIfNotExist("X-Backend", "backend-value");
        }

        public void Outbound(IOutboundContext context)
        {
            context.SetHeaderIfNotExist("X-Outbound", "outbound-value");
        }

        public void OnError(IOnErrorContext context)
        {
            context.SetHeaderIfNotExist("X-OnError", "onerror-value");
        }
    }

    class MultiSetHeaderIfNotExist : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.SetHeaderIfNotExist("A", "value-a");
            context.SetHeaderIfNotExist("B", "value-b");
        }
    }

    [TestMethod]
    public void SetHeaderIfNotExist_Inbound_ShouldPreserveExistingHeader()
    {
        var test = new SimpleSetHeaderIfNotExist().AsTestDocument();
        test.Context.Request.Headers["X-Inbound"] = ["existing"];

        test.RunInbound();

        test.Context.Request.Headers.Should().ContainKey("X-Inbound")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("existing");
    }

    [TestMethod]
    public void SetHeaderIfNotExist_Backend_ShouldAddRequestHeader()
    {
        var test = new SimpleSetHeaderIfNotExist().AsTestDocument();

        test.RunBackend();

        test.Context.Request.Headers.Should().ContainKey("X-Backend")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("backend-value");
    }

    [TestMethod]
    public void SetHeaderIfNotExist_Outbound_ShouldAddResponseHeader()
    {
        var test = new SimpleSetHeaderIfNotExist().AsTestDocument();

        test.RunOutbound();

        test.Context.Response.Headers.Should().ContainKey("X-Outbound")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("outbound-value");
    }

    [TestMethod]
    public void SetHeaderIfNotExist_OnError_ShouldPreserveExistingResponseHeader()
    {
        var test = new SimpleSetHeaderIfNotExist().AsTestDocument();
        test.Context.Response.Headers["X-OnError"] = ["existing"];

        test.RunOnError();

        test.Context.Response.Headers.Should().ContainKey("X-OnError")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("existing");
    }

    [TestMethod]
    public void SetHeaderIfNotExist_Inbound_Callback()
    {
        var test = new SimpleSetHeaderIfNotExist().AsTestDocument();
        var callbackExecuted = false;

        test.SetupInbound().SetHeaderIfNotExist().WithCallback((context, name, values) =>
        {
            callbackExecuted = true;
            context.Request.Headers[name] = [values[0] + "-callback"];
        });

        test.RunInbound();

        callbackExecuted.Should().BeTrue();
        test.Context.Request.Headers.Should().ContainKey("X-Inbound")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("inbound-value-callback");
    }

    [TestMethod]
    public void SetHeaderIfNotExist_Inbound_PredicateCallback()
    {
        var test = new MultiSetHeaderIfNotExist().AsTestDocument();
        var matchedCount = 0;

        test.SetupInbound()
            .SetHeaderIfNotExist((_, name, _) => name == "B")
            .WithCallback((context, name, values) =>
            {
                matchedCount++;
                context.Request.Headers[name] = [values[0] + "-callback"];
            });

        test.RunInbound();

        matchedCount.Should().Be(1);
        test.Context.Request.Headers.Should().ContainKey("A")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("value-a");
        test.Context.Request.Headers.Should().ContainKey("B")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("value-b-callback");
    }
}
