// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockPublishToDarpProvider
{
    public static Setup PublishToDarp(this MockPoliciesProvider<IInboundContext> mock) =>
        PublishToDarp(mock, (_, _) => true);

    public static Setup PublishToDarp(this MockPoliciesProvider<IOutboundContext> mock) =>
        PublishToDarp(mock, (_, _) => true);

    public static Setup PublishToDarp(this MockPoliciesProvider<IOnErrorContext> mock) =>
        PublishToDarp(mock, (_, _) => true);

    public static Setup PublishToDarp(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, PublishToDarpConfig, bool> predicate
    ) => PublishToDarp<IInboundContext>(mock, predicate);

    public static Setup PublishToDarp(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, PublishToDarpConfig, bool> predicate
    ) => PublishToDarp<IOutboundContext>(mock, predicate);

    public static Setup PublishToDarp(
        this MockPoliciesProvider<IOnErrorContext> mock,
        Func<GatewayContext, PublishToDarpConfig, bool> predicate
    ) => PublishToDarp<IOnErrorContext>(mock, predicate);

    private static Setup PublishToDarp<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, PublishToDarpConfig, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<PublishToDarpHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, PublishToDarpConfig, bool> _predicate;
        private readonly PublishToDarpHandler _handler;

        internal Setup(
            Func<GatewayContext, PublishToDarpConfig, bool> predicate,
            PublishToDarpHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, PublishToDarpConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
