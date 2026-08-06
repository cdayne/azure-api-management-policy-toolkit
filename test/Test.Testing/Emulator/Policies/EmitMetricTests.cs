// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class EmitMetricTests
{
    class SimpleEmitMetric : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.EmitMetric(new EmitMetricConfig
            {
                Name = "inbound-metric",
                Namespace = "apim-tests",
                Value = 1,
                Dimensions = [new MetricDimensionConfig { Name = "source", Value = "inbound" }]
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.EmitMetric(new EmitMetricConfig
            {
                Name = "outbound-metric",
                Value = 3,
                Dimensions = [new MetricDimensionConfig { Name = "source", Value = "outbound" }]
            });
        }

        public void OnError(IOnErrorContext context)
        {
            context.EmitMetric(new EmitMetricConfig
            {
                Name = "onerror-metric",
                Value = 4,
                Dimensions = [new MetricDimensionConfig { Name = "source", Value = "onerror" }]
            });
        }
    }

    class MultiEmitMetric : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.EmitMetric(new EmitMetricConfig
            {
                Name = "metric-a",
                Value = 10,
                Dimensions = [new MetricDimensionConfig { Name = "kind", Value = "a" }]
            });
            context.EmitMetric(new EmitMetricConfig
            {
                Name = "metric-b",
                Value = 20,
                Dimensions = [new MetricDimensionConfig { Name = "kind", Value = "b" }]
            });
        }
    }

    class EmitMetricWithoutValue : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.EmitMetric(new EmitMetricConfig
            {
                Name = "no-value-metric",
                Dimensions = [new MetricDimensionConfig { Name = "source", Value = "inbound" }]
            });
        }
    }

    [TestMethod]
    public void EmitMetric_Inbound_ShouldNotThrow()
    {
        var test = new SimpleEmitMetric().AsTestDocument();

        test.RunInbound();

        test.SetupMetricStore().Metrics.Should().ContainSingle()
            .Which.Name.Should().Be("inbound-metric");
    }

    [TestMethod]
    public void EmitMetric_Outbound_RecordsMetric()
    {
        var test = new SimpleEmitMetric().AsTestDocument();

        test.RunOutbound();

        test.SetupMetricStore().Metrics.Should().ContainSingle()
            .Which.Name.Should().Be("outbound-metric");
        test.SetupMetricStore().Metrics[0].Value.Should().Be(3);
    }

    [TestMethod]
    public void EmitMetric_OnError_RecordsMetric()
    {
        var test = new SimpleEmitMetric().AsTestDocument();

        test.RunOnError();

        test.SetupMetricStore().Metrics.Should().ContainSingle()
            .Which.Name.Should().Be("onerror-metric");
        test.SetupMetricStore().Metrics[0].Value.Should().Be(4);
    }

    [TestMethod]
    public void EmitMetric_WithoutValue_DefaultsToOne()
    {
        var test = new EmitMetricWithoutValue().AsTestDocument();

        test.RunInbound();

        test.SetupMetricStore().Metrics.Should().ContainSingle()
            .Which.Value.Should().Be(1);
    }

    [TestMethod]
    public void EmitMetric_MultipleMetricsCollected()
    {
        var test = new MultiEmitMetric().AsTestDocument();

        test.RunInbound();

        var metrics = test.SetupMetricStore().Metrics;
        metrics.Should().HaveCount(2);
        metrics[0].Name.Should().Be("metric-a");
        metrics[0].Value.Should().Be(10);
        metrics[1].Name.Should().Be("metric-b");
        metrics[1].Value.Should().Be(20);
    }

    [TestMethod]
    public void EmitMetric_Inbound_Callback()
    {
        var test = new SimpleEmitMetric().AsTestDocument();
        string? observedName = null;

        test.SetupInbound().EmitMetric().WithCallback((context, config) =>
        {
            observedName = config.Name;
            context.Variables["metric-name"] = config.Name;
        });

        test.RunInbound();

        observedName.Should().Be("inbound-metric");
        test.Context.Variables.Should().ContainKey("metric-name")
            .WhoseValue.Should().Be("inbound-metric");
        test.SetupMetricStore().Metrics.Should().BeEmpty();
    }

    [TestMethod]
    public void EmitMetric_Inbound_PredicateCallback()
    {
        var test = new MultiEmitMetric().AsTestDocument();
        var matchedCount = 0;

        test.SetupInbound()
            .EmitMetric((_, config) => config.Name == "metric-b")
            .WithCallback((context, config) =>
            {
                matchedCount++;
                context.Variables["metric-b-value"] = config.Value;
            });

        test.RunInbound();

        matchedCount.Should().Be(1);
        test.Context.Variables.Should().ContainKey("metric-b-value")
            .WhoseValue.Should().Be(20d);
    }
}
