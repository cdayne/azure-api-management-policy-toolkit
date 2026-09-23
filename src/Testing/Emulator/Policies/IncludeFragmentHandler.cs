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
internal class IncludeFragmentHandler : IPolicyHandler
{
    public string PolicyName => nameof(IInboundContext.IncludeFragment);

    public object? Handle(GatewayContext context, object?[]? args)
    {
        var fragmentId = args?.FirstOrDefault()?.ToString()
            ?? throw new InvalidOperationException("Fragment ID is required for IncludeFragment.");

        // 1. Check pre-registered fragments first
        if (!context.FragmentRegistry.TryGetValue(fragmentId, out var fragment))
        {
            // 2. Scan all loaded assemblies for [Document] classes implementing IFragment, named as the compiler
            //    names them
            var fragmentType = FindFragmentType(fragmentId)
                ?? throw new InvalidOperationException(
                    $"Fragment '{fragmentId}' not found. Register it via RegisterFragment(\"{fragmentId}\", instance) " +
                    $"or ensure a class implementing IFragment named \"{fragmentId}\" by its [Document] attribute " +
                    "or its class name is loaded.");

            fragment = (IFragment)Activator.CreateInstance(fragmentType)!;
        }

        ExecuteFragment(fragment, context);
        return null;
    }

    private static Type? FindFragmentType(string fragmentId)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(IFragment).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
                        continue;

                    var docAttr = type.GetCustomAttributes(typeof(DocumentAttribute), false)
                        .OfType<DocumentAttribute>()
                        .FirstOrDefault();
                    if (docAttr is null)
                        continue;

                    // The compiler names a document after the [Document] name, or after the class when the attribute
                    // doesn't give one, and takes the document type from the attribute or the implemented interface.
                    // Fragment ids are matched the way the fragment registry matches them.
                    if (string.Equals(docAttr.Name ?? type.Name, fragmentId, StringComparison.OrdinalIgnoreCase))
                        return type;
                }
            }
            catch
            {
                // Skip assemblies that can't be scanned (e.g., dynamic or unloadable assemblies)
            }
        }

        return null;
    }

    private static void ExecuteFragment(IFragment fragment, GatewayContext context)
    {
        var handlers = context.CurrentSectionHandlers
            ?? throw new InvalidOperationException(
                "IncludeFragment must be called from within a section context.");

        var proxy = SectionContextProxy<IFragmentContext>.CreateWithHandlers<IFragmentContext>(context, handlers);
        try
        {
            fragment.Fragment(proxy.Object);
        }
        catch (FinishSectionProcessingException)
        {
            throw;
        }
    }
}
