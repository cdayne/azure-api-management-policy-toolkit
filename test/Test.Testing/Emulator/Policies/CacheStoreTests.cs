// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Expressions;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class CacheStoreTests
{
    class SimpleCacheStore : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.CacheStore(60, true);
        }
    }

    class CacheStoreWithoutCacheResponse : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.CacheStore(60, null);
        }
    }

    class CacheStoreDoNotCacheResponse : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.CacheStore(60, false);
        }
    }

    class VaryByCacheDocument : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.CacheLookup(new CacheLookupConfig
            {
                VaryByDeveloper = false,
                VaryByDeveloperGroups = false,
                VaryByHeaders = ["Accept"],
                VaryByQueryParameters = ["id"]
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.CacheStore(60, true);
        }
    }

    class InternalCachingTypeDocument : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.CacheLookup(new CacheLookupConfig
            {
                VaryByDeveloper = false,
                VaryByDeveloperGroups = false,
                CachingType = "internal"
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.CacheStore(60, true);
        }
    }

    class CacheStoreWithAllowPrivateCaching : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.CacheLookup(new CacheLookupConfig
            {
                VaryByDeveloper = false,
                VaryByDeveloperGroups = false,
                AllowPrivateResponseCaching = true
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.CacheStore(60, true);
        }
    }

    [TestMethod]
    public void CacheStore_StoresResponseInPreferExternalStore()
    {
        var test = new SimpleCacheStore().AsTestDocument();
        test.Context.Response.StatusCode = 200;
        test.Context.Response.Body.Content = "response-body";
        var key = test.Context.Request.Url.ToString();

        test.RunOutbound();

        var cacheValue = test.SetupCacheStore().InternalCache.Should().ContainKey(key).WhoseValue;
        cacheValue.Value.Should().NotBeSameAs(test.Context.Response, "the cached entry should be a snapshot, not a live reference");
        var cachedResponse = cacheValue.Value.Should().BeOfType<MockResponse>().Which;
        cachedResponse.StatusCode.Should().Be(200);
        cachedResponse.Body.Content.Should().Be("response-body");
        cacheValue.Ttl.TotalSeconds.Should().Be(60);
    }

    [TestMethod]
    public void CacheStore_WithNullCacheResponseArgument_DefaultsToStoringResponse()
    {
        var test = new CacheStoreWithoutCacheResponse().AsTestDocument();
        var key = test.Context.Request.Url.ToString();

        test.RunOutbound();

        test.SetupCacheStore().InternalCache.Should().ContainKey(key);
    }

    [TestMethod]
    public void CacheStore_CacheResponseFalse_DoesNotStore()
    {
        var test = new CacheStoreDoNotCacheResponse().AsTestDocument();
        var key = test.Context.Request.Url.ToString();

        test.RunOutbound();

        test.SetupCacheStore().InternalCache.Should().NotContainKey(key);
        test.SetupCacheStore().ExternalCache.Should().NotContainKey(key);
    }

    [TestMethod]
    public void CacheStore_WithExternalCacheSetup_StoresInExternalCache()
    {
        var test = new SimpleCacheStore().AsTestDocument();
        test.SetupCacheStore().WithExternalCacheSetup();
        var key = test.Context.Request.Url.ToString();

        test.RunOutbound();

        test.SetupCacheStore().ExternalCache.Should().ContainKey(key);
        test.SetupCacheStore().InternalCache.Should().NotContainKey(key);
    }

    [TestMethod]
    public void CacheStore_Callback()
    {
        var test = new SimpleCacheStore().AsTestDocument();
        var executedCallback = false;
        test.SetupOutbound().CacheStore().WithCallback((_, _, _) =>
        {
            executedCallback = true;
        });

        test.RunOutbound();

        executedCallback.Should().BeTrue();
        var key = test.Context.Request.Url.ToString();
        test.SetupCacheStore().InternalCache.Should().NotContainKey(key);
    }

    [TestMethod]
    public void CacheStore_UsesSameCacheKeyAsPrecedingCacheLookup()
    {
        var test = new VaryByCacheDocument().AsTestDocument();
        test.Context.Request.Headers["Accept"] = ["application/json"];
        test.Context.Request.Url.Query["id"] = ["42"];

        test.RunInbound();
        var expectedKey = (string)test.Context.Variables["__cache_lookup_key"]!;

        test.RunOutbound();

        test.SetupCacheStore().InternalCache.Should().ContainKey(expectedKey);
    }

    [TestMethod]
    public void CacheStore_UsesCachingTypeFromPrecedingCacheLookup()
    {
        var test = new InternalCachingTypeDocument().AsTestDocument();
        test.SetupCacheStore().WithExternalCacheSetup();
        var key = test.Context.Request.Url.ToString();

        test.RunInbound();
        test.RunOutbound();

        test.SetupCacheStore().InternalCache.Should().ContainKey(key);
        test.SetupCacheStore().ExternalCache.Should().NotContainKey(key);
    }

    [TestMethod]
    public void CacheStore_NonGetRequest_DoesNotStore()
    {
        var test = new SimpleCacheStore().AsTestDocument();
        test.Context.Request.Method = "POST";
        var key = test.Context.Request.Url.ToString();

        test.RunOutbound();

        test.SetupCacheStore().InternalCache.Should().NotContainKey(key);
    }

    [TestMethod]
    public void CacheStore_RequestWithAuthorizationHeader_DoesNotStore()
    {
        var test = new SimpleCacheStore().AsTestDocument();
        test.Context.Request.Headers["Authorization"] = ["Bearer token"];
        var key = test.Context.Request.Url.ToString();

        test.RunOutbound();

        test.SetupCacheStore().InternalCache.Should().NotContainKey(key);
    }

    [TestMethod]
    public void CacheStore_RequestWithAuthorizationHeader_Stores_WhenPrivateCachingAllowed()
    {
        var test = new CacheStoreWithAllowPrivateCaching().AsTestDocument();
        test.Context.Request.Headers["Authorization"] = ["Bearer token"];
        var key = test.Context.Request.Url.ToString();

        test.RunInbound();
        test.RunOutbound();

        test.SetupCacheStore().InternalCache.Should().ContainKey(key);
    }

    [TestMethod]
    public void CacheStore_LaterResponseMutation_DoesNotAffectCachedEntry()
    {
        var test = new SimpleCacheStore().AsTestDocument();
        test.Context.Response.StatusCode = 200;
        test.Context.Response.Body.Content = "original-body";
        var key = test.Context.Request.Url.ToString();

        test.RunOutbound();
        test.Context.Response.StatusCode = 500;
        test.Context.Response.Body.Content = "mutated-body";

        var cachedResponse = test.SetupCacheStore().InternalCache[key].Value.Should().BeOfType<MockResponse>().Which;
        cachedResponse.StatusCode.Should().Be(200);
        cachedResponse.Body.Content.Should().Be("original-body");
    }
}
