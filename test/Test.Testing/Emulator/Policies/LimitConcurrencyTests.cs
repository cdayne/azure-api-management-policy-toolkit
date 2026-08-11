// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class LimitConcurrencyTests
{
    class SimpleLimitConcurrency : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.LimitConcurrency(new LimitConcurrencyConfig { Key = "my-key", MaxCount = 5 }, () =>
            {
                context.SetVariable("executed", true);
            });
        }

        public void Backend(IBackendContext context) { }
        public void Outbound(IOutboundContext context) { }
        public void OnError(IOnErrorContext context) { }
    }

    class OutboundLimitConcurrency : IDocument
    {
        public void Inbound(IInboundContext context) { }
        public void Backend(IBackendContext context) { }

        public void Outbound(IOutboundContext context)
        {
            context.LimitConcurrency(new LimitConcurrencyConfig { Key = "my-key", MaxCount = 2 }, () =>
            {
                context.SetVariable("outbound-executed", true);
            });
        }

        public void OnError(IOnErrorContext context) { }
    }

    class BackendLimitConcurrency : IDocument
    {
        public void Inbound(IInboundContext context) { }

        public void Backend(IBackendContext context)
        {
            context.LimitConcurrency(new LimitConcurrencyConfig { Key = "my-key", MaxCount = 2 }, () =>
            {
                context.SetVariable("backend-executed", true);
            });
        }

        public void Outbound(IOutboundContext context) { }
        public void OnError(IOnErrorContext context) { }
    }

    class OnErrorLimitConcurrency : IDocument
    {
        public void Inbound(IInboundContext context) { }
        public void Backend(IBackendContext context) { }
        public void Outbound(IOutboundContext context) { }

        public void OnError(IOnErrorContext context)
        {
            context.LimitConcurrency(new LimitConcurrencyConfig { Key = "my-key", MaxCount = 2 }, () =>
            {
                context.SetVariable("onerror-executed", true);
            });
        }
    }

    [TestMethod]
    public void LimitConcurrency_Inbound_ShouldExecuteSection()
    {
        var test = new SimpleLimitConcurrency().AsTestDocument();

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("executed")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void LimitConcurrency_Outbound_ShouldExecuteSection()
    {
        var test = new OutboundLimitConcurrency().AsTestDocument();

        test.RunOutbound();

        test.Context.Variables.Should().ContainKey("outbound-executed")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void LimitConcurrency_Backend_ShouldExecuteSection()
    {
        var test = new BackendLimitConcurrency().AsTestDocument();

        test.RunBackend();

        test.Context.Variables.Should().ContainKey("backend-executed")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void LimitConcurrency_OnError_ShouldExecuteSection()
    {
        var test = new OnErrorLimitConcurrency().AsTestDocument();

        test.RunOnError();

        test.Context.Variables.Should().ContainKey("onerror-executed")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void LimitConcurrency_Inbound_Callback()
    {
        var test = new SimpleLimitConcurrency().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound().LimitConcurrency().WithCallback((_, config, section) =>
        {
            executedCallback = true;
            config.Key.Should().Be("my-key");
            section();
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
        test.Context.Variables.Should().ContainKey("executed")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void LimitConcurrency_Inbound_CallbackCanSimulateLimitExceededBySkippingSection()
    {
        var test = new SimpleLimitConcurrency().AsTestDocument();

        test.SetupInbound().LimitConcurrency().WithCallback((context, _, _) =>
        {
            context.Variables["limit-exceeded"] = true;
        });

        test.RunInbound();

        test.Context.Variables.Should().NotContainKey("executed");
        test.Context.Variables.Should().ContainKey("limit-exceeded")
            .WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void LimitConcurrency_Inbound_CallbackWithPredicate()
    {
        var test = new SimpleLimitConcurrency().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound()
            .LimitConcurrency((_, config, _) => config.MaxCount == 5)
            .WithCallback((_, _, section) =>
            {
                executedCallback = true;
                section();
            });

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }
}
