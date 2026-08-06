// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class LlmContentSafetyTests
{
    class SimpleLlmContentSafety : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.LlmContentSafety(new LlmContentSafetyConfig
            {
                BackendId = "content-safety-backend",
                ShieldPrompt = true,
                Categories = new ContentSafetyCategories
                {
                    OutputType = "FourSeverityLevels",
                    Categories = [new ContentSafetyCategory { Name = "Hate", Threshold = 2 }]
                },
                BlockLists = new ContentSafetyBlockLists { Ids = ["blocklist-1"] }
            });
        }
    }

    [TestMethod]
    public void LlmContentSafety_Inbound_ShouldNotThrow()
    {
        var test = new SimpleLlmContentSafety().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void LlmContentSafety_Inbound_Callback()
    {
        var test = new SimpleLlmContentSafety().AsTestDocument();
        string? observedBackendId = null;

        test.SetupInbound().LlmContentSafety().WithCallback((context, config) =>
        {
            observedBackendId = config.BackendId;
            context.Variables["backend-id"] = config.BackendId;
        });

        test.RunInbound();

        observedBackendId.Should().Be("content-safety-backend");
        test.Context.Variables.Should().ContainKey("backend-id")
            .WhoseValue.Should().Be("content-safety-backend");
    }
}
