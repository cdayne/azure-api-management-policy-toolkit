// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockValidateStatusCodeProvider
{
    public static Setup ValidateStatusCode(this MockPoliciesProvider<IOutboundContext> mock) =>
        ValidateStatusCode(mock, (_, _) => true);

    public static Setup ValidateStatusCode(this MockPoliciesProvider<IOnErrorContext> mock) =>
        ValidateStatusCode(mock, (_, _) => true);

    public static Setup ValidateStatusCode(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, ValidateStatusCodeConfig, bool> predicate
    ) => ValidateStatusCode<IOutboundContext>(mock, predicate);

    public static Setup ValidateStatusCode(
        this MockPoliciesProvider<IOnErrorContext> mock,
        Func<GatewayContext, ValidateStatusCodeConfig, bool> predicate
    ) => ValidateStatusCode<IOnErrorContext>(mock, predicate);

    private static Setup ValidateStatusCode<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, ValidateStatusCodeConfig, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<ValidateStatusCodeHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, ValidateStatusCodeConfig, bool> _predicate;
        private readonly ValidateStatusCodeHandler _handler;

        internal Setup(
            Func<GatewayContext, ValidateStatusCodeConfig, bool> predicate,
            ValidateStatusCodeHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, ValidateStatusCodeConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
