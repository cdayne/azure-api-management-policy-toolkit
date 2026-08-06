// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

internal static class IgnoreErrorExtensions
{
    /// <summary>
    /// Invokes <paramref name="action"/>, returning <paramref name="fallback"/> if it throws and
    /// <paramref name="ignoreError"/> is true. Rethrows otherwise.
    /// </summary>
    public static TResult InvokeOrFallback<TResult>(Func<TResult> action, bool ignoreError, TResult fallback) =>
        InvokeOrFallback(action, static a => a(), ignoreError, fallback);

    /// <summary>
    /// Invokes <paramref name="action"/> with <paramref name="state"/>, returning <paramref name="fallback"/>
    /// if it throws and <paramref name="ignoreError"/> is true. Rethrows otherwise.
    /// </summary>
    /// <remarks>
    /// Pass a <c>static</c> lambda for <paramref name="action"/> that reads everything it needs from
    /// <paramref name="state"/>, so no per-call closure is allocated.
    /// </remarks>
    public static TResult InvokeOrFallback<TState, TResult>(
        TState state,
        Func<TState, TResult> action,
        bool ignoreError,
        TResult fallback)
    {
        try
        {
            return action(state);
        }
        catch
        {
            if (ignoreError)
            {
                return fallback;
            }

            throw;
        }
    }
}
