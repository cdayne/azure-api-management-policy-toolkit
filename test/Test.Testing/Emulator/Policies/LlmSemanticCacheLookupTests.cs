// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class LlmSemanticCacheLookupTests
{
    class SimpleLlmSemanticCacheLookup : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.LlmSemanticCacheLookup(new SemanticCacheLookupConfig
            {
                ScoreThreshold = 0.8M,
                EmbeddingsBackendId = "embeddings-backend",
                EmbeddingsBackendAuth = "system-assigned",
                IgnoreSystemMessages = true,
                MaxMessageCount = 4,
                CacheId = "my-cache",
                VaryBy = ["@(context.Subscription.Id)"]
            });
        }
    }

    [TestMethod]
    public void LlmSemanticCacheLookup_Inbound_ShouldNotThrow()
    {
        var test = new SimpleLlmSemanticCacheLookup().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void LlmSemanticCacheLookup_Inbound_Callback()
    {
        var test = new SimpleLlmSemanticCacheLookup().AsTestDocument();
        string? observedBackendId = null;

        test.SetupInbound().LlmSemanticCacheLookup().WithCallback((context, config) =>
        {
            observedBackendId = config.EmbeddingsBackendId;
            context.Variables["backend-id"] = config.EmbeddingsBackendId;
        });

        test.RunInbound();

        observedBackendId.Should().Be("embeddings-backend");
        test.Context.Variables.Should().ContainKey("backend-id")
            .WhoseValue.Should().Be("embeddings-backend");
    }
}
