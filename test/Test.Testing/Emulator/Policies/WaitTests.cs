// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class WaitTests
{
    class SimpleWait : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Wait(() =>
            {
                context.SetVariable("waited", true);
            });
        }

        public void Backend(IBackendContext context) { }
        public void Outbound(IOutboundContext context) { }
        public void OnError(IOnErrorContext context) { }
    }

    class WaitForAny : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Wait(() =>
            {
                context.SetVariable("waited-any", true);
            }, "any");
        }

        public void Backend(IBackendContext context) { }
        public void Outbound(IOutboundContext context) { }
        public void OnError(IOnErrorContext context) { }
    }

    class OutboundWait : IDocument
    {
        public void Inbound(IInboundContext context) { }
        public void Backend(IBackendContext context) { }

        public void Outbound(IOutboundContext context)
        {
            context.Wait(() =>
            {
                context.SetVariable("outbound-waited", true);
            });
        }

        public void OnError(IOnErrorContext context) { }
    }

    class BackendWait : IDocument
    {
        public void Inbound(IInboundContext context) { }

        public void Backend(IBackendContext context)
        {
            context.Wait(() =>
            {
                context.SetVariable("backend-waited", true);
            });
        }

        public void Outbound(IOutboundContext context) { }
        public void OnError(IOnErrorContext context) { }
    }

    class OnErrorWait : IDocument
    {
        public void Inbound(IInboundContext context) { }
        public void Backend(IBackendContext context) { }
        public void Outbound(IOutboundContext context) { }

        public void OnError(IOnErrorContext context)
        {
            context.Wait(() =>
            {
                context.SetVariable("onerror-waited", true);
            });
        }
    }

    [TestMethod]
    public void Wait_Inbound_ShouldExecuteSection()
    {
        var test = new SimpleWait().AsTestDocument();

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("waited")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void Wait_Inbound_ShouldPassWaitForValue()
    {
        var test = new WaitForAny().AsTestDocument();

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("waited-any")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void Wait_Outbound_ShouldExecuteSection()
    {
        var test = new OutboundWait().AsTestDocument();

        test.RunOutbound();

        test.Context.Variables.Should().ContainKey("outbound-waited")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void Wait_Backend_ShouldExecuteSection()
    {
        var test = new BackendWait().AsTestDocument();

        test.RunBackend();

        test.Context.Variables.Should().ContainKey("backend-waited")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void Wait_OnError_ShouldExecuteSection()
    {
        var test = new OnErrorWait().AsTestDocument();

        test.RunOnError();

        test.Context.Variables.Should().ContainKey("onerror-waited")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void Wait_Inbound_Callback()
    {
        var test = new SimpleWait().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound().Wait().WithCallback((_, section, _) =>
        {
            executedCallback = true;
            section();
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
        test.Context.Variables.Should().ContainKey("waited")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void Wait_Inbound_CallbackCanSkipSection()
    {
        var test = new SimpleWait().AsTestDocument();

        test.SetupInbound().Wait().WithCallback((context, _, _) =>
        {
            context.Variables["skipped"] = true;
        });

        test.RunInbound();

        test.Context.Variables.Should().NotContainKey("waited");
        test.Context.Variables.Should().ContainKey("skipped")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void Wait_Inbound_CallbackWithPredicate()
    {
        var test = new WaitForAny().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound()
            .Wait((_, _, waitFor) => waitFor == "any")
            .WithCallback((_, section, _) =>
            {
                executedCallback = true;
                section();
            });

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }
}
