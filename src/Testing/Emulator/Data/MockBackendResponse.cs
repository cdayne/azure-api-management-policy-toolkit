// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Data;

public class MockBackendResponse
{
    public int StatusCode { get; init; } = 200;
    public string StatusReason { get; init; } = "OK";
    public string? Body { get; init; }
    public Dictionary<string, string[]> Headers { get; init; } = new();
}
