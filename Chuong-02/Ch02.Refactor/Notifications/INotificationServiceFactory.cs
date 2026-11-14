namespace EduHub.Ch02.Notifications;

/// <summary>Factory: tạo INotificationService theo kênh, thay cho chuỗi if/else ở ClubManager cũ.</summary>
public interface INotificationServiceFactory
{
    INotificationService Create(NotificationChannel channel);
}
