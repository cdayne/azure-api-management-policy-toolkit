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
            SetBackendUrl(context, config.BaseUrl);
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

            SetBackendUrl(context, backend.Url);
        }
    }

    private static void SetBackendUrl(GatewayContext context, string url)
    {
        context.BackendUrl = url;
        context.Api.ServiceUrl = new MockUrl(new Uri(url));
    }
}