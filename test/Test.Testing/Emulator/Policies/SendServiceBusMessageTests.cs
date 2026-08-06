// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class SendServiceBusMessageTests
{
    class SimpleSendServiceBusMessage : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.SendServiceBusMessage(new SendServiceBusMessageConfig
            {
                QueueName = "orders",
                Namespace = "contoso.servicebus.windows.net",
                ClientId = "client-id",
                Payload = "inbound-payload",
                MessageProperties = [new ServiceBusMessageProperty { Name = "kind", Value = "inbound" }]
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.SendServiceBusMessage(new SendServiceBusMessageConfig
            {
                TopicName = "audit-topic",
                Payload = "outbound-payload"
            });
        }

        public void OnError(IOnErrorContext context)
        {
            context.SendServiceBusMessage(new SendServiceBusMessageConfig
            {
                QueueName = "dead-letter",
                Payload = "error-payload"
            });
        }
    }

    class MultiSendServiceBusMessage : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.SendServiceBusMessage(new SendServiceBusMessageConfig
            {
                QueueName = "queue-a",
                Payload = "payload-a"
            });
            context.SendServiceBusMessage(new SendServiceBusMessageConfig
            {
                TopicName = "topic-b",
                Payload = "payload-b"
            });
        }
    }

    [TestMethod]
    public void SendServiceBusMessage_Inbound_ShouldNotThrow()
    {
        var test = new SimpleSendServiceBusMessage().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void SendServiceBusMessage_Outbound_ShouldNotThrow()
    {
        var test = new SimpleSendServiceBusMessage().AsTestDocument();

        test.RunOutbound();
    }

    [TestMethod]
    public void SendServiceBusMessage_OnError_ShouldNotThrow()
    {
        var test = new SimpleSendServiceBusMessage().AsTestDocument();

        test.RunOnError();
    }

    [TestMethod]
    public void SendServiceBusMessage_Inbound_Callback()
    {
        var test = new SimpleSendServiceBusMessage().AsTestDocument();
        string? observedPayload = null;

        test.SetupInbound().SendServiceBusMessage().WithCallback((context, config) =>
        {
            observedPayload = config.Payload;
            context.Variables["service-bus-target"] = config.QueueName;
        });

        test.RunInbound();

        observedPayload.Should().Be("inbound-payload");
        test.Context.Variables.Should().ContainKey("service-bus-target")
            .WhoseValue.Should().Be("orders");
    }

    [TestMethod]
    public void SendServiceBusMessage_Inbound_PredicateCallback()
    {
        var test = new MultiSendServiceBusMessage().AsTestDocument();
        var matchedCount = 0;

        test.SetupInbound()
            .SendServiceBusMessage((_, config) => config.TopicName == "topic-b")
            .WithCallback((context, config) =>
            {
                matchedCount++;
                context.Variables["selected-topic"] = config.TopicName;
            });

        test.RunInbound();

        matchedCount.Should().Be(1);
        test.Context.Variables.Should().ContainKey("selected-topic")
            .WhoseValue.Should().Be("topic-b");
    }
}
