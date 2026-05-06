using BankingApp.Infrastructure.Services;

namespace BankingApp.Infrastructure.Tests.Services;

/// <summary>
///     Tests for <see cref="OtpAttemptTracker" /> state tracking behavior.
/// </summary>
public class OtpAttemptTrackerTests
{
    [Fact]
    public void RecordFailure_WhenFirstFailureForUser_ReturnsOne()
    {
        // Arrange
        var otpAttemptTracker = new OtpAttemptTracker();
        const int userId = 123;

        // Act
        int attemptCount = otpAttemptTracker.RecordFailure(userId);

        // Assert
        attemptCount.Should().Be(1);
    }

    [Fact]
    public void RecordFailure_WhenCalledMultipleTimesForSameUser_IncrementsCount()
    {
        // Arrange
        var otpAttemptTracker = new OtpAttemptTracker();
        const int userId = 456;

        // Act
        int firstAttemptCount = otpAttemptTracker.RecordFailure(userId);
        int secondAttemptCount = otpAttemptTracker.RecordFailure(userId);

        // Assert
        firstAttemptCount.Should().Be(1);
        secondAttemptCount.Should().Be(2);
    }

    [Fact]
    public void RecordFailure_WhenCalledForDifferentUsers_TracksAttemptsIndependently()
    {
        // Arrange
        var otpAttemptTracker = new OtpAttemptTracker();
        const int firstUserId = 100;
        const int secondUserId = 200;

        // Act
        int firstUserFirstAttempt = otpAttemptTracker.RecordFailure(firstUserId);
        int secondUserFirstAttempt = otpAttemptTracker.RecordFailure(secondUserId);
        int firstUserSecondAttempt = otpAttemptTracker.RecordFailure(firstUserId);

        // Assert
        firstUserFirstAttempt.Should().Be(1);
        secondUserFirstAttempt.Should().Be(1);
        firstUserSecondAttempt.Should().Be(2);
    }

    [Fact]
    public void Reset_WhenUserHasFailures_ClearsCountForNextFailure()
    {
        // Arrange
        var otpAttemptTracker = new OtpAttemptTracker();
        const int userId = 300;
        _ = otpAttemptTracker.RecordFailure(userId);
        _ = otpAttemptTracker.RecordFailure(userId);

        // Act
        otpAttemptTracker.Reset(userId);
        int attemptCountAfterReset = otpAttemptTracker.RecordFailure(userId);

        // Assert
        attemptCountAfterReset.Should().Be(1);
    }

    [Fact]
    public void Reset_WhenUserHasNoFailures_DoesNotThrowAndNextFailureStartsAtOne()
    {
        // Arrange
        var otpAttemptTracker = new OtpAttemptTracker();
        const int userId = 400;

        // Act
        Exception? resetException = Record.Exception(() => otpAttemptTracker.Reset(userId));
        int attemptCountAfterReset = otpAttemptTracker.RecordFailure(userId);

        // Assert
        resetException.Should().BeNull();
        attemptCountAfterReset.Should().Be(1);
    }
}
