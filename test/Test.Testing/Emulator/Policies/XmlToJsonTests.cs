// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class XmlToJsonTests
{
    class SimpleXmlToJson : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.XmlToJson(new XmlToJsonConfig { Kind = "javascript-friendly", Apply = "always" });
        }

        public void Backend(IBackendContext context)
        {
            context.XmlToJson(new XmlToJsonConfig { Kind = "javascript-friendly", Apply = "always" });
        }

        public void Outbound(IOutboundContext context)
        {
            context.XmlToJson(new XmlToJsonConfig { Kind = "javascript-friendly", Apply = "always" });
        }

        public void OnError(IOnErrorContext context)
        {
            context.XmlToJson(new XmlToJsonConfig { Kind = "javascript-friendly", Apply = "always" });
        }
    }

    class WithOptionalFieldsXmlToJson : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.XmlToJson(new XmlToJsonConfig
            {
                Kind = "javascript-friendly",
                Apply = "content-type-xml",
                ConsiderAcceptHeader = false,
                AlwaysArrayChildElements = true
            });
        }
    }

    [TestMethod]
    public void XmlToJson_Inbound_ShouldNotThrow()
    {
        var test = new SimpleXmlToJson().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void XmlToJson_Outbound_ShouldNotThrow()
    {
        var test = new SimpleXmlToJson().AsTestDocument();

        test.RunOutbound();
    }

    [TestMethod]
    public void XmlToJson_Backend_ShouldNotThrow()
    {
        var test = new SimpleXmlToJson().AsTestDocument();

        test.RunBackend();
    }

    [TestMethod]
    public void XmlToJson_OnError_ShouldNotThrow()
    {
        var test = new SimpleXmlToJson().AsTestDocument();

        test.RunOnError();
    }

    [TestMethod]
    public void XmlToJson_WithOptionalFields_ShouldNotThrow()
    {
        var test = new WithOptionalFieldsXmlToJson().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void XmlToJson_Inbound_Callback()
    {
        var test = new SimpleXmlToJson().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound().XmlToJson().WithCallback((context, config) =>
        {
            executedCallback = true;
            context.Variables["kind"] = config.Kind;
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
        test.Context.Variables.Should().ContainKey("kind")
            .WhoseValue.Should().Be("javascript-friendly");
    }

    [TestMethod]
    public void XmlToJson_Inbound_CallbackWithPredicate()
    {
        var test = new SimpleXmlToJson().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound()
            .XmlToJson((_, config) => config.Apply == "always")
            .WithCallback((_, _) => executedCallback = true);

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }
}
