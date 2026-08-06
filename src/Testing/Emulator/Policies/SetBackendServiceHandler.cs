// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Expressions;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

[
    Section(nameof(IInboundContext)),
    Section(nameof(IBackendContext)),
    Section(nameof(IOutboundContext)),
    Section(nameof(IOnErrorContext))
]
internal class SetBackendServiceHandler : PolicyHandler<SetBackendServiceConfig>
{
    public override string PolicyName => nameof(IInboundContext.SetBackendService);

    protected override void Handle(GatewayContext context, SetBackendServiceConfig config)
    {
        if (config.BaseUrl is not null)
        {
            context.BackendUrl = config.BaseUrl;
            context.Api.ServiceUrl = new MockUrl(new Uri(config.BaseUrl));
        }
        else if (config.BackendId is not null)
        {
            if (!context.BackendStore.TryGet(config.BackendId, out var backend))
            {
                throw new BadRuntimeConfigurationException(
                    $"Backend with id '{config.BackendId}' could not be found.")
                {
                    Policy = PolicyName
                };
            }

            context.BackendUrl = backend.Url;
            context.Api.ServiceUrl = new MockUrl(new Uri(backend.Url));
        }
    }
}