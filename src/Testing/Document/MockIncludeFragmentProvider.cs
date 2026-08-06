// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockIncludeFragmentProvider
{
    public static Setup IncludeFragment(this MockPoliciesProvider<IInboundContext> mock) =>
        IncludeFragment(mock, (_, _) => true);

    public static Setup IncludeFragment(this MockPoliciesProvider<IBackendContext> mock) =>
        IncludeFragment(mock, (_, _) => true);

    public static Setup IncludeFragment(this MockPoliciesProvider<IOutboundContext> mock) =>
        IncludeFragment(mock, (_, _) => true);

    public static Setup IncludeFragment(this MockPoliciesProvider<IOnErrorContext> mock) =>
        IncludeFragment(mock, (_, _) => true);

    public static Setup IncludeFragment(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, string, bool> predicate
    ) => IncludeFragment<IInboundContext>(mock, predicate);

    public static Setup IncludeFragment(
        this MockPoliciesProvider<IBackendContext> mock,
        Func<GatewayContext, string, bool> predicate
    ) => IncludeFragment<IBackendContext>(mock, predicate);

    public static Setup IncludeFragment(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, string, bool> predicate
    ) => IncludeFragment<IOutboundContext>(mock, predicate);

    public static Setup IncludeFragment(
        this MockPoliciesProvider<IOnErrorContext> mock,
        Func<GatewayContext, string, bool> predicate
    ) => IncludeFragment<IOnErrorContext>(mock, predicate);

    private static Setup IncludeFragment<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, string, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<IncludeFragmentHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, string, bool> _predicate;
        private readonly IncludeFragmentHandler _handler;

        internal Setup(
            Func<GatewayContext, string, bool> predicate,
            IncludeFragmentHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, string> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
