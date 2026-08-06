// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Expressions;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class CacheLookupTests
{
    class SimpleCacheLookup : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.CacheLookup(new CacheLookupConfig
            {
                VaryByDeveloper = false,
                VaryByDeveloperGroups = false
            });
        }
    }

    class CacheLookupWithVaryBy : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.CacheLookup(new CacheLookupConfig
            {
                VaryByDeveloper = true,
                VaryByDeveloperGroups = true,
                CachingType = "internal",
                DownstreamCachingType = "public",
                MustRevalidate = true,
                AllowPrivateResponseCaching = true,
                VaryByHeaders = ["Accept"],
                VaryByQueryParameters = ["id"]
            });
        }
    }

    class CacheLookupThenSetHeader : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.CacheLookup(new CacheLookupConfig
            {
                VaryByDeveloper = false,
                VaryByDeveloperGroups = false
            });
            context.SetHeader("X-After-Cache-Lookup", "executed");
        }
    }

    class CacheLookupExternal : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.CacheLookup(new CacheLookupConfig
            {
                VaryByDeveloper = false,
                VaryByDeveloperGroups = false,
                CachingType = "external"
            });
        }
    }

    [TestMethod]
    public void CacheLookup_CacheMiss_WhenNothingCached()
    {
        var test = new SimpleCacheLookup().AsTestDocument();

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("__cache_hit").WhoseValue.Should().Be(false);
    }

    [TestMethod]
    public void CacheLookup_CacheHit_CopiesResponseFromPreferExternalStore()
    {
        var test = new SimpleCacheLookup().AsTestDocument();
        var cachedResponse = new MockResponse { StatusCode = 201, StatusReason = "Created" };
        cachedResponse.Body.Content = "cached-body";
        cachedResponse.Headers["X-Cache"] = ["hit"];
        var key = test.Context.Request.Url.ToString();
        test.SetupCacheStore().WithInternalCacheValue(key, cachedResponse);

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("__cache_hit").WhoseValue.Should().Be(true);
        test.Context.Response.StatusCode.Should().Be(201);
        test.Context.Response.StatusReason.Should().Be("Created");
        test.Context.Response.Body.Content.Should().Be("cached-body");
        test.Context.Response.Headers.Should().ContainKey("X-Cache");
    }

    [TestMethod]
    public void CacheLookup_BuildsCacheKeyIncludingVaryByHeadersAndQueryParameters()
    {
        var test = new CacheLookupWithVaryBy().AsTestDocument();
        test.Context.Request.Headers["Accept"] = ["application/json"];
        test.Context.Request.Url.Query["id"] = ["42"];
        var cachedResponse = new MockResponse { StatusCode = 200 };
        var expectedKey =
            $"{test.Context.Request.Url};h:Accept=application/json;q:id=42";
        test.SetupCacheStore().WithInternalCacheValue(expectedKey, cachedResponse);

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("__cache_hit").WhoseValue.Should().Be(true);
        test.Context.Variables.Should().ContainKey("__cache_lookup_key").WhoseValue.Should().Be(expectedKey);
    }

    [TestMethod]
    public void CacheLookup_ExternalCache_MissesWhenNotSetup()
    {
        var test = new CacheLookupExternal().AsTestDocument();
        var key = test.Context.Request.Url.ToString();
        test.SetupCacheStore().WithExternalCacheValue(key, new MockResponse());

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("__cache_hit").WhoseValue.Should().Be(false);
    }

    [TestMethod]
    public void CacheLookup_ExternalCache_HitsWhenSetup()
    {
        var test = new CacheLookupExternal().AsTestDocument();
        var key = test.Context.Request.Url.ToString();
        var cachedResponse = new MockResponse { StatusCode = 204 };
        test.SetupCacheStore().WithExternalCacheSetup().WithExternalCacheValue(key, cachedResponse);

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("__cache_hit").WhoseValue.Should().Be(true);
        test.Context.Response.StatusCode.Should().Be(204);
    }

    [TestMethod]
    public void CacheLookup_CacheHit_ShortCircuitsRestOfInboundSection()
    {
        var test = new CacheLookupThenSetHeader().AsTestDocument();
        var cachedResponse = new MockResponse { StatusCode = 200 };
        var key = test.Context.Request.Url.ToString();
        test.SetupCacheStore().WithInternalCacheValue(key, cachedResponse);
        var headerExecuted = false;
        test.SetupInbound().SetHeader().WithCallback((_, _, _) => headerExecuted = true);

        test.RunInbound();

        headerExecuted.Should().BeFalse();
        test.Context.Variables.Should().ContainKey("__cache_hit").WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void CacheLookup_ExpiredInternalCacheEntry_TreatedAsMiss()
    {
        var test = new SimpleCacheLookup().AsTestDocument();
        var cachedResponse = new MockResponse { StatusCode = 200 };
        var key = test.Context.Request.Url.ToString();
        test.SetupCacheStore().WithInternalCacheValue(key, cachedResponse, duration: 0);

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("__cache_hit").WhoseValue.Should().Be(false);
    }

    [TestMethod]
    public void CacheLookup_Callback()
    {
        var test = new SimpleCacheLookup().AsTestDocument();
        var executedCallback = false;
        test.SetupInbound().CacheLookup().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
        test.Context.Variables.Should().NotContainKey("__cache_hit");
    }
}
