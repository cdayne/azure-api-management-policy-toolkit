// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Expressions;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Services;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

[Section(nameof(IInboundContext))]
internal class CacheLookupHandler : PolicyHandler<CacheLookupConfig>
{
    public override string PolicyName => nameof(IInboundContext.CacheLookup);

    protected override void Handle(GatewayContext context, CacheLookupConfig config)
    {
        context.CacheInfo.Capture(config);
        var key = context.CacheInfo.BuildCacheKey(context.Request);
        context.Variables["__cache_lookup_key"] = key;

        var cache = context.Services.Resolve<ICache>();
        if (cache is not null)
        {
            var cachedValue = cache.GetAsync(key).GetAwaiter().GetResult();
            if (ResponseUtilities.TryCopyCachedResponse(cachedValue, context.Response))
            {
                context.Variables["__cache_hit"] = true;
                throw new FinishSectionProcessingException();
            }

            context.Variables["__cache_hit"] = false;
            return;
        }

        var store = context.CacheStore.GetCache(context.CacheInfo.CachingType);
        if (store is null)
        {
            context.Variables["__cache_hit"] = false;
            return;
        }

        if (store.TryGetValue(key, out var cachedEntry) && !cachedEntry.IsExpired && cachedEntry.Value is MockResponse cachedResponse)
        {
            ResponseUtilities.Copy(cachedResponse, context.Response);
            context.Variables["__cache_hit"] = true;
            throw new FinishSectionProcessingException();
        }

        context.Variables["__cache_hit"] = false;
    }
}
