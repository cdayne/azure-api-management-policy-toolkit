// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class LlmTokenLimitTests
{
    class SimpleLlmTokenLimit : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.LlmTokenLimit(new TokenLimitConfig
            {
                CounterKey = "@(context.Subscription.Id)",
                EstimatePromptToken = true,
                TokensPerMinute = 1000,
                TokenQuota = 10000,
                TokenQuotaPeriod = "Month",
                RetryAfterHeaderName = "Retry-After",
                RetryAfterVariableName = "retryAfter",
                RemainingQuotaTokensHeaderName = "Remaining-Quota-Tokens",
                RemainingQuotaTokensVariableName = "remainingQuotaTokens",
                RemainingTokensHeaderName = "Remaining-Tokens",
                RemainingTokensVariableName = "remainingTokens",
                TokensConsumedHeaderName = "Tokens-Consumed",
                TokensConsumedVariableName = "tokensConsumed"
            });
        }
    }

    [TestMethod]
    public void LlmTokenLimit_Inbound_ShouldNotThrow()
    {
        var test = new SimpleLlmTokenLimit().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void LlmTokenLimit_Inbound_Callback()
    {
        var test = new SimpleLlmTokenLimit().AsTestDocument();
        string? observedCounterKey = null;

        test.SetupInbound().LlmTokenLimit().WithCallback((context, config) =>
        {
            observedCounterKey = config.CounterKey;
            context.Variables["counter-key"] = config.CounterKey;
        });

        test.RunInbound();

        observedCounterKey.Should().Be("@(context.Subscription.Id)");
        test.Context.Variables.Should().ContainKey("counter-key")
            .WhoseValue.Should().Be("@(context.Subscription.Id)");
    }
}
