namespace EduHub.Ch01.Domain;

/// <summary>Các trạng thái hợp lệ của phiếu đăng ký (tránh magic string).</summary>
public static class RegistrationStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
}

/// <summary>Phiếu đăng ký của Member cho Event (bảng nối M:N).</summary>
public class Registration
{
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public Guid EventId { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public string Status { get; private set; }

    private Registration(Guid memberId, Guid eventId)
    {
        Id = Guid.NewGuid();
        MemberId = memberId;
        EventId = eventId;
        RegisteredAt = DateTime.UtcNow;
        Status = RegistrationStatus.Pending;
    }

    public static Registration Create(Guid memberId, Guid eventId)
    {
        if (memberId == Guid.Empty)
            throw new ArgumentException("MemberId không hợp lệ.", nameof(memberId));
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId không hợp lệ.", nameof(eventId));

        return new Registration(memberId, eventId);
    }

    /// <summary>Pending → Confirmed.</summary>
    public void Confirm()
    {
        if (Status != RegistrationStatus.Pending)
            throw new InvalidOperationException($"Chỉ xác nhận được phiếu đang Pending (hiện tại: {Status}).");
        Status = RegistrationStatus.Confirmed;
    }

    /// <summary>Pending/Confirmed → Cancelled.</summary>
    public void Cancel()
    {
        if (Status == RegistrationStatus.Cancelled)
            throw new InvalidOperationException("Phiếu đăng ký đã bị huỷ trước đó.");
        Status = RegistrationStatus.Cancelled;
    }
}
