// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class SetBackendServiceTests
{
    class SimpleSetBackendService : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.SetBackendService(new SetBackendServiceConfig
            {
                BaseUrl = "https://backend.example.com",
            });
        }

        public void Backend(IBackendContext context)
        {
            context.SetBackendService(new SetBackendServiceConfig
            {
                BaseUrl = "https://backend-section.example.com",
            });
        }

        public void Outbound(IOutboundContext context)
        {
            context.SetBackendService(new SetBackendServiceConfig
            {
                BaseUrl = "https://outbound-backend.example.com",
            });
        }

        public void OnError(IOnErrorContext context)
        {
            context.SetBackendService(new SetBackendServiceConfig
            {
                BaseUrl = "https://error-backend.example.com",
            });
        }
    }

    [TestMethod]
    public void SetBackendService_SetsBackendUrl()
    {
        // Arrange
        var test = new SimpleSetBackendService().AsTestDocument();

        // Act
        test.RunInbound();

        // Assert
        test.Context.BackendUrl.Should().Be("https://backend.example.com");
    }

    [TestMethod]
    public void SetBackendService_Backend_SetsBackendUrl()
    {
        var test = new SimpleSetBackendService().AsTestDocument();

        test.RunBackend();

        test.Context.BackendUrl.Should().Be("https://backend-section.example.com");
        test.Context.Api.ServiceUrl.Host.Should().Be("backend-section.example.com");
    }

    [TestMethod]
    public void SetBackendService_Outbound_SetsBackendUrl()
    {
        var test = new SimpleSetBackendService().AsTestDocument();

        test.RunOutbound();

        test.Context.BackendUrl.Should().Be("https://outbound-backend.example.com");
        test.Context.Api.ServiceUrl.Host.Should().Be("outbound-backend.example.com");
    }

    [TestMethod]
    public void SetBackendService_OnError_SetsBackendUrl()
    {
        var test = new SimpleSetBackendService().AsTestDocument();

        test.RunOnError();

        test.Context.BackendUrl.Should().Be("https://error-backend.example.com");
        test.Context.Api.ServiceUrl.Host.Should().Be("error-backend.example.com");
    }

    [TestMethod]
    public void SetBackendService_Callback()
    {
        // Arrange
        var test = new SimpleSetBackendService().AsTestDocument();
        var executedCallback = false;

        test.SetupInbound().SetBackendService().WithCallback((context, config) =>
        {
            executedCallback = true;
            context.Variables["backend-url"] = config.BaseUrl!;
        });

        // Act
        test.RunInbound();

        // Assert
        executedCallback.Should().BeTrue();
        test.Context.Variables.Should().ContainKey("backend-url")
            .WhoseValue.Should().Be("https://backend.example.com");
    }

    [TestMethod]
    public void SetBackendService_BackendId_ResolvesConfiguredBackend()
    {
        var test = new BackendIdDocument().AsTestDocument();
        test.SetupBackendStore().Add("backend", "https://resolved.example.com/v2");

        test.RunInbound();

        test.Context.Api.ServiceUrl.Host.Should().Be("resolved.example.com");
        test.Context.BackendUrl.Should().Be("https://resolved.example.com/v2");
    }

    [TestMethod]
    public void SetBackendService_BackendId_NotFound_Throws()
    {
        var test = new BackendIdDocument().AsTestDocument();

        var act = () => test.RunInbound();

        act.Should().Throw<BadRuntimeConfigurationException>()
            .Which.Message.Should().Contain("backend");
    }

    class BackendIdDocument : IDocument
    {
        public void Inbound(IInboundContext context) =>
            context.SetBackendService(new SetBackendServiceConfig { BackendId = "backend" });

        public void Backend(IBackendContext context) { }
        public void Outbound(IOutboundContext context) { }
        public void OnError(IOnErrorContext context) { }
    }
}
