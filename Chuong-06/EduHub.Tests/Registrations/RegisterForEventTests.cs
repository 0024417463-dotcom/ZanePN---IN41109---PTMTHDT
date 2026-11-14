using EduHub.Application.Registrations;
using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EduHub.Tests.Registrations;

public class RegisterForEventTests
{
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IRegistrationRepository> _regs = new();
    private readonly Event _event = Event.Create("Workshop", Guid.NewGuid(), DateTime.UtcNow, 2);

    public RegisterForEventTests()
        => _events.Setup(e => e.GetByIdAsync(_event.Id, default)).ReturnsAsync(_event);

    private RegisterForEventCommandHandler Handler() => new(_events.Object, _regs.Object);

    [Fact]
    public async Task Register_WhenEventNotFull_CreatesRegistration()
    {
        _regs.Setup(r => r.CountByEventAsync(_event.Id, default)).ReturnsAsync(1);
        var memberId = Guid.NewGuid();

        var reg = await Handler().HandleAsync(new RegisterForEventCommand(_event.Id, memberId));

        reg.MemberId.Should().Be(memberId);
        reg.Status.Should().Be("Confirmed");
        _regs.Verify(r => r.AddAsync(reg, default), Times.Once);
    }

    [Fact]
    public async Task Register_WhenEventFull_ThrowsException()
    {
        _regs.Setup(r => r.CountByEventAsync(_event.Id, default)).ReturnsAsync(2);

        var act = () => Handler().HandleAsync(new RegisterForEventCommand(_event.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*đủ chỗ*");
        _regs.Verify(r => r.AddAsync(It.IsAny<Registration>(), default), Times.Never);
    }

    [Fact]
    public async Task Register_Duplicate_ThrowsException()
    {
        var memberId = Guid.NewGuid();
        _regs.Setup(r => r.ExistsAsync(_event.Id, memberId, default)).ReturnsAsync(true);

        var act = () => Handler().HandleAsync(new RegisterForEventCommand(_event.Id, memberId));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*đã đăng ký*");
    }

    [Fact]
    public async Task Register_WhenEventNotFound_ThrowsKeyNotFound()
    {
        _events.Setup(e => e.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Event?)null);

        var act = () => Handler().HandleAsync(new RegisterForEventCommand(Guid.NewGuid(), Guid.NewGuid()));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
