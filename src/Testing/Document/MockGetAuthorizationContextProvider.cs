// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockGetAuthorizationContextProvider
{
    public static Setup GetAuthorizationContext(this MockPoliciesProvider<IInboundContext> mock) =>
        GetAuthorizationContext(mock, (_, _) => true);

    public static Setup GetAuthorizationContext(this MockPoliciesProvider<IBackendContext> mock) =>
        GetAuthorizationContext(mock, (_, _) => true);

    public static Setup GetAuthorizationContext(this MockPoliciesProvider<IOutboundContext> mock) =>
        GetAuthorizationContext(mock, (_, _) => true);

    public static Setup GetAuthorizationContext(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, GetAuthorizationContextConfig, bool> predicate
    ) => GetAuthorizationContext<IInboundContext>(mock, predicate);

    public static Setup GetAuthorizationContext(
        this MockPoliciesProvider<IBackendContext> mock,
        Func<GatewayContext, GetAuthorizationContextConfig, bool> predicate
    ) => GetAuthorizationContext<IBackendContext>(mock, predicate);

    public static Setup GetAuthorizationContext(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, GetAuthorizationContextConfig, bool> predicate
    ) => GetAuthorizationContext<IOutboundContext>(mock, predicate);

    private static Setup GetAuthorizationContext<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, GetAuthorizationContextConfig, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<GetAuthorizationContextHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, GetAuthorizationContextConfig, bool> _predicate;
        private readonly GetAuthorizationContextHandler _handler;

        internal Setup(
            Func<GatewayContext, GetAuthorizationContextConfig, bool> predicate,
            GetAuthorizationContextHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, GetAuthorizationContextConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());

        public void WithAuthorizationProviderHook(Func<string, string, Authorization> hook) =>
            _handler.ProvideAuthorizationHooks.Add((_predicate, hook).ToTuple());

        public void ReturnsAuthorization(Authorization authorization) =>
            this.WithAuthorizationProviderHook((_, _) => authorization);

        public void WithError(string error) =>
            this.WithAuthorizationProviderHook((_, _) => throw new HttpRequestException(error));
    }
}
