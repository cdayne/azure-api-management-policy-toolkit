// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Data;

/// <summary>
/// Queues mock responses for the forward-request policy. <c>ForwardRequestHandler</c> calls
/// <see cref="GetNext"/> once per invocation: a queued response (via <see cref="Returns"/>) is
/// consumed first, falling back to the default set via <see cref="ReturnsDefault"/> once the
/// queue is empty, and falling back to a real HTTP call if neither is set.
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

    /// <summary>Sets the response returned once the queue is empty.</summary>
    public ForwardRequestStore ReturnsDefault(MockBackendResponse response)
    {
        _defaultResponse = response;
        return this;
    }

    internal MockBackendResponse? GetNext() =>
        _responses.Count > 0 ? _responses.Dequeue() : _defaultResponse;
}
