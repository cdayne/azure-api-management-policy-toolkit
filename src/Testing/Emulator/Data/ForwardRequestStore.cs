// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Data;

/// <summary>
/// Queues mock responses for the forward-request policy. Consulted by <c>ForwardRequestHandler</c>
/// only after its own per-call <c>ForwardRequest().Returns(...)</c>/<c>ReturnsDefault(...)</c> setup (on
/// <see cref="Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document.MockForwardRequestProvider.Setup"/>)
/// finds no match, so a handler-level default always takes precedence over anything queued here.
/// </summary>
public class ForwardRequestStore
{
    private readonly Queue<MockBackendResponse> _responses = new();
    private MockBackendResponse? _defaultResponse;

    /// <summary>Enqueues a response to be returned, in order, by successive ForwardRequest calls.</summary>
    public ForwardRequestStore Returns(MockBackendResponse response)
    {
        _responses.Enqueue(response);
        return this;
    }

    /// <summary>
    /// Sets the response returned once the queue is empty. Ignored if
    /// <c>ForwardRequest().ReturnsDefault(...)</c> was also set — that one wins.
    /// </summary>
    public ForwardRequestStore ReturnsDefault(MockBackendResponse response)
    {
        _defaultResponse = response;
        return this;
    }

    internal MockBackendResponse? GetNext() =>
        _responses.Count > 0 ? _responses.Dequeue() : _defaultResponse;
}
