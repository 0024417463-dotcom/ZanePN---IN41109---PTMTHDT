using EduHub.Application.Events;
using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EduHub.Tests.Events;

public class CreateEventCommandTests
{
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IClubRepository> _clubs = new();

    private CreateEventCommandHandler Handler() => new(_events.Object, _clubs.Object);

    [Fact]
    public async Task CreateEvent_WithValidData_ReturnsEvent()
    {
        var club = Club.Create("A");
        _clubs.Setup(c => c.GetByIdAsync(club.Id, default)).ReturnsAsync(club);

        var ev = await Handler().HandleAsync(new CreateEventCommand(club.Id, "Workshop", 50, DateTime.UtcNow));

        ev.Capacity.Should().Be(50);
        _events.Verify(e => e.AddAsync(ev, default), Times.Once);
    }

    [Fact]
    public async Task CreateEvent_WithNegativeCapacity_ThrowsException()
    {
        var club = Club.Create("A");
        _clubs.Setup(c => c.GetByIdAsync(club.Id, default)).ReturnsAsync(club);

        var act = () => Handler().HandleAsync(new CreateEventCommand(club.Id, "Workshop", -5, DateTime.UtcNow));

        await act.Should().ThrowAsync<ArgumentException>();
        _events.Verify(e => e.AddAsync(It.IsAny<Event>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateEvent_WhenClubNotFound_ThrowsKeyNotFound()
    {
        _clubs.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Club?)null);

        var act = () => Handler().HandleAsync(new CreateEventCommand(Guid.NewGuid(), "X", 10, DateTime.UtcNow));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
