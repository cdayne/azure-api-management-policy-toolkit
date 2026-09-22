// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;

public interface BasicAuthCredentials
{
    // Defaults to Username so implementations written before UserId existed keep compiling.
#pragma warning disable CS0618
    public string UserId => Username;
#pragma warning restore CS0618

    [Obsolete("Use UserId, which matches the API Management expression API.")]
    public string Username { get; }

    public string Password { get; }
}