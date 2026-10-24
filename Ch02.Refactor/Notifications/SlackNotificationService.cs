namespace EduHub.Ch02.Notifications;

public class SlackNotificationService : INotificationService
{
    public void Send(string recipient, string message) =>
        Console.WriteLine($"[Slack] to {recipient}: {message}");
}
