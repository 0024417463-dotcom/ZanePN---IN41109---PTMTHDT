using EduHub.Application.Common;
using EduHub.Application.Registrations;
using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EduHub.Tests.Registrations;

public class RegisterForEventTests
{
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IRegistrationRepository> _regs = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Event _event = Event.Create("Workshop", Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 2);
    private readonly Guid _memberId = Guid.NewGuid();

    public RegisterForEventTests()
    {
        _events.Setup(e => e.GetByIdAsync(_event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_event);
        _regs.Setup(r => r.MemberExistsAsync(_memberId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private RegisterForEventCommandHandler Handler() =>
        new(_events.Object, _regs.Object, _uow.Object, NullLogger<RegisterForEventCommandHandler>.Instance);

    [Fact]
    public async Task Register_Valid_AddsAndSavesOnce()
    {
        var dto = await Handler().HandleAsync(new(_memberId, _event.Id));
        Assert.Equal(_event.Id, dto.EventId);
        _regs.Verify(x => x.Add(It.IsAny<Registration>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_UnknownEvent_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Handler().HandleAsync(new(_memberId, Guid.NewGuid())));
    }

    [Fact]
    public async Task Register_UnknownMember_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Handler().HandleAsync(new(Guid.NewGuid(), _event.Id)));
    }

    [Fact]
    public async Task Register_Duplicate_ThrowsConflict()
    {
        _regs.Setup(x => x.ExistsAsync(_memberId, _event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        await Assert.ThrowsAsync<ConflictException>(() => Handler().HandleAsync(new(_memberId, _event.Id)));
    }

    [Fact]
    public async Task Register_WhenFull_ThrowsBusinessRuleAndDoesNotSave()
    {
        _regs.Setup(x => x.CountActiveByEventIdAsync(_event.Id, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => Handler().HandleAsync(new(_memberId, _event.Id)));

        Assert.Equal("Sự kiện đã đạt giới hạn.", ex.Message);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
