// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class ValidateContentTests
{
    class SimpleValidateContent : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateContent(new ValidateContentConfig
            {
                UnspecifiedContentTypeAction = "prevent",
                MaxSize = 1000,
                SizeExceededAction = "prevent",
                ErrorsVariableName = "errors",
                ContentTypeMap = new ContentTypeMapConfig
                {
                    AnyContentTypeValue = "application/json",
                    MissingContentTypeValue = "application/octet-stream",
                    Types = [new ContentTypeMap { From = "text/json", To = "application/json" }]
                },
                Contents =
                [
                    new ValidateContent
                    {
                        ValidateAs = "json",
                        Action = "prevent",
                        Type = "application/json",
                        SchemaId = "schema-id",
                        AllowAdditionalProperties = false,
                        CaseInsensitivePropertyNames = true
                    }
                ]
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.ValidateContent(new ValidateContentConfig
            {
                UnspecifiedContentTypeAction = "prevent", MaxSize = 1000, SizeExceededAction = "prevent"
            });
        }

        public void OnError(IOnErrorContext context)
        {
            context.ValidateContent(new ValidateContentConfig
            {
                UnspecifiedContentTypeAction = "prevent", MaxSize = 1000, SizeExceededAction = "prevent"
            });
        }
    }

    class ExpressionValidateContent : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateContent(new ValidateContentConfig
            {
                UnspecifiedContentTypeAction = GetAction(context.ExpressionContext),
                MaxSize = GetMaxSize(context.ExpressionContext),
                SizeExceededAction = GetAction(context.ExpressionContext)
            });
        }

        public string GetAction(IExpressionContext context) => "prevent";

        public int GetMaxSize(IExpressionContext context) => 2048;
    }

    [TestMethod]
    public void ValidateContent_DefaultIsNoOp()
    {
        var test = new SimpleValidateContent().AsTestDocument();

        test.RunInbound();

        test.Context.Response.StatusCode.Should().NotBe(413);
    }

    [TestMethod]
    public void ValidateContent_Callback()
    {
        var test = new SimpleValidateContent().AsTestDocument();
        var executedCallback = false;
        test.SetupInbound().ValidateContent().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateContent_CallbackSimulatesFailure()
    {
        var test = new SimpleValidateContent().AsTestDocument();
        test.SetupInbound().ValidateContent().WithCallback((context, _) =>
        {
            context.Response.StatusCode = 413;
            throw new FinishSectionProcessingException();
        });

        test.RunInbound();

        test.Context.Response.StatusCode.Should().Be(413);
    }

    [TestMethod]
    public void ValidateContent_PredicateSelectsCallback()
    {
        var test = new SimpleValidateContent().AsTestDocument();
        var matchedCorrect = false;
        test.SetupInbound().ValidateContent((_, config) => config.MaxSize == 1000)
            .WithCallback((_, _) => matchedCorrect = true);
        test.SetupInbound().ValidateContent((_, config) => config.MaxSize == 2000)
            .WithCallback((_, _) => matchedCorrect = false);

        test.RunInbound();

        matchedCorrect.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateContent_Outbound()
    {
        var test = new SimpleValidateContent().AsTestDocument();

        test.RunOutbound();

        test.Context.Response.StatusCode.Should().NotBe(413);
    }

    [TestMethod]
    public void ValidateContent_OnError()
    {
        var test = new SimpleValidateContent().AsTestDocument();

        test.RunOnError();

        test.Context.Response.StatusCode.Should().NotBe(413);
    }

    [TestMethod]
    public void ValidateContent_ExpressionProperties()
    {
        var test = new ExpressionValidateContent().AsTestDocument();
        ValidateContentConfig? observedConfig = null;
        test.SetupInbound().ValidateContent().WithCallback((_, config) => observedConfig = config);

        test.RunInbound();

        observedConfig.Should().NotBeNull();
        observedConfig!.UnspecifiedContentTypeAction.Should().Be("prevent");
        observedConfig.MaxSize.Should().Be(2048);
        observedConfig.SizeExceededAction.Should().Be("prevent");
    }
}
