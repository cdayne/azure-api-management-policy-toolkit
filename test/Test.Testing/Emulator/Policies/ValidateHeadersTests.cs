// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class ValidateHeadersTests
{
    class SimpleValidateHeaders : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.ValidateHeaders(new ValidateHeadersConfig
            {
                SpecifiedHeaderAction = "prevent",
                UnspecifiedHeaderAction = "ignore",
                ErrorsVariableName = "errors",
                Headers = [new ValidateHeader { Name = "Content-Type", Action = "prevent" }]
            });
        }

        public void OnError(IOnErrorContext context)
        {
            context.ValidateHeaders(new ValidateHeadersConfig
            {
                SpecifiedHeaderAction = "prevent", UnspecifiedHeaderAction = "ignore"
            });
        }
    }

    class ExpressionValidateHeaders : IDocument
    {
        public void Outbound(IOutboundContext context)
        {
            context.ValidateHeaders(new ValidateHeadersConfig
            {
                SpecifiedHeaderAction = GetAction(context.ExpressionContext),
                UnspecifiedHeaderAction = GetAction(context.ExpressionContext)
            });
        }

        public string GetAction(IExpressionContext context) => "prevent";
    }

    [TestMethod]
    public void ValidateHeaders_DefaultIsNoOp()
    {
        var test = new SimpleValidateHeaders().AsTestDocument();

        test.RunOutbound();

        test.Context.Response.StatusCode.Should().NotBe(400);
    }

    [TestMethod]
    public void ValidateHeaders_Callback()
    {
        var test = new SimpleValidateHeaders().AsTestDocument();
        var executedCallback = false;
        test.SetupOutbound().ValidateHeaders().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunOutbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateHeaders_CallbackSimulatesFailure()
    {
        var test = new SimpleValidateHeaders().AsTestDocument();
        test.SetupOutbound().ValidateHeaders().WithCallback((context, _) =>
        {
            context.Response.StatusCode = 400;
            throw new FinishSectionProcessingException();
        });

        test.RunOutbound();

        test.Context.Response.StatusCode.Should().Be(400);
    }

    [TestMethod]
    public void ValidateHeaders_PredicateSelectsCallback()
    {
        var test = new SimpleValidateHeaders().AsTestDocument();
        var matchedCorrect = false;
        test.SetupOutbound().ValidateHeaders((_, config) => config.UnspecifiedHeaderAction == "ignore")
            .WithCallback((_, _) => matchedCorrect = true);
        test.SetupOutbound().ValidateHeaders((_, config) => config.UnspecifiedHeaderAction == "prevent")
            .WithCallback((_, _) => matchedCorrect = false);

        test.RunOutbound();

        matchedCorrect.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateHeaders_OnError()
    {
        var test = new SimpleValidateHeaders().AsTestDocument();

        test.RunOnError();

        test.Context.Response.StatusCode.Should().NotBe(400);
    }

    [TestMethod]
    public void ValidateHeaders_ExpressionProperties()
    {
        var test = new ExpressionValidateHeaders().AsTestDocument();
        ValidateHeadersConfig? observedConfig = null;
        test.SetupOutbound().ValidateHeaders().WithCallback((_, config) => observedConfig = config);

        test.RunOutbound();

        observedConfig.Should().NotBeNull();
        observedConfig!.SpecifiedHeaderAction.Should().Be("prevent");
        observedConfig.UnspecifiedHeaderAction.Should().Be("prevent");
    }
}
