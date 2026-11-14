namespace EduHub.Ch01.Domain;

/// <summary>Ghi nhận sự hiện diện thực tế của một Registration (quan hệ 1–1).</summary>
public class CheckIn
{
    public const string Manual = "Manual";
    public const string QrCode = "QrCode";

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
        if (method != Manual && method != QrCode)
            throw new ArgumentException($"Method phải là \"{Manual}\" hoặc \"{QrCode}\".", nameof(method));
        if (registration.Status != RegistrationStatus.Confirmed)
            throw new InvalidOperationException("Chỉ check-in được khi đăng ký đã Confirmed.");
        return new CheckIn(registration.Id, method);
    }

    public string Summary() => $"Check-in {Id.ToString()[..8]} | đăng ký {RegistrationId.ToString()[..8]} | {Method} | {CheckedInAt:HH:mm:ss} UTC";
}
