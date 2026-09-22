// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Expressions;

// Username stays the positional parameter, so existing test code that passes it by name, uses it in a with
// expression or deconstructs the record keeps compiling.
public record MockBasicAuthCredentials(
    [property: Obsolete("Use UserId, which matches the API Management expression API.")] string Username,
    string Password) : BasicAuthCredentials
{
#pragma warning disable CS0618
    public string UserId
    {
        get => Username;
        init => Username = value;
    }
#pragma warning restore CS0618
}