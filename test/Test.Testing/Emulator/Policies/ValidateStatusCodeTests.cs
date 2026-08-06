// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class ValidateStatusCodeTests
{
    class SimpleValidateStatusCode : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.ValidateStatusCode(new ValidateStatusCodeConfig
            {
                UnspecifiedStatusCodeAction = "prevent",
                ErrorVariableName = "errors",
                StatusCodes = [new ValidateStatusCode { Code = 200, Action = "ignore" }]
            });
        }

        public void OnError(IOnErrorContext context)
        {
            context.ValidateStatusCode(new ValidateStatusCodeConfig { UnspecifiedStatusCodeAction = "prevent" });
        }
    }

    class ExpressionValidateStatusCode : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.ValidateStatusCode(new ValidateStatusCodeConfig
            {
                UnspecifiedStatusCodeAction = GetAction(context.ExpressionContext),
                StatusCodes = [new ValidateStatusCode { Code = 200, Action = GetAction(context.ExpressionContext) }]
            });
        }

        public string GetAction(IExpressionContext context) => "prevent";
    }

    [TestMethod]
    public void ValidateStatusCode_DefaultIsNoOp()
    {
        var test = new SimpleValidateStatusCode().AsTestDocument();

        test.RunOutbound();

        test.Context.Response.StatusCode.Should().NotBe(500);
    }

    [TestMethod]
    public void ValidateStatusCode_Callback()
    {
        var test = new SimpleValidateStatusCode().AsTestDocument();
        var executedCallback = false;
        test.SetupOutbound().ValidateStatusCode().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunOutbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateStatusCode_CallbackSimulatesFailure()
    {
        var test = new SimpleValidateStatusCode().AsTestDocument();
        test.SetupOutbound().ValidateStatusCode().WithCallback((context, _) =>
        {
            context.Response.StatusCode = 500;
            throw new FinishSectionProcessingException();
        });

        test.RunOutbound();

        test.Context.Response.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public void ValidateStatusCode_PredicateSelectsCallback()
    {
        var test = new SimpleValidateStatusCode().AsTestDocument();
        var matchedCorrect = false;
        test.SetupOutbound().ValidateStatusCode((_, config) => config.UnspecifiedStatusCodeAction == "prevent")
            .WithCallback((_, _) => matchedCorrect = true);
        test.SetupOutbound().ValidateStatusCode((_, config) => config.UnspecifiedStatusCodeAction == "ignore")
            .WithCallback((_, _) => matchedCorrect = false);

        test.RunOutbound();

        matchedCorrect.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateStatusCode_OnError()
    {
        var test = new SimpleValidateStatusCode().AsTestDocument();

        test.RunOnError();

        test.Context.Response.StatusCode.Should().NotBe(500);
    }

    [TestMethod]
    public void ValidateStatusCode_ExpressionProperties()
    {
        var test = new ExpressionValidateStatusCode().AsTestDocument();
        ValidateStatusCodeConfig? observedConfig = null;
        test.SetupOutbound().ValidateStatusCode().WithCallback((_, config) => observedConfig = config);

        test.RunOutbound();

        observedConfig.Should().NotBeNull();
        observedConfig!.UnspecifiedStatusCodeAction.Should().Be("prevent");
        observedConfig.StatusCodes.Should().ContainSingle().Which.Action.Should().Be("prevent");
    }
}
