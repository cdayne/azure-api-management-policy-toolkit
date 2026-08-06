// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockMockResponseProvider
{
    public static Setup MockResponse(this MockPoliciesProvider<IInboundContext> mock) =>
        MockResponse(mock, (_, _) => true);

    public static Setup MockResponse(this MockPoliciesProvider<IOutboundContext> mock) =>
        MockResponse(mock, (_, _) => true);

    public static Setup MockResponse(this MockPoliciesProvider<IOnErrorContext> mock) =>
        MockResponse(mock, (_, _) => true);

    public static Setup MockResponse(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, MockResponseConfig?, bool> predicate
    ) => MockResponse<IInboundContext>(mock, predicate);

    public static Setup MockResponse(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, MockResponseConfig?, bool> predicate
    ) => MockResponse<IOutboundContext>(mock, predicate);

    public static Setup MockResponse(
        this MockPoliciesProvider<IOnErrorContext> mock,
        Func<GatewayContext, MockResponseConfig?, bool> predicate
    ) => MockResponse<IOnErrorContext>(mock, predicate);

    private static Setup MockResponse<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, MockResponseConfig?, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<MockResponseHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, MockResponseConfig?, bool> _predicate;
        private readonly MockResponseHandler _handler;

        internal Setup(
            Func<GatewayContext, MockResponseConfig?, bool> predicate,
            MockResponseHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, MockResponseConfig?> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}