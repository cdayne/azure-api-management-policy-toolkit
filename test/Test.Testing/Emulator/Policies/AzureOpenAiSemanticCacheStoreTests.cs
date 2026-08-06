// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class AzureOpenAiSemanticCacheStoreTests
{
    class SimpleAzureOpenAiSemanticCacheStore : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.AzureOpenAiSemanticCacheStore(300);
        }
    }

    [TestMethod]
    public void AzureOpenAiSemanticCacheStore_Outbound_Callback()
    {
        var test = new SimpleAzureOpenAiSemanticCacheStore().AsTestDocument();
        uint? observedDuration = null;

        test.SetupOutbound().AzureOpenAiSemanticCacheStore().WithCallback((_, duration) =>
        {
            observedDuration = duration;
        });

        test.RunOutbound();

        observedDuration.Should().Be(300u);
    }
}
