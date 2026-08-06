// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockXmlToJsonProvider
{
    public static Setup XmlToJson<T>(this MockPoliciesProvider<T> mock) where T : class =>
        XmlToJson(mock, (_, _) => true);

    public static Setup XmlToJson<T>(
        this MockPoliciesProvider<T> mock,
        Func<GatewayContext, XmlToJsonConfig, bool> predicate
    ) where T : class
    {
        var handler = mock.SectionContextProxy.GetHandler<XmlToJsonHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, XmlToJsonConfig, bool> _predicate;
        private readonly XmlToJsonHandler _handler;

        internal Setup(
            Func<GatewayContext, XmlToJsonConfig, bool> predicate,
            XmlToJsonHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, XmlToJsonConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
