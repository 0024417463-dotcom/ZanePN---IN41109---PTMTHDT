using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace EduHub.Application.Registrations;

public class GetEventRegistrationsQueryHandler(IEventRepository events, IRegistrationRepository registrations)
{
    public async Task<IReadOnlyList<RegistrationMemberDto>> HandleAsync(Guid eventId, CancellationToken ct = default)
    {
        if (await events.GetByIdAsync(eventId, ct) is null)
            throw new NotFoundException("Sự kiện không tồn tại.");
        var list = await registrations.GetByEventAsync(eventId, ct);
        return list.Select(x => x.ToDto()).ToList();
    }
}

public class CancelRegistrationCommandHandler(IRegistrationRepository registrations, IUnitOfWork uow,
    ILogger<CancelRegistrationCommandHandler> logger)
{
    // Hủy = đổi trạng thái sang Cancelled (giữ lịch sử), không xóa dòng
    public async Task HandleAsync(Guid eventId, Guid registrationId, CancellationToken ct = default)
    {
        var reg = await registrations.GetAsync(eventId, registrationId, ct)
                  ?? throw new NotFoundException("Đăng ký không tồn tại.");
        reg.Cancel();
        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Đã hủy đăng ký {RegistrationId} của sự kiện {EventId}", registrationId, eventId);
    }
}
