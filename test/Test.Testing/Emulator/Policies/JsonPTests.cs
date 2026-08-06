// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class JsonPTests
{
    class SimpleJsonP : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.JsonP("callback");
        }
    }

    class MultiJsonP : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.JsonP("callback-a");
            context.JsonP("callback-b");
        }
    }

    [TestMethod]
    public void JsonP_Outbound_ShouldNotThrowOrMutateResponse()
    {
        var test = new TestDocument(new SimpleJsonP())
        {
            Context = { Response = { Headers = { ["X-Existing"] = ["1"] } } }
        };

        test.RunOutbound();

        test.Context.Response.Headers.Should().ContainKey("X-Existing")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("1");
    }

    [TestMethod]
    public void JsonP_Outbound_Callback()
    {
        var test = new SimpleJsonP().AsTestDocument();
        string? observedParameter = null;

        test.SetupOutbound().JsonP().WithCallback((context, callbackParameterName) =>
        {
            observedParameter = callbackParameterName;
            context.Variables["jsonp"] = callbackParameterName;
        });

        test.RunOutbound();

        observedParameter.Should().Be("callback");
        test.Context.Variables.Should().ContainKey("jsonp")
            .WhoseValue.Should().Be("callback");
    }

    [TestMethod]
    public void JsonP_Outbound_PredicateCallback()
    {
        var test = new MultiJsonP().AsTestDocument();
        var matchedCount = 0;

        test.SetupOutbound()
            .JsonP((_, callbackParameterName) => callbackParameterName == "callback-b")
            .WithCallback((context, callbackParameterName) =>
            {
                matchedCount++;
                context.Variables["jsonp-selected"] = callbackParameterName;
            });

        test.RunOutbound();

        matchedCount.Should().Be(1);
        test.Context.Variables.Should().ContainKey("jsonp-selected")
            .WhoseValue.Should().Be("callback-b");
    }
}
