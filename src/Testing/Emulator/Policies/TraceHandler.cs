// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Data;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

[
    Section(nameof(IInboundContext)),
    Section(nameof(IBackendContext)),
    Section(nameof(IOutboundContext)),
    Section(nameof(IOnErrorContext))
]
internal class TraceHandler : PolicyHandler<TraceConfig>
{
    public override string PolicyName => nameof(IInboundContext.Trace);

    protected override void Handle(GatewayContext context, TraceConfig config)
    {
        context.TraceStore.Add(new TraceEntry(config.Source, config.Message, config.Severity, config.Metadata));
    }
}
