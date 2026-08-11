// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

[
    Section(nameof(IInboundContext)),
    Section(nameof(IBackendContext)),
    Section(nameof(IOutboundContext)),
    Section(nameof(IOnErrorContext))
]
internal class XmlToJsonHandler : PolicyHandler<XmlToJsonConfig>
{
    public override string PolicyName => nameof(IInboundContext.XmlToJson);

    protected override void Handle(GatewayContext context, XmlToJsonConfig config)
    {
        // No-op by default in emulator.
        // XML to JSON conversion is not simulated in tests.
        // Test authors use CallbackSetup to simulate conversion behavior.
    }
}
