using EduHub.Application.Common;
using EduHub.Application.Events;
using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EduHub.Tests.Events;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IClubRepository> _clubs = new();
    private readonly Mock<IRegistrationRepository> _regs = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private EventService Service() =>
        new(_events.Object, _clubs.Object, _regs.Object, _uow.Object, NullLogger<EventService>.Instance);

    private static CreateEventCommand Cmd(Guid clubId) =>
        new("Workshop", null, clubId, null, DateTime.UtcNow.AddDays(3), 50);

    [Fact]
    public async Task Create_UnknownClub_ThrowsBusinessRuleAndDoesNotSave()
    {
        _clubs.Setup(c => c.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().CreateAsync(Cmd(Guid.NewGuid())));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_Valid_AddsAndSavesOnce()
    {
        var clubId = Guid.NewGuid();
        _clubs.Setup(c => c.ExistsAsync(clubId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var dto = await Service().CreateAsync(Cmd(clubId));

        Assert.Equal("Workshop", dto.Name);
        _events.Verify(e => e.Add(It.IsAny<Event>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_Missing_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Service().GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Update_CapacityBelowRegistered_ThrowsBusinessRule()
    {
        var ev = Event.Create("Workshop", Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 10);
        _events.Setup(e => e.GetByIdAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ev);
        _regs.Setup(r => r.CountActiveByEventIdAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(7);

        var cmd = new UpdateEventCommand("Workshop", null, null, DateTime.UtcNow.AddDays(2), 5);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().UpdateAsync(ev.Id, cmd));
    }
}
