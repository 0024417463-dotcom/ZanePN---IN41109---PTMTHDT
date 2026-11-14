using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EduHub.Application.Registrations;

public record RegisterForEventCommand(Guid MemberId, Guid EventId);

public class RegisterForEventCommandHandler(IEventRepository events, IRegistrationRepository registrations,
    IUnitOfWork uow, ILogger<RegisterForEventCommandHandler> logger)
{
    // Thứ tự kiểm tra: Event tồn tại (404) -> Member tồn tại (404) -> chưa đăng ký (409) -> chưa đầy (400)
    public async Task<RegistrationDto> HandleAsync(RegisterForEventCommand command, CancellationToken ct = default)
    {
        var ev = await events.GetByIdAsync(command.EventId, ct)
                 ?? throw new NotFoundException("Sự kiện không tồn tại.");

        if (!await registrations.MemberExistsAsync(command.MemberId, ct))
            throw new NotFoundException("Thành viên không tồn tại.");

        if (await registrations.ExistsAsync(command.MemberId, command.EventId, ct))
            throw new ConflictException("Thành viên đã đăng ký sự kiện này.");

        var count = await registrations.CountActiveByEventIdAsync(command.EventId, ct);
        if (ev.IsFull(count))
        {
            logger.LogWarning("Sự kiện {EventId} đã đầy ({Count}/{Capacity}), từ chối thành viên {MemberId}",
                ev.Id, count, ev.Capacity, command.MemberId);
            throw new BusinessRuleException("Sự kiện đã đạt giới hạn.");
        }

        var registration = Registration.Create(command.MemberId, command.EventId);
        registrations.Add(registration);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Thành viên {MemberId} đăng ký sự kiện {EventId}", command.MemberId, command.EventId);
        return registration.ToDto();
    }
}
