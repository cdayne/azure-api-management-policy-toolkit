// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockEmitMetricProvider
{
    public static Setup EmitMetric(this MockPoliciesProvider<IInboundContext> mock) =>
        EmitMetric(mock, (_, _) => true);

    public static Setup EmitMetric(this MockPoliciesProvider<IOutboundContext> mock) =>
        EmitMetric(mock, (_, _) => true);

    public static Setup EmitMetric(this MockPoliciesProvider<IOnErrorContext> mock) =>
        EmitMetric(mock, (_, _) => true);

    public static Setup EmitMetric(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, EmitMetricConfig, bool> predicate
    ) => EmitMetric<IInboundContext>(mock, predicate);

    public static Setup EmitMetric(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, EmitMetricConfig, bool> predicate
    ) => EmitMetric<IOutboundContext>(mock, predicate);

    public static Setup EmitMetric(
        this MockPoliciesProvider<IOnErrorContext> mock,
        Func<GatewayContext, EmitMetricConfig, bool> predicate
    ) => EmitMetric<IOnErrorContext>(mock, predicate);

    private static Setup EmitMetric<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, EmitMetricConfig, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<EmitMetricHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, EmitMetricConfig, bool> _predicate;
        private readonly EmitMetricHandler _handler;

        internal Setup(
            Func<GatewayContext, EmitMetricConfig, bool> predicate,
            EmitMetricHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, EmitMetricConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
