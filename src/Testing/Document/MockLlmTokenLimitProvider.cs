// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockLlmTokenLimitProvider
{
    public static Setup LlmTokenLimit(
        this MockPoliciesProvider<IInboundContext> mock) => LlmTokenLimit(mock, (_, _) => true);

    public static Setup LlmTokenLimit(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, TokenLimitConfig, bool> predicate)
    {
        var handler = mock.SectionContextProxy.GetHandler<LlmTokenLimitHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, TokenLimitConfig, bool> _predicate;
        private readonly LlmTokenLimitHandler _handler;

        internal Setup(
            Func<GatewayContext, TokenLimitConfig, bool> predicate,
            LlmTokenLimitHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, TokenLimitConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
