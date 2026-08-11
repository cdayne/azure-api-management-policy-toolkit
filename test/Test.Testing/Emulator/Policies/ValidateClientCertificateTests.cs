// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class ValidateClientCertificateTests
{
    class SimpleValidateClientCertificate : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.ValidateClientCertificate(new ValidateClientCertificateConfig
            {
                ValidateRevocation = true,
                ValidateTrust = true,
                ValidateNotBefore = true,
                ValidateNotAfter = true,
                IgnoreError = false,
                Identities =
                [
                    new CertificateIdentity
                    {
                        Thumbprint = "thumbprint",
                        SerialNumber = "serial",
                        CommonName = "common-name",
                        Subject = "subject",
                        DnsName = "dns-name",
                        IssuerSubject = "issuer-subject",
                        IssuerThumbprint = "issuer-thumbprint",
                        IssuerCertificateId = "issuer-cert-id"
                    }
                ]
            });
        }
    }

    [TestMethod]
    public void ValidateClientCertificate_DefaultIsNoOp()
    {
        var test = new SimpleValidateClientCertificate().AsTestDocument();

        test.RunInbound();

        test.Context.Response.StatusCode.Should().NotBe(403);
    }

    [TestMethod]
    public void ValidateClientCertificate_Callback()
    {
        var test = new SimpleValidateClientCertificate().AsTestDocument();
        var executedCallback = false;
        test.SetupInbound().ValidateClientCertificate().WithCallback((_, _) =>
        {
            executedCallback = true;
        });

        test.RunInbound();

        executedCallback.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateClientCertificate_CallbackSimulatesFailure()
    {
        var test = new SimpleValidateClientCertificate().AsTestDocument();
        test.SetupInbound().ValidateClientCertificate().WithCallback((context, _) =>
        {
            context.Response.StatusCode = 403;
            throw new FinishSectionProcessingException();
        });

        test.RunInbound();

        test.Context.Response.StatusCode.Should().Be(403);
    }

    [TestMethod]
    public void ValidateClientCertificate_PredicateSelectsCallback()
    {
        var test = new SimpleValidateClientCertificate().AsTestDocument();
        var matchedCorrect = false;
        test.SetupInbound().ValidateClientCertificate((_, config) => config.ValidateTrust == true)
            .WithCallback((_, _) => matchedCorrect = true);
        test.SetupInbound().ValidateClientCertificate((_, config) => config.ValidateTrust == false)
            .WithCallback((_, _) => matchedCorrect = false);

        test.RunInbound();

        matchedCorrect.Should().BeTrue();
    }
}
