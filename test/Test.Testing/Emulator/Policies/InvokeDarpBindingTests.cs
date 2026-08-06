// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class InvokeDarpBindingTests
{
    class SimpleInvokeDarpBinding : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.InvokeDarpBinding(new InvokeDarpBindingConfig
            {
                Name = "inbound-binding",
                Operation = "create",
                IgnoreError = false,
                ResponseVariableName = "bindingResponse",
                Timeout = 10,
                Template = "liquid",
                ContentType = "application/json",
                MetaData = [new DarpMetaData { Key = "ttlInSeconds", Value = "60" }],
                Data = "{ \"value\": 1 }"
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.InvokeDarpBinding(new InvokeDarpBindingConfig { Name = "outbound-binding" });
        }

        public void OnError(IOnErrorContext context)
        {
            context.InvokeDarpBinding(new InvokeDarpBindingConfig { Name = "onerror-binding" });
        }
    }

    [TestMethod]
    public void InvokeDarpBinding_Inbound_ShouldNotThrow()
    {
        var test = new SimpleInvokeDarpBinding().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void InvokeDarpBinding_Outbound_ShouldNotThrow()
    {
        var test = new SimpleInvokeDarpBinding().AsTestDocument();

        test.RunOutbound();
    }

    [TestMethod]
    public void InvokeDarpBinding_OnError_ShouldNotThrow()
    {
        var test = new SimpleInvokeDarpBinding().AsTestDocument();

        test.RunOnError();
    }

    [TestMethod]
    public void InvokeDarpBinding_Inbound_Callback()
    {
        var test = new SimpleInvokeDarpBinding().AsTestDocument();
        string? observedName = null;

        test.SetupInbound().InvokeDarpBinding().WithCallback((context, config) =>
        {
            observedName = config.Name;
            context.Variables["binding-name"] = config.Name;
        });

        test.RunInbound();

        observedName.Should().Be("inbound-binding");
        test.Context.Variables.Should().ContainKey("binding-name")
            .WhoseValue.Should().Be("inbound-binding");
    }

    [TestMethod]
    public void InvokeDarpBinding_Outbound_Callback()
    {
        var test = new SimpleInvokeDarpBinding().AsTestDocument();
        var executedCallback = false;

        test.SetupOutbound().InvokeDarpBinding().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunOutbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void InvokeDarpBinding_OnError_Callback()
    {
        var test = new SimpleInvokeDarpBinding().AsTestDocument();
        var executedCallback = false;

        test.SetupOnError().InvokeDarpBinding().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunOnError();

        executedCallback.Should().BeTrue();
    }
}
