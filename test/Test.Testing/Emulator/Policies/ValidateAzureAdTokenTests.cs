// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class ValidateAzureAdTokenTests
{
    class SimpleValidateAzureAdToken : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateAzureAdToken(new ValidateAzureAdTokenConfig
            {
                TenantId = "test-tenant",
                HeaderName = "Authorization",
                FailedValidationHttpCode = 401,
                FailedValidationErrorMessage = "Invalid token",
                OutputTokenVariableName = "jwt",
                BackendApplicationIds = ["backend-app-id"],
                ClientApplicationIds = ["client-app-id"],
                Audiences = ["audience"],
                RequiredClaims = [new ClaimConfig { Name = "roles" }],
                DecryptionKeys = [new DecryptionKey { CertificateId = "cert-id" }]
            });
        }
    }

    class ExpressionValidateAzureAdToken : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateAzureAdToken(new ValidateAzureAdTokenConfig
            {
                TenantId = GetTenantId(context.ExpressionContext),
                QueryParameterName = "access_token",
                TokenValue = GetTokenValue(context.ExpressionContext)
            });
        }

        public string GetTenantId(IExpressionContext context) => "expr-tenant";

        public string GetTokenValue(IExpressionContext context) => "expr-token";
    }

    [TestMethod]
    public void ValidateAzureAdToken_DefaultIsNoOp()
    {
        var test = new SimpleValidateAzureAdToken().AsTestDocument();

        test.RunInbound();

        test.Context.Response.StatusCode.Should().NotBe(401);
    }

    [TestMethod]
    public void ValidateAzureAdToken_Callback()
    {
        var test = new SimpleValidateAzureAdToken().AsTestDocument();
        var executedCallback = false;
        test.SetupInbound().ValidateAzureAdToken().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateAzureAdToken_CallbackSimulatesFailure()
    {
        var test = new SimpleValidateAzureAdToken().AsTestDocument();
        test.SetupInbound().ValidateAzureAdToken().WithCallback((context, config) =>
        {
            context.Response.StatusCode = config.FailedValidationHttpCode!.Value;
            throw new FinishSectionProcessingException();
        });

        test.RunInbound();

        test.Context.Response.StatusCode.Should().Be(401);
    }

    [TestMethod]
    public void ValidateAzureAdToken_PredicateSelectsCallback()
    {
        var test = new SimpleValidateAzureAdToken().AsTestDocument();
        var matchedCorrect = false;
        test.SetupInbound().ValidateAzureAdToken((_, config) => config.TenantId == "test-tenant")
            .WithCallback((_, _) => matchedCorrect = true);
        test.SetupInbound().ValidateAzureAdToken((_, config) => config.TenantId == "other-tenant")
            .WithCallback((_, _) => matchedCorrect = false);

        test.RunInbound();

        matchedCorrect.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateAzureAdToken_ExpressionProperties()
    {
        var test = new ExpressionValidateAzureAdToken().AsTestDocument();
        ValidateAzureAdTokenConfig? observedConfig = null;
        test.SetupInbound().ValidateAzureAdToken().WithCallback((_, config) => observedConfig = config);

        test.RunInbound();

        observedConfig.Should().NotBeNull();
        observedConfig!.TenantId.Should().Be("expr-tenant");
        observedConfig.QueryParameterName.Should().Be("access_token");
        observedConfig.TokenValue.Should().Be("expr-token");
    }
}
