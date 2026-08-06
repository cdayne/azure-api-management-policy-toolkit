// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Expressions;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Data;

/// <summary>
/// Carries the caching settings established by a <c>cache-lookup</c> call so a later
/// <c>cache-store</c> call in the same request reuses the same caching type and vary-by
/// key derivation. Defaults match cache-store's own defaults when no cache-lookup ran.
/// </summary>
internal class CacheInfo
{
    public string CachingType { get; private set; } = "prefer-external";

    public string[]? VaryByHeaders { get; private set; }

    public string[]? VaryByQueryParameters { get; private set; }

    public bool AllowPrivateResponseCaching { get; private set; }

    public void Capture(CacheLookupConfig config)
    {
        CachingType = config.CachingType ?? "prefer-external";
        VaryByHeaders = config.VaryByHeaders;
        VaryByQueryParameters = config.VaryByQueryParameters;
        AllowPrivateResponseCaching = config.AllowPrivateResponseCaching ?? false;
    }

    public string BuildCacheKey(MockRequest request)
    {
        var sb = new StringBuilder();
        sb.Append(request.Url);

        if (VaryByHeaders is not null)
        {
            foreach (var header in VaryByHeaders)
            {
                if (request.Headers.TryGetValue(header, out var values))
                {
                    sb.Append($";h:{header}={string.Join(",", values)}");
                }
            }
        }

        if (VaryByQueryParameters is not null)
        {
            foreach (var param in VaryByQueryParameters)
            {
                if (request.Url.Query.TryGetValue(param, out var values))
                {
                    sb.Append($";q:{param}={string.Join(",", values)}");
                }
            }
        }

        return sb.ToString();
    }
}
