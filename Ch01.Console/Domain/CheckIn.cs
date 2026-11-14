namespace EduHub.Ch01.Domain;

/// <summary>Các phương thức check-in hợp lệ.</summary>
public static class CheckInMethod
{
    public const string Manual = "Manual";
    public const string QrCode = "QrCode";
}

/// <summary>Ghi nhận sự hiện diện thực tế của một Registration (quan hệ 1–1).</summary>
public class CheckIn
{
    public Guid Id { get; private set; }
    public Guid RegistrationId { get; private set; }
    public DateTime CheckedInAt { get; private set; }
    public string Method { get; private set; }

    private CheckIn(Guid registrationId, string method)
    {
        Id = Guid.NewGuid();
        RegistrationId = registrationId;
        CheckedInAt = DateTime.UtcNow;
        Method = method;
    }

    public static CheckIn Create(Registration registration, string method)
    {
        ArgumentNullException.ThrowIfNull(registration);

        if (registration.Status != RegistrationStatus.Confirmed)
            throw new InvalidOperationException(
                $"Chỉ check-in được phiếu đã Confirmed (hiện tại: {registration.Status}).");
        if (method != CheckInMethod.Manual && method != CheckInMethod.QrCode)
            throw new ArgumentException($"Method phải là \"{CheckInMethod.Manual}\" hoặc \"{CheckInMethod.QrCode}\".", nameof(method));

        return new CheckIn(registration.Id, method);
    }

    public string Summary() => $"Check-in {Id.ToString()[..8]} | Registration {RegistrationId.ToString()[..8]} | {Method} | {CheckedInAt:HH:mm:ss} UTC";
}
