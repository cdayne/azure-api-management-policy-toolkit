// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class ProxyTests
{
    class SimpleProxy : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Proxy(new ProxyConfig
            {
                Url = "https://proxy.contoso.local:8443",
                Username = "contoso-user",
                Password = "contoso-password"
            });
        }
    }

    class MultiProxy : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Proxy(new ProxyConfig { Url = "https://proxy-a" });
            context.Proxy(new ProxyConfig { Url = "https://proxy-b" });
        }
    }

    [TestMethod]
    public void Proxy_Inbound_ShouldNotThrowOrMutateContext()
    {
        var test = new TestDocument(new SimpleProxy())
        {
            Context = { Request = { Url = { Path = "/original" } } }
        };

        test.RunInbound();

        test.Context.Request.Url.Path.Should().Be("/original");
    }

    [TestMethod]
    public void Proxy_Inbound_Callback()
    {
        var test = new SimpleProxy().AsTestDocument();
        string? observedUrl = null;

        test.SetupInbound().Proxy().WithCallback((context, config) =>
        {
            observedUrl = config.Url;
            context.Variables["proxy-url"] = config.Url;
        });

        test.RunInbound();

        observedUrl.Should().Be("https://proxy.contoso.local:8443");
        test.Context.Variables.Should().ContainKey("proxy-url")
            .WhoseValue.Should().Be("https://proxy.contoso.local:8443");
    }

    [TestMethod]
    public void Proxy_Inbound_PredicateCallback()
    {
        var test = new MultiProxy().AsTestDocument();
        var matchedCount = 0;

        test.SetupInbound()
            .Proxy((_, config) => config.Url == "https://proxy-b")
            .WithCallback((context, config) =>
            {
                matchedCount++;
                context.Variables["selected-proxy"] = config.Url;
            });

        test.RunInbound();

        matchedCount.Should().Be(1);
        test.Context.Variables.Should().ContainKey("selected-proxy")
            .WhoseValue.Should().Be("https://proxy-b");
    }
}
