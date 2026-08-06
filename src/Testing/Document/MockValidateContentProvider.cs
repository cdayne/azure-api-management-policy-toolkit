// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockValidateContentProvider
{
    public static Setup ValidateContent(this MockPoliciesProvider<IInboundContext> mock) =>
        ValidateContent(mock, (_, _) => true);

    public static Setup ValidateContent(this MockPoliciesProvider<IOutboundContext> mock) =>
        ValidateContent(mock, (_, _) => true);

    public static Setup ValidateContent(this MockPoliciesProvider<IOnErrorContext> mock) =>
        ValidateContent(mock, (_, _) => true);

    public static Setup ValidateContent(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, ValidateContentConfig, bool> predicate
    ) => ValidateContent<IInboundContext>(mock, predicate);

    public static Setup ValidateContent(
        this MockPoliciesProvider<IOutboundContext> mock,
        Func<GatewayContext, ValidateContentConfig, bool> predicate
    ) => ValidateContent<IOutboundContext>(mock, predicate);

    public static Setup ValidateContent(
        this MockPoliciesProvider<IOnErrorContext> mock,
        Func<GatewayContext, ValidateContentConfig, bool> predicate
    ) => ValidateContent<IOnErrorContext>(mock, predicate);

    private static Setup ValidateContent<T>(
        MockPoliciesProvider<T> mock,
        Func<GatewayContext, ValidateContentConfig, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<ValidateContentHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, ValidateContentConfig, bool> _predicate;
        private readonly ValidateContentHandler _handler;

        internal Setup(
            Func<GatewayContext, ValidateContentConfig, bool> predicate,
            ValidateContentHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, ValidateContentConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
