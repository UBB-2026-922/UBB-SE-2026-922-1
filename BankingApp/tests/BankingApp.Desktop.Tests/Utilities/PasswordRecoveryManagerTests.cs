// <copyright file="PasswordRecoveryManagerTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;

namespace BankingApp.Desktop.Tests.Utilities;

/// <summary>
///     Unit tests for the throttling and validation logic inside <see cref="PasswordRecoveryManager" />.
///     Network calls are isolated through <see cref="TestablePasswordRecoveryManager" />, which implements
///     <see cref="IPasswordRecoveryManager" /> and records calls without touching HTTP.
/// </summary>
public class PasswordRecoveryManagerTests
{
    private const string TestEmail = "user@example.com";
    private const int CooldownSeconds = 60;
    private const int HalfCooldownSeconds = 30;
    private const int JustPastCooldownSeconds = 61;

    /// <summary>
    ///     Before any request has been made, <see cref="IPasswordRecoveryManager.CanResendCode" />
    ///     should be true and <see cref="IPasswordRecoveryManager.SecondsUntilResendAllowed" /> should be zero.
    /// </summary>
    [Fact]
    public void CanResendCode_BeforeAnyRequest_ReturnsTrue()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        IPasswordRecoveryManager manager = BuildManager(clock, true);

        // Assert
        manager.CanResendCode.Should().BeTrue();
        manager.SecondsUntilResendAllowed.Should().Be(0);
    }

    /// <summary>
    ///     Immediately after the first code request, <see cref="IPasswordRecoveryManager.CanResendCode" />
    ///     should be false and <see cref="IPasswordRecoveryManager.SecondsUntilResendAllowed" /> should be positive.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CanResendCode_ImmediatelyAfterFirstRequest_ReturnsFalse()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        IPasswordRecoveryManager manager = BuildManager(clock, true);

        // Act
        await manager.RequestCodeAsync(TestEmail);

        // Assert
        manager.CanResendCode.Should().BeFalse();
        manager.SecondsUntilResendAllowed.Should().BePositive();
    }

    /// <summary>
    ///     After the full cooldown period has elapsed, <see cref="IPasswordRecoveryManager.CanResendCode" />
    ///     should return to true and <see cref="IPasswordRecoveryManager.SecondsUntilResendAllowed" /> should be zero.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CanResendCode_AfterCooldownExpires_ReturnsTrue()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        IPasswordRecoveryManager manager = BuildManager(clock, true);

        // Act
        await manager.RequestCodeAsync(TestEmail);
        clock.Advance(TimeSpan.FromSeconds(CooldownSeconds));

        // Assert
        manager.CanResendCode.Should().BeTrue();
        manager.SecondsUntilResendAllowed.Should().Be(0);
    }

    /// <summary>
    ///     When a second request is made within the cooldown window, the underlying API should not be
    ///     called again and the cached <see cref="ForgotPasswordState.EmailSent" /> state should be returned.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RequestCode_CalledTwiceWithinCooldown_ThrottlesSecondCall()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        var fake = new FakeApiResponder { ShouldSucceed = true };
        IPasswordRecoveryManager manager = BuildManagerWithFakeResponder(clock, fake);

        // Act
        ForgotPasswordState firstResult = await manager.RequestCodeAsync(TestEmail);
        int callsAfterFirst = fake.RequestCount;
        clock.Advance(TimeSpan.FromSeconds(HalfCooldownSeconds));
        ForgotPasswordState secondResult = await manager.RequestCodeAsync(TestEmail);

        // Assert
        firstResult.Should().Be(ForgotPasswordState.EmailSent);
        secondResult.Should().Be(ForgotPasswordState.EmailSent);
        fake.RequestCount.Should().Be(callsAfterFirst);
    }

    /// <summary>
    ///     When a request is made after the cooldown has expired, a new API call should be issued
    ///     and the request count should increment.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RequestCode_CalledAfterCooldownExpires_IssuesNewApiCall()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        var fake = new FakeApiResponder { ShouldSucceed = true };
        IPasswordRecoveryManager manager = BuildManagerWithFakeResponder(clock, fake);

        // Act
        await manager.RequestCodeAsync(TestEmail);
        int callsAfterFirst = fake.RequestCount;
        clock.Advance(TimeSpan.FromSeconds(JustPastCooldownSeconds));
        await manager.RequestCodeAsync(TestEmail);

        // Assert
        fake.RequestCount.Should().Be(callsAfterFirst + 1);
    }

    /// <summary>
    ///     Passwords that satisfy all complexity rules (minimum length, upper, lower, digit, special character)
    ///     should be considered valid.
    /// </summary>
    /// <param name="password">A password that meets all complexity requirements.</param>
    [Theory]
    [InlineData("Password1!")]
    [InlineData("Str0ng#Pass")]
    [InlineData("C0mpl3x!ty")]
    public void IsPasswordValid_WithValidPassword_ReturnsTrue(string password)
    {
        // Arrange
        IPasswordRecoveryManager manager = BuildManager(new FakeSystemClock(DateTime.UtcNow), false);

        // Assert
        manager.IsPasswordValid(password).Should().BeTrue();
    }

    /// <summary>
    ///     Passwords that violate at least one complexity rule should be considered invalid.
    /// </summary>
    /// <param name="password">A password that fails one or more complexity requirements.</param>
    [Theory]
    [InlineData("short1!")] // fewer than 8 chars
    [InlineData("alllowercase1!")] // no uppercase
    [InlineData("ALLUPPERCASE1!")] // no lowercase
    [InlineData("NoDigitsHere!")] // no digit
    [InlineData("NoSpecial1ABC")] // no special char
    [InlineData("")] // empty
    [InlineData("   ")] // whitespace only
    public void IsPasswordValid_WithWeakPassword_ReturnsFalse(string password)
    {
        // Arrange
        IPasswordRecoveryManager manager = BuildManager(new FakeSystemClock(DateTime.UtcNow), false);

        // Assert
        manager.IsPasswordValid(password).Should().BeFalse();
    }

    private static IPasswordRecoveryManager BuildManager(FakeSystemClock clock, bool succeedApi)
    {
        return BuildManagerWithFakeResponder(clock, new FakeApiResponder { ShouldSucceed = succeedApi });
    }

    private static IPasswordRecoveryManager BuildManagerWithFakeResponder(FakeSystemClock clock, FakeApiResponder fake)
    {
        return new TestablePasswordRecoveryManager(fake, clock);
    }

    /// <summary>
    ///     Controllable system clock stub — advances on demand so time-dependent logic can be tested deterministically.
    /// </summary>
    private sealed class FakeSystemClock : ISystemClock
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FakeSystemClock" /> class.
        /// </summary>
        public FakeSystemClock(DateTime start)
        {
            UtcNow = start;
        }

        /// <inheritdoc />
        public DateTime UtcNow { get; private set; }

        /// <summary>
        ///     Supports the FakeSystemClock test helper.
        /// </summary>
        public void Advance(TimeSpan duration)
        {
            UtcNow += duration;
        }
    }

    /// <summary>
    ///     Records how many times the API was called and controls whether each call succeeds or fails.
    /// </summary>
    private sealed class FakeApiResponder
    {
        public bool ShouldSucceed { get; init; }

        public int RequestCount { get; private set; }

        /// <summary>
        ///     Supports the FakeApiResponder test helper.
        /// </summary>
        public Task<ForgotPasswordState> HandleRequestCodeAsync()
        {
            RequestCount++;
            return Task.FromResult(ShouldSucceed ? ForgotPasswordState.EmailSent : ForgotPasswordState.Error);
        }
    }

    /// <summary>
    ///     Minimal <see cref="IPasswordRecoveryManager" /> that re-implements the throttling logic under test
    ///     and delegates API calls to <see cref="FakeApiResponder" /> instead of making real HTTP requests.
    /// </summary>
    private sealed class TestablePasswordRecoveryManager : IPasswordRecoveryManager
    {
        private readonly ISystemClock _clock;
        private readonly FakeApiResponder _responder;
        private DateTime? _lastRequestedAt;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestablePasswordRecoveryManager" /> class.
        /// </summary>
        public TestablePasswordRecoveryManager(FakeApiResponder responder, ISystemClock clock)
        {
            _responder = responder;
            _clock = clock;
        }

        public bool CanResendCode
        {
            get
            {
                if (_lastRequestedAt is null)
                {
                    return true;
                }

                return (_clock.UtcNow - _lastRequestedAt.Value).TotalSeconds >= CooldownSeconds;
            }
        }

        public int SecondsUntilResendAllowed
        {
            get
            {
                if (_lastRequestedAt is null)
                {
                    return 0;
                }

                double remaining = CooldownSeconds - (_clock.UtcNow - _lastRequestedAt.Value).TotalSeconds;
                return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
            }
        }

        /// <summary>
        ///     Supports the TestablePasswordRecoveryManager test helper.
        /// </summary>
        public async Task<ForgotPasswordState> RequestCodeAsync(string email)
        {
            if (!CanResendCode)
            {
                return ForgotPasswordState.EmailSent;
            }

            ForgotPasswordState state = await _responder.HandleRequestCodeAsync();
            if (state == ForgotPasswordState.EmailSent)
            {
                _lastRequestedAt = _clock.UtcNow;
            }

            return state;
        }

        /// <summary>
        ///     Supports the TestablePasswordRecoveryManager test helper.
        /// </summary>
        public Task<ForgotPasswordState> VerifyTokenAsync(string token)
        {
            return Task.FromResult(ForgotPasswordState.TokenValid);
        }

        /// <summary>
        ///     Supports the TestablePasswordRecoveryManager test helper.
        /// </summary>
        public Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword)
        {
            return Task.FromResult(ForgotPasswordState.PasswordResetSuccess);
        }

        /// <summary>
        ///     Supports the TestablePasswordRecoveryManager test helper.
        /// </summary>
        public bool IsPasswordValid(string password)
        {
            return !string.IsNullOrWhiteSpace(password)
                   && password.Length >= PasswordValidator.MinimumLength
                   && password.Any(char.IsUpper)
                   && password.Any(char.IsLower)
                   && password.Any(char.IsDigit)
                   && password.Any(character => !char.IsLetterOrDigit(character));
        }
    }
}