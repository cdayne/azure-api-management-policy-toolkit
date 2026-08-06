// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class CrossDomainTests
{
    class SimpleCrossDomain : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.CrossDomain("allow-origin");
        }
    }

    class MultiCrossDomain : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.CrossDomain("allow-origin");
            context.CrossDomain("allow-methods");
        }
    }

    [TestMethod]
    public void CrossDomain_Inbound_ShouldNotThrowOrMutateContext()
    {
        var test = new TestDocument(new SimpleCrossDomain())
        {
            Context = { Request = { Url = { Path = "/before" } } }
        };

        test.RunInbound();

        test.Context.Request.Url.Path.Should().Be("/before");
    }

    [TestMethod]
    public void CrossDomain_Inbound_Callback()
    {
        var test = new SimpleCrossDomain().AsTestDocument();
        string? observedPolicy = null;

        test.SetupInbound().CrossDomain().WithCallback((context, policy) =>
        {
            observedPolicy = policy;
            context.Variables["cross-domain"] = policy;
        });

        test.RunInbound();

        observedPolicy.Should().Be("allow-origin");
        test.Context.Variables.Should().ContainKey("cross-domain")
            .WhoseValue.Should().Be("allow-origin");
    }

    [TestMethod]
    public void CrossDomain_Inbound_PredicateCallback()
    {
        var test = new MultiCrossDomain().AsTestDocument();
        var callbackCount = 0;

        test.SetupInbound()
            .CrossDomain((_, policy) => policy == "allow-methods")
            .WithCallback((context, policy) =>
            {
                callbackCount++;
                context.Variables["selected-policy"] = policy;
            });

        test.RunInbound();

        callbackCount.Should().Be(1);
        test.Context.Variables.Should().ContainKey("selected-policy")
            .WhoseValue.Should().Be("allow-methods");
    }
}
