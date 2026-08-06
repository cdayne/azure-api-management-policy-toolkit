// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockValidateHeadersProvider
{
    public static Setup ValidateHeaders(this MockPoliciesProvider<IOutboundContext> mock) =>
        ValidateHeaders(mock, (_, _) => true);

    public static Setup ValidateHeaders(this MockPoliciesProvider<IOnErrorContext> mock) =>
        ValidateHeaders(mock, (_, _) => true);

    public static Setup ValidateHeaders(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, ValidateHeadersConfig, bool> predicate
    ) => ValidateHeaders<IOutboundContext>(mock, predicate);

    public static Setup ValidateHeaders(
        this MockPoliciesProvider<IOnErrorContext> mock,
        Func<GatewayContext, ValidateHeadersConfig, bool> predicate
    ) => ValidateHeaders<IOnErrorContext>(mock, predicate);

    private static Setup ValidateHeaders<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, ValidateHeadersConfig, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<ValidateHeadersHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, ValidateHeadersConfig, bool> _predicate;
        private readonly ValidateHeadersHandler _handler;

        internal Setup(
            Func<GatewayContext, ValidateHeadersConfig, bool> predicate,
            ValidateHeadersHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, ValidateHeadersConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
