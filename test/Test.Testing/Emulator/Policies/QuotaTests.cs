// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Services;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class QuotaTests
{
    class SimpleQuota : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Quota(new QuotaConfig { Calls = 2, RenewalPeriod = 60 });
        }
    }

    class QuotaThenSetHeader : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Quota(new QuotaConfig { Calls = 1, RenewalPeriod = 60 });
            context.SetHeader("X-After-Quota", "executed");
        }
    }

    class MultiQuota : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.Quota(new QuotaConfig { Calls = 2, RenewalPeriod = 60 });
            context.Quota(new QuotaConfig { Calls = 5, RenewalPeriod = 120 });
        }
    }

    private sealed class StubRateLimiter(bool shouldAllow) : IRateLimiter
    {
        public Task<bool> TryConsumeAsync(string key, int permits = 1, CancellationToken cancellationToken = default) =>
            Task.FromResult(shouldAllow);
    }

    [TestMethod]
    public void Quota_UnderLimit_ShouldIncrementStore()
    {
        var test = new SimpleQuota().AsTestDocument();
        var quotaKey = $"quota:sub:{test.Context.Subscription.Id}";

        test.RunInbound();

        test.Context.Response.StatusCode.Should().NotBe(403);
        test.SetupRateLimitStore().GetCount(quotaKey).Should().Be(1);
    }

    [TestMethod]
    public void Quota_Exceeded_ShouldSetForbiddenAndStopSection()
    {
        var test = new QuotaThenSetHeader().AsTestDocument();
        var quotaKey = $"quota:sub:{test.Context.Subscription.Id}";
        var setHeaderExecuted = false;
        test.SetupRateLimitStore().SetCount(quotaKey, 1);
        test.SetupInbound().SetHeader().WithCallback((_, _, _) => setHeaderExecuted = true);

        test.RunInbound();

        setHeaderExecuted.Should().BeFalse();
        test.Context.Response.StatusCode.Should().Be(403);
        test.Context.Response.StatusReason.Should().Be("Quota Exceeded");
    }

    [TestMethod]
    public void Quota_Inbound_Callback()
    {
        var test = new SimpleQuota().AsTestDocument();
        var callbackExecuted = false;
        var quotaKey = $"quota:sub:{test.Context.Subscription.Id}";

        test.SetupInbound().Quota().WithCallback((context, config) =>
        {
            callbackExecuted = true;
            context.Variables["quota-calls"] = config.Calls;
        });

        test.RunInbound();

        callbackExecuted.Should().BeTrue();
        test.Context.Variables.Should().ContainKey("quota-calls")
            .WhoseValue.Should().Be(2);
        test.SetupRateLimitStore().GetCount(quotaKey).Should().Be(0);
    }

    [TestMethod]
    public void Quota_Inbound_PredicateCallback()
    {
        var test = new MultiQuota().AsTestDocument();
        var quotaKey = $"quota:sub:{test.Context.Subscription.Id}";
        var callbackExecuted = false;

        test.SetupInbound()
            .Quota((_, config) => config.RenewalPeriod == 120)
            .WithCallback((context, config) =>
            {
                callbackExecuted = true;
                context.Variables["quota-renewal"] = config.RenewalPeriod;
            });

        test.RunInbound();

        callbackExecuted.Should().BeTrue();
        test.Context.Variables.Should().ContainKey("quota-renewal")
            .WhoseValue.Should().Be(120);
        test.SetupRateLimitStore().GetCount(quotaKey).Should().Be(1);
    }

    [TestMethod]
    public void Quota_RegisteredRateLimiter_ShouldControlOutcome()
    {
        var allowedTest = new SimpleQuota().AsTestDocument();
        allowedTest.Context.Services.Register<IRateLimiter>(new StubRateLimiter(true));

        allowedTest.RunInbound();

        allowedTest.Context.Response.StatusCode.Should().NotBe(403);

        var deniedTest = new SimpleQuota().AsTestDocument();
        deniedTest.Context.Services.Register<IRateLimiter>(new StubRateLimiter(false));

        deniedTest.RunInbound();

        deniedTest.Context.Response.StatusCode.Should().Be(403);
        deniedTest.Context.Response.StatusReason.Should().Be("Quota Exceeded");
    }
}
