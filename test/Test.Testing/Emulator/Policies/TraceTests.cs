// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class TraceTests
{
    class SimpleTrace : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Trace(new TraceConfig { Source = "inbound-source", Message = "inbound-message" });
        }

        public void Backend(IBackendContext context)
        {
            context.Trace(new TraceConfig { Source = "backend-source", Message = "backend-message" });
        }

        public void Outbound(IOutboundContext context)
        {
            context.Trace(new TraceConfig { Source = "outbound-source", Message = "outbound-message" });
        }

        public void OnError(IOnErrorContext context)
        {
            context.Trace(new TraceConfig { Source = "onerror-source", Message = "onerror-message" });
        }
    }

    class TraceWithMetadata : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Trace(new TraceConfig
            {
                Source = "inbound-source",
                Message = "inbound-message",
                Severity = "information",
                Metadata = [new TraceMetadata { Name = "key", Value = "value" }]
            });
        }
    }

    [TestMethod]
    public void Trace_Inbound_RecordsEntry()
    {
        var test = new SimpleTrace().AsTestDocument();

        test.RunInbound();

        var entry = test.SetupTraceStore().Entries.Should().ContainSingle().Subject;
        entry.Source.Should().Be("inbound-source");
        entry.Message.Should().Be("inbound-message");
    }

    [TestMethod]
    public void Trace_Outbound_RecordsEntry()
    {
        var test = new SimpleTrace().AsTestDocument();

        test.RunOutbound();

        test.SetupTraceStore().Entries.Should().ContainSingle()
            .Which.Source.Should().Be("outbound-source");
    }

    [TestMethod]
    public void Trace_Backend_RecordsEntry()
    {
        var test = new SimpleTrace().AsTestDocument();

        test.RunBackend();

        test.SetupTraceStore().Entries.Should().ContainSingle()
            .Which.Source.Should().Be("backend-source");
    }

    [TestMethod]
    public void Trace_OnError_RecordsEntry()
    {
        var test = new SimpleTrace().AsTestDocument();

        test.RunOnError();

        test.SetupTraceStore().Entries.Should().ContainSingle()
            .Which.Source.Should().Be("onerror-source");
    }

    [TestMethod]
    public void Trace_WithMetadataAndSeverity_RecordsFullEntry()
    {
        var test = new TraceWithMetadata().AsTestDocument();

        test.RunInbound();

        var entry = test.SetupTraceStore().Entries.Should().ContainSingle().Subject;
        entry.Severity.Should().Be("information");
        entry.Metadata.Should().ContainSingle()
            .Which.Name.Should().Be("key");
    }

    [TestMethod]
    public void Trace_Inbound_Callback()
    {
        var test = new SimpleTrace().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound().Trace().WithCallback((_, config) =>
        {
            executedCallback = true;
            config.Message.Should().Be("inbound-message");
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
        test.SetupTraceStore().Entries.Should().BeEmpty();
    }

    [TestMethod]
    public void Trace_Inbound_CallbackWithPredicate()
    {
        var test = new SimpleTrace().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound()
            .Trace((_, config) => config.Source == "inbound-source")
            .WithCallback((_, _) => executedCallback = true);

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }
}
