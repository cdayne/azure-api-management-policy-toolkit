// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class LlmSemanticCacheStoreTests
{
    class SimpleLlmSemanticCacheStore : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.LlmSemanticCacheStore(300);
        }
    }

    [TestMethod]
    public void LlmSemanticCacheStore_Outbound_ShouldNotThrow()
    {
        var test = new SimpleLlmSemanticCacheStore().AsTestDocument();

        test.RunOutbound();
    }

    [TestMethod]
    public void LlmSemanticCacheStore_Outbound_Callback()
    {
        var test = new SimpleLlmSemanticCacheStore().AsTestDocument();
        uint? observedDuration = null;

        test.SetupOutbound().LlmSemanticCacheStore().WithCallback((context, duration) =>
        {
            observedDuration = duration;
            context.Variables["cache-duration"] = duration;
        });

        test.RunOutbound();

        observedDuration.Should().Be(300u);
        test.Context.Variables.Should().ContainKey("cache-duration")
            .WhoseValue.Should().Be(300u);
    }
}
