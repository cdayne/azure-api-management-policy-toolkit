// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class PublishToDarpTests
{
    class SimplePublishToDarp : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.PublishToDarp(new PublishToDarpConfig
            {
                Topic = "inbound-topic",
                Content = "inbound-content",
                PubSubName = "pubsub-component",
                IgnoreError = false,
                ResponseVariableName = "publishResponse",
                Timeout = 5000,
                Template = "liquid",
                ContentType = "application/json"
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.PublishToDarp(new PublishToDarpConfig { Topic = "outbound-topic", Content = "outbound-content" });
        }

        public void OnError(IOnErrorContext context)
        {
            context.PublishToDarp(new PublishToDarpConfig { Topic = "onerror-topic", Content = "onerror-content" });
        }
    }

    [TestMethod]
    public void PublishToDarp_Inbound_ShouldNotThrow()
    {
        var test = new SimplePublishToDarp().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void PublishToDarp_Outbound_ShouldNotThrow()
    {
        var test = new SimplePublishToDarp().AsTestDocument();

        test.RunOutbound();
    }

    [TestMethod]
    public void PublishToDarp_OnError_ShouldNotThrow()
    {
        var test = new SimplePublishToDarp().AsTestDocument();

        test.RunOnError();
    }

    [TestMethod]
    public void PublishToDarp_Inbound_Callback()
    {
        var test = new SimplePublishToDarp().AsTestDocument();
        string? observedTopic = null;

        test.SetupInbound().PublishToDarp().WithCallback((context, config) =>
        {
            observedTopic = config.Topic;
            context.Variables["topic"] = config.Topic;
        });

        test.RunInbound();

        observedTopic.Should().Be("inbound-topic");
        test.Context.Variables.Should().ContainKey("topic")
            .WhoseValue.Should().Be("inbound-topic");
    }

    [TestMethod]
    public void PublishToDarp_Outbound_Callback()
    {
        var test = new SimplePublishToDarp().AsTestDocument();
        var executedCallback = false;

        test.SetupOutbound().PublishToDarp().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunOutbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void PublishToDarp_OnError_Callback()
    {
        var test = new SimplePublishToDarp().AsTestDocument();
        var executedCallback = false;

        test.SetupOnError().PublishToDarp().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunOnError();

        executedCallback.Should().BeTrue();
    }
}
