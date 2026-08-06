// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockSetQueryParameterIfNotExistProvider
{
    public static Setup SetQueryParameterIfNotExist<T>(this MockPoliciesProvider<T> mock) where T : class =>
        SetQueryParameterIfNotExist(mock, (_, _, _) => true);

    public static Setup SetQueryParameterIfNotExist<T>(
        this MockPoliciesProvider<T> mock,
        Func<GatewayContext, string, string[], bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<SetQueryParameterIfNotExistHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, string, string[], bool> _predicate;
        private readonly SetQueryParameterIfNotExistHandler _handler;

        internal Setup(
            Func<GatewayContext, string, string[], bool> predicate,
            SetQueryParameterIfNotExistHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, string, string[]> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
