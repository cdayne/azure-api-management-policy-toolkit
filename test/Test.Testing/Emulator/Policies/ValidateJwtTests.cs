// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class ValidateJwtTests
{
    class SimpleValidateJwt : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateJwt(new ValidateJwtConfig
            {
                HeaderName = "Authorization",
                FailedValidationHttpCode = 401,
                FailedValidationErrorMessage = "Invalid token",
                RequireExpirationTime = true,
                RequireScheme = "Bearer",
                RequireSignedTokens = true,
                ClockSkew = 5,
                OutputTokenVariableName = "jwt",
                Audiences = ["audience"],
                Issuers = ["issuer"],
                OpenIdConfigs = [new OpenIdConfig { Url = "https://example.com/.well-known/openid-configuration" }],
                IssuerSigningKeys = [new Base64KeyConfig { Value = "c2lnbmluZy1rZXk=" }],
                DecryptionKeys = [new CertificateKeyConfig { CertificateId = "cert-id" }],
                RequiredClaims = [new ClaimConfig { Name = "roles" }]
            });
        }
    }

    class ExpressionValidateJwt : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateJwt(new ValidateJwtConfig
            {
                HeaderName = GetHeaderName(context.ExpressionContext),
                QueryParameterName = GetQueryParameterName(context.ExpressionContext),
                TokenValue = GetTokenValue(context.ExpressionContext),
                FailedValidationHttpCode = GetFailedCode(context.ExpressionContext),
                FailedValidationErrorMessage = GetFailedMessage(context.ExpressionContext),
                RequireExpirationTime = GetRequireExpiration(context.ExpressionContext),
                RequireScheme = GetScheme(context.ExpressionContext),
                RequireSignedTokens = GetRequireSigned(context.ExpressionContext),
                ClockSkew = GetClockSkew(context.ExpressionContext),
                Audiences = GetAudiences(context.ExpressionContext),
                Issuers = GetIssuers(context.ExpressionContext)
            });
        }

        public string GetHeaderName(IExpressionContext context) => "Authorization";
        public string GetQueryParameterName(IExpressionContext context) => "access_token";
        public string GetTokenValue(IExpressionContext context) => "token-value";
        public int GetFailedCode(IExpressionContext context) => 401;
        public string GetFailedMessage(IExpressionContext context) => "Invalid token";
        public bool GetRequireExpiration(IExpressionContext context) => true;
        public string GetScheme(IExpressionContext context) => "Bearer";
        public bool GetRequireSigned(IExpressionContext context) => true;
        public int GetClockSkew(IExpressionContext context) => 5;
        public string[] GetAudiences(IExpressionContext context) => ["audience"];
        public string[] GetIssuers(IExpressionContext context) => ["issuer"];
    }

    [TestMethod]
    public void ValidateJwt_DefaultIsNoOp()
    {
        var test = new SimpleValidateJwt().AsTestDocument();

        test.RunInbound();

        test.Context.Response.StatusCode.Should().NotBe(401);
    }

    [TestMethod]
    public void ValidateJwt_Callback()
    {
        var test = new SimpleValidateJwt().AsTestDocument();
        var executedCallback = false;
        test.SetupInbound().ValidateJwt().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateJwt_CallbackSimulatesFailure()
    {
        var test = new SimpleValidateJwt().AsTestDocument();
        test.SetupInbound().ValidateJwt().WithCallback((context, config) =>
        {
            context.Response.StatusCode = config.FailedValidationHttpCode!.Value;
            throw new FinishSectionProcessingException();
        });

        test.RunInbound();

        test.Context.Response.StatusCode.Should().Be(401);
    }

    [TestMethod]
    public void ValidateJwt_PredicateSelectsCallback()
    {
        var test = new SimpleValidateJwt().AsTestDocument();
        var matchedCorrect = false;
        test.SetupInbound().ValidateJwt((_, config) => config.HeaderName == "Authorization")
            .WithCallback((_, _) => matchedCorrect = true);
        test.SetupInbound().ValidateJwt((_, config) => config.HeaderName == "X-Other")
            .WithCallback((_, _) => matchedCorrect = false);

        test.RunInbound();

        matchedCorrect.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateJwt_ExpressionProperties()
    {
        var test = new ExpressionValidateJwt().AsTestDocument();
        ValidateJwtConfig? observedConfig = null;
        test.SetupInbound().ValidateJwt().WithCallback((_, config) => observedConfig = config);

        test.RunInbound();

        observedConfig.Should().NotBeNull();
        observedConfig!.HeaderName.Should().Be("Authorization");
        observedConfig.QueryParameterName.Should().Be("access_token");
        observedConfig.TokenValue.Should().Be("token-value");
        observedConfig.FailedValidationHttpCode.Should().Be(401);
        observedConfig.FailedValidationErrorMessage.Should().Be("Invalid token");
        observedConfig.RequireExpirationTime.Should().BeTrue();
        observedConfig.RequireScheme.Should().Be("Bearer");
        observedConfig.RequireSignedTokens.Should().BeTrue();
        observedConfig.ClockSkew.Should().Be(5);
        observedConfig.Audiences.Should().ContainSingle().Which.Should().Be("audience");
        observedConfig.Issuers.Should().ContainSingle().Which.Should().Be("issuer");
    }
}
