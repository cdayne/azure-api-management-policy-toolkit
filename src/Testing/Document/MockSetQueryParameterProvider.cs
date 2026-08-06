// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockSetQueryParameterProvider
{
    public static Setup SetQueryParameter<T>(this MockPoliciesProvider<T> mock) where T : class =>
        SetQueryParameter(mock, (_, _, _) => true);

    public static Setup SetQueryParameter<T>(
        this MockPoliciesProvider<T> mock,
        Func<GatewayContext, string, string[], bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<SetQueryParameterHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, string, string[], bool> _predicate;
        private readonly SetQueryParameterHandler _handler;

        internal Setup(
            Func<GatewayContext, string, string[], bool> predicate,
            SetQueryParameterHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, string, string[]> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
