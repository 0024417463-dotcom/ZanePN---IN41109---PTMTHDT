using EduHub.Domain.Entities;
using FluentAssertions;

namespace EduHub.Tests.Events;

public class CheckInTests
{
    private static readonly DateTime Now = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Verify_FirstTime_SetsIsVerifiedAndTime()
    {
        var checkIn = CheckIn.CreateForRegistration(Guid.NewGuid());

        checkIn.Verify(Now);

        checkIn.IsVerified.Should().BeTrue();
        checkIn.CheckedInAt.Should().Be(Now);
    }

    [Fact]
    public void Verify_SecondTime_ThrowsInvalidOperation()
    {
        var checkIn = CheckIn.CreateForRegistration(Guid.NewGuid());
        checkIn.Verify(Now);

        var act = () => checkIn.Verify(Now.AddMinutes(1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateEvent_WhenNameEmpty_ThrowsArgumentException()
    {
        var act = () => Event.Create(" ", Guid.NewGuid(), DateTime.UtcNow, 10);

        act.Should().Throw<ArgumentException>();
    }
}
