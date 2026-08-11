// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class ValidateParametersTests
{
    class SimpleValidateParameters : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateParameters(new ValidateParametersConfig
            {
                SpecifiedParameterAction = "prevent",
                UnspecifiedParameterAction = "ignore",
                ErrorsVariableName = "errors",
                Headers = new ValidateHeaderParameters
                {
                    SpecifiedParameterAction = "prevent",
                    UnspecifiedParameterAction = "ignore",
                    Parameters = [new ValidateParameter { Name = "Content-Type", Action = "prevent" }]
                },
                Query = new ValidateQueryParameters
                {
                    SpecifiedParameterAction = "prevent",
                    UnspecifiedParameterAction = "ignore",
                    Parameters = [new ValidateParameter { Name = "api-version", Action = "prevent" }]
                },
                Path = new ValidatePathParameters
                {
                    SpecifiedParameterAction = "prevent",
                    Parameters = [new ValidateParameter { Name = "id", Action = "prevent" }]
                }
            });
        }
    }

    class ExpressionValidateParameters : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateParameters(new ValidateParametersConfig
            {
                SpecifiedParameterAction = GetAction(context.ExpressionContext),
                UnspecifiedParameterAction = GetAction(context.ExpressionContext)
            });
        }

        public string GetAction(IExpressionContext context) => "prevent";
    }

    [TestMethod]
    public void ValidateParameters_DefaultIsNoOp()
    {
        var test = new SimpleValidateParameters().AsTestDocument();

        test.RunInbound();

        test.Context.Response.StatusCode.Should().NotBe(400);
    }

    [TestMethod]
    public void ValidateParameters_Callback()
    {
        var test = new SimpleValidateParameters().AsTestDocument();
        var executedCallback = false;
        test.SetupInbound().ValidateParameters().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateParameters_CallbackSimulatesFailure()
    {
        var test = new SimpleValidateParameters().AsTestDocument();
        test.SetupInbound().ValidateParameters().WithCallback((context, _) =>
        {
            context.Response.StatusCode = 400;
            throw new FinishSectionProcessingException();
        });

        test.RunInbound();

        test.Context.Response.StatusCode.Should().Be(400);
    }

    [TestMethod]
    public void ValidateParameters_PredicateSelectsCallback()
    {
        var test = new SimpleValidateParameters().AsTestDocument();
        var matchedCorrect = false;
        test.SetupInbound().ValidateParameters((_, config) => config.UnspecifiedParameterAction == "ignore")
            .WithCallback((_, _) => matchedCorrect = true);
        test.SetupInbound().ValidateParameters((_, config) => config.UnspecifiedParameterAction == "prevent")
            .WithCallback((_, _) => matchedCorrect = false);

        test.RunInbound();

        matchedCorrect.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateParameters_ExpressionProperties()
    {
        var test = new ExpressionValidateParameters().AsTestDocument();
        ValidateParametersConfig? observedConfig = null;
        test.SetupInbound().ValidateParameters().WithCallback((_, config) => observedConfig = config);

        test.RunInbound();

        observedConfig.Should().NotBeNull();
        observedConfig!.SpecifiedParameterAction.Should().Be("prevent");
        observedConfig.UnspecifiedParameterAction.Should().Be("prevent");
    }
}
