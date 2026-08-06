// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockSendServiceBusMessageProvider
{
    public static Setup SendServiceBusMessage(this MockPoliciesProvider<IInboundContext> mock) =>
        SendServiceBusMessage(mock, (_, _) => true);

    public static Setup SendServiceBusMessage(this MockPoliciesProvider<IOutboundContext> mock) =>
        SendServiceBusMessage(mock, (_, _) => true);

    public static Setup SendServiceBusMessage(this MockPoliciesProvider<IOnErrorContext> mock) =>
        SendServiceBusMessage(mock, (_, _) => true);

    public static Setup SendServiceBusMessage(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, SendServiceBusMessageConfig, bool> predicate
    ) => SendServiceBusMessage<IInboundContext>(mock, predicate);

    public static Setup SendServiceBusMessage(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, SendServiceBusMessageConfig, bool> predicate
    ) => SendServiceBusMessage<IOutboundContext>(mock, predicate);

    public static Setup SendServiceBusMessage(
        this MockPoliciesProvider<IOnErrorContext> mock,
        Func<GatewayContext, SendServiceBusMessageConfig, bool> predicate
    ) => SendServiceBusMessage<IOnErrorContext>(mock, predicate);

    private static Setup SendServiceBusMessage<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, SendServiceBusMessageConfig, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<SendServiceBusMessageHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, SendServiceBusMessageConfig, bool> _predicate;
        private readonly SendServiceBusMessageHandler _handler;

        internal Setup(
            Func<GatewayContext, SendServiceBusMessageConfig, bool> predicate,
            SendServiceBusMessageHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, SendServiceBusMessageConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
