// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class XslTransformTests
{
    private const string StyleSheet = "<xsl:stylesheet version=\"1.0\" />";

    class SimpleXslTransform : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.XslTransform(new XslTransformConfig
            {
                StyleSheet = StyleSheet,
                Parameters = [new XslTransformParameter { Name = "env", Value = "inbound" }]
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.XslTransform(new XslTransformConfig
            {
                StyleSheet = StyleSheet
            });
        }

        public void OnError(IOnErrorContext context)
        {
            context.XslTransform(new XslTransformConfig
            {
                StyleSheet = StyleSheet
            });
        }
    }

    class MultiXslTransform : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.XslTransform(new XslTransformConfig { StyleSheet = "sheet-a" });
            context.XslTransform(new XslTransformConfig { StyleSheet = "sheet-b" });
        }
    }

    [TestMethod]
    public void XslTransform_Inbound_ShouldNotThrow()
    {
        var test = new SimpleXslTransform().AsTestDocument();

        test.RunInbound();
    }

    [TestMethod]
    public void XslTransform_Outbound_ShouldNotThrow()
    {
        var test = new SimpleXslTransform().AsTestDocument();

        test.RunOutbound();
    }

    [TestMethod]
    public void XslTransform_OnError_ShouldNotThrow()
    {
        var test = new SimpleXslTransform().AsTestDocument();

        test.RunOnError();
    }

    [TestMethod]
    public void XslTransform_Inbound_Callback()
    {
        var test = new SimpleXslTransform().AsTestDocument();
        string? observedSheet = null;

        test.SetupInbound().XslTransform().WithCallback((context, config) =>
        {
            observedSheet = config.StyleSheet;
            context.Variables["xsl-sheet"] = config.StyleSheet;
        });

        test.RunInbound();

        observedSheet.Should().Be(StyleSheet);
        test.Context.Variables.Should().ContainKey("xsl-sheet");
    }

    [TestMethod]
    public void XslTransform_Inbound_PredicateCallback()
    {
        var test = new MultiXslTransform().AsTestDocument();
        var matchedCount = 0;

        test.SetupInbound()
            .XslTransform((_, config) => config.StyleSheet == "sheet-b")
            .WithCallback((context, config) =>
            {
                matchedCount++;
                context.Variables["selected-sheet"] = config.StyleSheet;
            });

        test.RunInbound();

        matchedCount.Should().Be(1);
        test.Context.Variables.Should().ContainKey("selected-sheet")
            .WhoseValue.Should().Be("sheet-b");
    }
}
