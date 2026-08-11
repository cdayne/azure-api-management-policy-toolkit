// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockInvokeDarpBindingProvider
{
    public static Setup InvokeDarpBinding(this MockPoliciesProvider<IInboundContext> mock) =>
        InvokeDarpBinding(mock, (_, _) => true);

    public static Setup InvokeDarpBinding(this MockPoliciesProvider<IOutboundContext> mock) =>
        InvokeDarpBinding(mock, (_, _) => true);

    public static Setup InvokeDarpBinding(this MockPoliciesProvider<IOnErrorContext> mock) =>
        InvokeDarpBinding(mock, (_, _) => true);

    public static Setup InvokeDarpBinding(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, InvokeDarpBindingConfig, bool> predicate
    ) => InvokeDarpBinding<IInboundContext>(mock, predicate);

    public static Setup InvokeDarpBinding(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, InvokeDarpBindingConfig, bool> predicate
    ) => InvokeDarpBinding<IOutboundContext>(mock, predicate);

    public static Setup InvokeDarpBinding(
        this MockPoliciesProvider<IOnErrorContext> mock,
        Func<GatewayContext, InvokeDarpBindingConfig, bool> predicate
    ) => InvokeDarpBinding<IOnErrorContext>(mock, predicate);

    private static Setup InvokeDarpBinding<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, InvokeDarpBindingConfig, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<InvokeDarpBindingHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, InvokeDarpBindingConfig, bool> _predicate;
        private readonly InvokeDarpBindingHandler _handler;

        internal Setup(
            Func<GatewayContext, InvokeDarpBindingConfig, bool> predicate,
            InvokeDarpBindingHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, InvokeDarpBindingConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
