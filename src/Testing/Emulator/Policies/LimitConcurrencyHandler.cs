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
internal class LimitConcurrencyHandler : PolicyHandler<LimitConcurrencyConfig, Action>
{
    public override string PolicyName => nameof(IInboundContext.LimitConcurrency);

    protected override void Handle(GatewayContext context, LimitConcurrencyConfig config, Action section)
    {
        // The emulator does not track real concurrency, so the child policies always execute
        // as if the limit was not reached. Test authors use CallbackSetup/WithCallback to
        // simulate the limit being exceeded (e.g., by not invoking the section delegate).
        section();
    }
}
