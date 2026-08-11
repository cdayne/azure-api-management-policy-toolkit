// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Emulator.Policies;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;

public static class MockValidateOdataRequestProvider
{
    public static Setup ValidateOdataRequest(this MockPoliciesProvider<IInboundContext> mock) =>
        ValidateOdataRequest(mock, (_, _) => true);

    public static Setup ValidateOdataRequest(
        this MockPoliciesProvider<IInboundContext> mock,
        Func<GatewayContext, ValidateOdataRequestConfig, bool> predicate
    )
    {
        var handler = mock.SectionContextProxy.GetHandler<ValidateOdataRequestHandler>();
        return new Setup(predicate, handler);
    }

    public class Setup
    {
        private readonly Func<GatewayContext, ValidateOdataRequestConfig, bool> _predicate;
        private readonly ValidateOdataRequestHandler _handler;

        internal Setup(
            Func<GatewayContext, ValidateOdataRequestConfig, bool> predicate,
            ValidateOdataRequestHandler handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public void WithCallback(Action<GatewayContext, ValidateOdataRequestConfig> callback) =>
            _handler.CallbackSetup.Add((_predicate, callback).ToTuple());
    }
}
