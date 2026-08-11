// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class AzureOpenAiTokenLimitTests
{
    class SimpleAzureOpenAiTokenLimit : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.AzureOpenAiTokenLimit(new TokenLimitConfig
            {
                CounterKey = "@(context.Subscription.Id)",
                EstimatePromptToken = true,
                TokensPerMinute = 1000,
                TokenQuota = 10000
            });
        }
    }

    [TestMethod]
    public void AzureOpenAiTokenLimit_Inbound_Callback()
    {
        var test = new SimpleAzureOpenAiTokenLimit().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound().AzureOpenAiTokenLimit().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }
}
