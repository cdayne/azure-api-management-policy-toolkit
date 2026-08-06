// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockWaitProvider
{
    public static Setup Wait<T>(this MockPoliciesProvider<T> mock) where T : class =>
        Wait(mock, (_, _, _) => true);

    public static Setup Wait<T>(
        this MockPoliciesProvider<T> mock,
        Func<GatewayContext, Action, string?, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<WaitHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, Action, string?, bool> _predicate;
        private readonly WaitHandler _handler;

        internal Setup(
            Func<GatewayContext, Action, string?, bool> predicate,
            WaitHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, Action, string?> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
