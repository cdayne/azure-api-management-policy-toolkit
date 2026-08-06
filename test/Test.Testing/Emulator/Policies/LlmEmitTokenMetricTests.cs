// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class LlmEmitTokenMetricTests
{
    class SimpleLlmEmitTokenMetric : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.LlmEmitTokenMetric(new EmitTokenMetricConfig
            {
                Namespace = "llm-namespace",
                Dimensions = [new MetricDimensionConfig { Name = "ApiId", Value = "my-api" }]
            });
        }
    }

    class MultiLlmEmitTokenMetric : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.LlmEmitTokenMetric(new EmitTokenMetricConfig
            {
                Namespace = "namespace-a",
                Dimensions = [new MetricDimensionConfig { Name = "kind", Value = "a" }]
            });
            context.LlmEmitTokenMetric(new EmitTokenMetricConfig
            {
                Namespace = "namespace-b",
                Dimensions = [new MetricDimensionConfig { Name = "kind", Value = "b" }]
            });
        }
    }

    [TestMethod]
    public void LlmEmitTokenMetric_Inbound_ShouldNotThrow()
    {
        var test = new SimpleLlmEmitTokenMetric().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void LlmEmitTokenMetric_Inbound_Callback()
    {
        var test = new SimpleLlmEmitTokenMetric().AsTestDocument();
        string? observedNamespace = null;

        test.SetupInbound().LlmEmitTokenMetric().WithCallback((context, config) =>
        {
            observedNamespace = config.Namespace;
            context.Variables["metric-namespace"] = config.Namespace;
        });

        test.RunInbound();

        observedNamespace.Should().Be("llm-namespace");
        test.Context.Variables.Should().ContainKey("metric-namespace")
            .WhoseValue.Should().Be("llm-namespace");
    }

    [TestMethod]
    public void LlmEmitTokenMetric_Inbound_PredicateCallback()
    {
        var test = new MultiLlmEmitTokenMetric().AsTestDocument();
        var matchedCount = 0;

        test.SetupInbound()
            .LlmEmitTokenMetric((_, config) => config.Namespace == "namespace-b")
            .WithCallback((context, config) =>
            {
                matchedCount++;
                context.Variables["matched-namespace"] = config.Namespace;
            });

        test.RunInbound();

        matchedCount.Should().Be(1);
        test.Context.Variables.Should().ContainKey("matched-namespace")
            .WhoseValue.Should().Be("namespace-b");
    }
}
