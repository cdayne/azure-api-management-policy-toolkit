// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class IncludeFragmentTests
{
    class SimpleFragment : IFragment
    {
        public void Fragment(IFragmentContext context)
        {
            context.SetVariable("fragment-executed", true);
        }
    }

    class InboundIncludeFragment : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.IncludeFragment("my-fragment");
        }
    }

    class BackendIncludeFragment : IDocument
    {
        public void Backend(IBackendContext context)
        {
            context.IncludeFragment("my-fragment");
        }
    }

    class OutboundIncludeFragment : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.IncludeFragment("my-fragment");
        }
    }

    class OnErrorIncludeFragment : IDocument
    {
        public void OnError(IOnErrorContext context)
        {
            context.IncludeFragment("my-fragment");
        }
    }

    class UnregisteredIncludeFragment : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.IncludeFragment("does-not-exist");
        }
    }

    [TestMethod]
    public void IncludeFragment_ExecutesRegisteredFragment_InInboundSection()
    {
        var test = new InboundIncludeFragment().AsTestDocument();
        test.RegisterFragment("my-fragment", new SimpleFragment());

        test.RunInbound();

        test.Context.Variables.Should().ContainKey("fragment-executed").WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void IncludeFragment_ExecutesRegisteredFragment_InBackendSection()
    {
        var test = new BackendIncludeFragment().AsTestDocument();
        test.RegisterFragment("my-fragment", new SimpleFragment());

        test.RunBackend();

        test.Context.Variables.Should().ContainKey("fragment-executed").WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void IncludeFragment_ExecutesRegisteredFragment_InOutboundSection()
    {
        var test = new OutboundIncludeFragment().AsTestDocument();
        test.RegisterFragment("my-fragment", new SimpleFragment());

        test.RunOutbound();

        test.Context.Variables.Should().ContainKey("fragment-executed").WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void IncludeFragment_ExecutesRegisteredFragment_InOnErrorSection()
    {
        var test = new OnErrorIncludeFragment().AsTestDocument();
        test.RegisterFragment("my-fragment", new SimpleFragment());

        test.RunOnError();

        test.Context.Variables.Should().ContainKey("fragment-executed").WhoseValue.Should().Be(true);
    }

    [TestMethod]
    public void IncludeFragment_ThrowsWhenFragmentIsNotRegisteredOrDiscoverable()
    {
        var test = new UnregisteredIncludeFragment().AsTestDocument();

        var ex = Assert.ThrowsException<PolicyException>(() => test.RunInbound());

        ex.Policy.Should().Be("IncludeFragment");
    }

    [TestMethod]
    public void IncludeFragment_Callback_ReplacesDefaultFragmentExecution()
    {
        var test = new InboundIncludeFragment().AsTestDocument();
        test.RegisterFragment("my-fragment", new SimpleFragment());
        string? callbackFragmentId = null;
        test.SetupInbound().IncludeFragment().WithCallback((_, fragmentId) =>
        {
            callbackFragmentId = fragmentId;
        });

        test.RunInbound();

        callbackFragmentId.Should().Be("my-fragment");
        test.Context.Variables.Should().NotContainKey("fragment-executed");
    }
}
