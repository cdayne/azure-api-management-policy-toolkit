// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Data;

/// <summary>
/// A single trace entry recorded by the trace policy.
/// </summary>
public record TraceEntry(string Source, string Message, string? Severity, TraceMetadata[]? Metadata);

/// <summary>
/// Store of trace entries recorded by the trace policy during a test run.
/// </summary>
public class TraceStore
{
    private readonly List<TraceEntry> _entries = new();

    public IReadOnlyList<TraceEntry> Entries => _entries;

    internal void Add(TraceEntry entry) => _entries.Add(entry);
}
