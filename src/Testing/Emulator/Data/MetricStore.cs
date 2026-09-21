// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Data;

public class MetricStore
{
    private readonly List<EmittedMetric> _metrics = new();

    public ImmutableArray<EmittedMetric> Metrics => _metrics.ToImmutableArray();

    internal void Add(EmittedMetric metric) => _metrics.Add(metric);
}
