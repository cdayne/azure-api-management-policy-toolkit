// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockValidateParametersProvider
{
    public static Setup ValidateParameters(this MockPoliciesProvider<IInboundContext> mock) =>
        ValidateParameters(mock, (_, _) => true);

    public static Setup ValidateParameters(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, ValidateParametersConfig, bool> predicate
    )
    {
        var handler = mock.SectionContextProxy.GetHandler<ValidateParametersHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, ValidateParametersConfig, bool> _predicate;
        private readonly ValidateParametersHandler _handler;

        internal Setup(
            Func<GatewayContext, ValidateParametersConfig, bool> predicate,
            ValidateParametersHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, ValidateParametersConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
