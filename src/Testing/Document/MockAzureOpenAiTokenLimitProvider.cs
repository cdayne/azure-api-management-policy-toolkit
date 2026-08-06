// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockAzureOpenAiTokenLimitProvider
{
    public static Setup AzureOpenAiTokenLimit(
        this MockPoliciesProvider<IInboundContext> mock) => AzureOpenAiTokenLimit(mock, (_, _) => true);

    public static Setup AzureOpenAiTokenLimit(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, TokenLimitConfig, bool> predicate)
    {
        var handler = mock.SectionContextProxy.GetHandler<AzureOpenAiTokenLimitHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, TokenLimitConfig, bool> _predicate;
        private readonly AzureOpenAiTokenLimitHandler _handler;

        internal Setup(
            Func<GatewayContext, TokenLimitConfig, bool> predicate,
            AzureOpenAiTokenLimitHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, TokenLimitConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
