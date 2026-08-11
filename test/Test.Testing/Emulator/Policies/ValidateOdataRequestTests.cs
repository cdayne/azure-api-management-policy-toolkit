// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class ValidateOdataRequestTests
{
    class SimpleValidateOdataRequest : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateOdataRequest(new ValidateOdataRequestConfig
            {
                ErrorVariableName = "errors",
                DefaultOdataVersion = "4.0",
                MinOdataVersion = "4.0",
                MaxOdataVersion = "4.0",
                MaxSize = 1000
            });
        }
    }

    [TestMethod]
    public void ValidateOdataRequest_DefaultIsNoOp()
    {
        var test = new SimpleValidateOdataRequest().AsTestDocument();

        test.RunInbound();

        test.Context.Response.StatusCode.Should().NotBe(400);
    }

    [TestMethod]
    public void ValidateOdataRequest_Callback()
    {
        var test = new SimpleValidateOdataRequest().AsTestDocument();
        var executedCallback = false;
        test.SetupInbound().ValidateOdataRequest().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateOdataRequest_CallbackSimulatesFailure()
    {
        var test = new SimpleValidateOdataRequest().AsTestDocument();
        test.SetupInbound().ValidateOdataRequest().WithCallback((context, _) =>
        {
            context.Response.StatusCode = 400;
            throw new FinishSectionProcessingException();
        });

        test.RunInbound();

        test.Context.Response.StatusCode.Should().Be(400);
    }

    [TestMethod]
    public void ValidateOdataRequest_PredicateSelectsCallback()
    {
        var test = new SimpleValidateOdataRequest().AsTestDocument();
        var matchedCorrect = false;
        test.SetupInbound().ValidateOdataRequest((_, config) => config.MaxSize == 1000)
            .WithCallback((_, _) => matchedCorrect = true);
        test.SetupInbound().ValidateOdataRequest((_, config) => config.MaxSize == 2000)
            .WithCallback((_, _) => matchedCorrect = false);

        test.RunInbound();

        matchedCorrect.Should().BeTrue();
    }
}
