// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Document;
using Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Services;

namespace Test.Emulator.Emulator.Policies;

[TestClass]
public class RateLimitByKeyTests
{
    class RateLimitByKeyWithHeaders : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.RateLimitByKey(new RateLimitByKeyConfig
            {
                Calls = 3,
                RenewalPeriod = 60,
                CounterKey = "user-1",
                RetryAfterHeaderName = "Retry-After",
                RetryAfterVariableName = "retryAfter",
                RemainingCallsHeaderName = "X-Remaining",
                RemainingCallsVariableName = "remainingCalls",
                TotalCallsHeaderName = "X-Limit"
            });
        }
    }

    class RateLimitByKeyThenSetHeader : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.RateLimitByKey(new RateLimitByKeyConfig
            {
                Calls = 1,
                RenewalPeriod = 30,
                CounterKey = "user-1",
                RetryAfterHeaderName = "Retry-After",
                RetryAfterVariableName = "retryAfter",
                TotalCallsHeaderName = "X-Limit"
            });
            context.SetHeader("X-After-RateLimit", "executed");
        }
    }

    class MultiRateLimitByKey : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.RateLimitByKey(new RateLimitByKeyConfig
            {
                Calls = 3,
                RenewalPeriod = 60,
                CounterKey = "counter-a"
            });
            context.RateLimitByKey(new RateLimitByKeyConfig
            {
                Calls = 5,
                RenewalPeriod = 120,
                CounterKey = "counter-b"
            });
        }
    }

    class RateLimitByKeyWithIncrementCount : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.RateLimitByKey(new RateLimitByKeyConfig
            {
                Calls = 10,
                RenewalPeriod = 60,
                CounterKey = "heavy-op",
                IncrementCount = 5
            });
        }
    }

    class RateLimitByKeyWithIncrementConditionFalse : IDocument
    {
        public void Inbound(IInboundContext context)
        {
            context.RateLimitByKey(new RateLimitByKeyConfig
            {
                Calls = 3,
                RenewalPeriod = 60,
                CounterKey = "conditional",
                IncrementCondition = false
            });
        }
    }

    private sealed class RecordingRateLimiter(bool shouldAllow) : IRateLimiter
    {
        public int? LastPermits { get; private set; }

        public Task<bool> TryConsumeAsync(string key, int permits = 1, CancellationToken cancellationToken = default)
        {
            LastPermits = permits;
            return Task.FromResult(shouldAllow);
        }
    }

    [TestMethod]
    public void RateLimitByKey_UnderLimit_ShouldIncrementStoreAndSetHeaders()
    {
        var test = new RateLimitByKeyWithHeaders().AsTestDocument();

        test.RunInbound();

        test.SetupRateLimitStore().GetCount("user-1").Should().Be(1);
        test.Context.Response.Headers.Should().ContainKey("X-Remaining")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("2");
        test.Context.Response.Headers.Should().ContainKey("X-Limit")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("3");
        test.Context.Variables.Should().ContainKey("remainingCalls")
            .WhoseValue.Should().Be(2);
    }

    [TestMethod]
    public void RateLimitByKey_Exceeded_ShouldSetHeadersVariablesAndStopSection()
    {
        var test = new RateLimitByKeyThenSetHeader().AsTestDocument();
        var setHeaderExecuted = false;
        test.SetupRateLimitStore().SetCount("user-1", 1);
        test.SetupInbound().SetHeader().WithCallback((_, _, _) => setHeaderExecuted = true);

        test.RunInbound();

        setHeaderExecuted.Should().BeFalse();
        test.Context.Response.StatusCode.Should().Be(429);
        test.Context.Response.StatusReason.Should().Be("Too Many Requests");
        test.Context.Response.Headers.Should().ContainKey("Retry-After")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("30");
        test.Context.Response.Headers.Should().ContainKey("X-Limit")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("1");
        test.Context.Variables.Should().ContainKey("retryAfter")
            .WhoseValue.Should().Be(30);
    }

    [TestMethod]
    public void RateLimitByKey_CounterNotIncrementedOnExceeded()
    {
        var test = new RateLimitByKeyWithHeaders().AsTestDocument();
        test.SetupRateLimitStore().SetCount("user-1", 3);

        test.RunInbound();

        test.Context.Response.StatusCode.Should().Be(429);
        test.SetupRateLimitStore().GetCount("user-1").Should().Be(3);
    }

    [TestMethod]
    public void RateLimitByKey_ResetAndRetry()
    {
        var test = new RateLimitByKeyWithHeaders().AsTestDocument();
        test.SetupRateLimitStore().SetCount("user-1", 3);

        test.RunInbound();
        test.Context.Response.StatusCode.Should().Be(429);

        test.SetupRateLimitStore().Reset();
        test.Context.Response.StatusCode = 200;

        test.RunInbound();

        test.Context.Response.StatusCode.Should().NotBe(429);
    }

    [TestMethod]
    public void RateLimitByKey_DifferentKeysAreIndependent()
    {
        var test = new RateLimitByKeyWithHeaders().AsTestDocument();
        test.SetupRateLimitStore().SetCount("other-key", 100);

        test.RunInbound();

        test.Context.Response.StatusCode.Should().NotBe(429);
    }

    [TestMethod]
    public void RateLimitByKey_IncrementCount()
    {
        var test = new RateLimitByKeyWithIncrementCount().AsTestDocument();

        test.RunInbound();

        test.SetupRateLimitStore().GetCount("heavy-op").Should().Be(5);
    }

    [TestMethod]
    public void RateLimitByKey_IncrementConditionFalse_DoesNotIncrement()
    {
        var test = new RateLimitByKeyWithIncrementConditionFalse().AsTestDocument();

        test.RunInbound();

        test.SetupRateLimitStore().GetCount("conditional").Should().Be(0);
    }

    [TestMethod]
    public void RateLimitByKey_RegisteredRateLimiter_RespectsIncrementCondition()
    {
        var test = new RateLimitByKeyWithIncrementConditionFalse().AsTestDocument();
        var limiter = new RecordingRateLimiter(true);
        test.Context.Services.Register<IRateLimiter>(limiter);

        test.RunInbound();

        limiter.LastPermits.Should().Be(0);
    }

    [TestMethod]
    public void RateLimitByKey_Inbound_Callback()
    {
        var test = new RateLimitByKeyWithHeaders().AsTestDocument();
        var callbackExecuted = false;

        test.SetupInbound().RateLimitByKey().WithCallback((context, config) =>
        {
            callbackExecuted = true;
            context.Variables["counter-key"] = config.CounterKey;
        });

        test.RunInbound();

        callbackExecuted.Should().BeTrue();
        test.Context.Variables.Should().ContainKey("counter-key")
            .WhoseValue.Should().Be("user-1");
        test.SetupRateLimitStore().GetCount("user-1").Should().Be(0);
    }

    [TestMethod]
    public void RateLimitByKey_Inbound_PredicateCallback()
    {
        var test = new MultiRateLimitByKey().AsTestDocument();
        var callbackExecuted = false;

        test.SetupInbound()
            .RateLimitByKey((_, config) => config.CounterKey == "counter-b")
            .WithCallback((context, config) =>
            {
                callbackExecuted = true;
                context.Variables["selected-counter"] = config.CounterKey;
            });

        test.RunInbound();

        callbackExecuted.Should().BeTrue();
        test.Context.Variables.Should().ContainKey("selected-counter")
            .WhoseValue.Should().Be("counter-b");
        test.SetupRateLimitStore().GetCount("counter-a").Should().Be(1);
        test.SetupRateLimitStore().GetCount("counter-b").Should().Be(0);
    }
}
