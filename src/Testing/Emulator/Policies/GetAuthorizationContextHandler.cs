// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

[
    Section(nameof(IInboundContext)),
    Section(nameof(IBackendContext)),
    Section(nameof(IOutboundContext))
]
internal class GetAuthorizationContextHandler : PolicyHandler<GetAuthorizationContextConfig>
{
    /// <summary>
    /// Hooks that provide a mock <see cref="Authorization"/> object for a given provider/authorization pair.
    /// Test authors register these via <c>WithAuthorizationProviderHook</c>/<c>ReturnsAuthorization</c>.
    /// </summary>
    public List<Tuple<
        Func<GatewayContext, GetAuthorizationContextConfig, bool>,
        Func<string, string, Authorization>
    >> ProvideAuthorizationHooks { get; } = new();

    public override string PolicyName => nameof(IInboundContext.GetAuthorizationContext);

    protected override void Handle(GatewayContext context, GetAuthorizationContextConfig config)
    {
        var provideAuthorizationHook = ProvideAuthorizationHooks.Find(hook => hook.Item1(context, config));
        var hook = provideAuthorizationHook is not null ? provideAuthorizationHook.Item2 : DefaultAuthorizationProvider;

        var authorization = IgnoreErrorExtensions.InvokeOrFallback(
            () => hook(config.ProviderId, config.AuthorizationId),
            config.IgnoreError ?? false,
            fallback: null!);

        context.Variables[config.ContextVariableName] = authorization!;
    }

    private static Authorization DefaultAuthorizationProvider(string providerId, string authorizationId) =>
        new(
            accessToken: $"mock-access-token-{providerId}-{authorizationId}",
            claims: new Dictionary<string, object>());
}
