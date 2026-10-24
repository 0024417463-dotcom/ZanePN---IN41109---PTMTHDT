using EduHub.Ch02.Domain;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Patterns.Observers;

/// <summary>Gửi email cho từng thành viên của CLB khi có sự kiện mới.</summary>
public class EmailNotifier : IEventObserver
{
    private readonly IMemberRepository _members;
    private readonly INotificationService _email;

    public EmailNotifier(IMemberRepository members, INotificationService email)
    {
        _members = members;
        _email = email;
    }

    public void OnEventCreated(Event ev, Club club)
    {
        foreach (var m in _members.GetByClub(club.Id))
            _email.Send(m.Email, $"new event '{ev.Title}' at {club.Name}");
    }
}
