namespace EduHub.Ch02.Notifications;

/// <summary>
/// Factory: chọn INotificationService theo kênh. Thêm kênh mới chỉ cần thêm một
/// nhánh ở đây (và một enum), các service nghiệp vụ không phải sửa (OCP).
/// </summary>
public class NotificationServiceFactory : INotificationServiceFactory
{
    public INotificationService Create(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => new EmailNotificationService(),
        NotificationChannel.Sms => new SmsNotificationService(),
        NotificationChannel.Slack => new SlackNotificationService(),
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unsupported channel")
    };
}
