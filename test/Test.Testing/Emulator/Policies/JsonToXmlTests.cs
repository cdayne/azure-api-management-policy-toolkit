// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class JsonToXmlTests
{
    class SimpleJsonToXml : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.JsonToXml(new JsonToXmlConfig { Apply = "always" });
        }

        public void Backend(IBackendContext context)
        {
            context.JsonToXml(new JsonToXmlConfig { Apply = "content-type-json" });
        }

        public void Outbound(IOutboundContext context)
        {
            context.JsonToXml(new JsonToXmlConfig { Apply = "always" });
        }

        public void OnError(IOnErrorContext context)
        {
            context.JsonToXml(new JsonToXmlConfig { Apply = "always" });
        }
    }

    class MultiJsonToXml : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.JsonToXml(new JsonToXmlConfig { Apply = "always" });
            context.JsonToXml(new JsonToXmlConfig { Apply = "content-type-json" });
        }
    }

    [TestMethod]
    public void JsonToXml_Inbound_ShouldNotThrow()
    {
        var test = new SimpleJsonToXml().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void JsonToXml_Backend_ShouldNotThrow()
    {
        var test = new SimpleJsonToXml().AsTestDocument();

        test.RunBackend();
    }

    [TestMethod]
    public void JsonToXml_Outbound_ShouldNotThrow()
    {
        var test = new SimpleJsonToXml().AsTestDocument();

        test.RunOutbound();
    }

    [TestMethod]
    public void JsonToXml_OnError_ShouldNotThrow()
    {
        var test = new SimpleJsonToXml().AsTestDocument();

        test.RunOnError();
    }

    [TestMethod]
    public void JsonToXml_Inbound_Callback()
    {
        var test = new SimpleJsonToXml().AsTestDocument();
        string? observedApply = null;

        test.SetupInbound().JsonToXml().WithCallback((context, config) =>
        {
            observedApply = config.Apply;
            context.Variables["json-to-xml-apply"] = config.Apply;
        });

        test.RunInbound();

        observedApply.Should().Be("always");
        test.Context.Variables.Should().ContainKey("json-to-xml-apply")
            .WhoseValue.Should().Be("always");
    }

    [TestMethod]
    public void JsonToXml_Inbound_PredicateCallback()
    {
        var test = new MultiJsonToXml().AsTestDocument();
        var matchedCount = 0;

        test.SetupInbound()
            .JsonToXml((_, config) => config.Apply == "content-type-json")
            .WithCallback((context, config) =>
            {
                matchedCount++;
                context.Variables["apply"] = config.Apply;
            });

        test.RunInbound();

        matchedCount.Should().Be(1);
        test.Context.Variables.Should().ContainKey("apply")
            .WhoseValue.Should().Be("content-type-json");
    }
}
