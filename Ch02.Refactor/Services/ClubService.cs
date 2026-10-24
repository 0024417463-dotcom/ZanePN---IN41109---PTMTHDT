using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

/// <summary>Nghiệp vụ CLB và thành viên. Mọi phụ thuộc nhận qua constructor dưới dạng interface.</summary>
public class ClubService : IClubService
{
    private readonly IClubRepository _clubs;
    private readonly IMemberRepository _members;
    private readonly INotificationServiceFactory _notifications;
    private readonly IAppLogger _logger;

    public ClubService(
        IClubRepository clubs,
        IMemberRepository members,
        INotificationServiceFactory notifications,
        IAppLogger logger)
    {
        _clubs = clubs;
        _members = members;
        _notifications = notifications;
        _logger = logger;
    }

    public Club CreateClub(string name, string description, string presidentEmail)
    {
        var club = Club.Create(name, description); // validate nằm ở Domain
        _clubs.Add(club);
        _logger.Info($"Club created: {club.Name}");

        _notifications.Create(NotificationChannel.Email)
            .Send(presidentEmail, $"Club '{club.Name}' has been created");
        return club;
    }

    public Member AddMember(Guid clubId, string memberName, string email)
    {
        var club = _clubs.GetById(clubId)
            ?? throw new InvalidOperationException("Club not found");

        if (!club.HasRoom(_members.CountByClub(clubId)))
            throw new InvalidOperationException($"Club is full (max {Club.MaxMembers} members)");

        var member = new Member { Id = Guid.NewGuid(), Name = memberName, Email = email, ClubId = clubId };
        _members.Add(member);
        _logger.Info($"Member {memberName} joined {club.Name}");

        _notifications.Create(NotificationChannel.Email).Send(email, $"Welcome to {club.Name}");
        _notifications.Create(NotificationChannel.Slack).Send("#clubs", $"{memberName} joined {club.Name}");
        return member;
    }

    public IReadOnlyList<Club> GetAllClubs() => _clubs.GetAll();
}
