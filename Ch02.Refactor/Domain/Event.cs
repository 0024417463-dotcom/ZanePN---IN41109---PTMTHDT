namespace EduHub.Ch02.Domain;

/// <summary>Sự kiện của CLB, kèm danh sách đăng ký và check-in.</summary>
public class Event
{
    public Guid Id { get; set; }
    public Guid ClubId { get; set; }
    public required string Title { get; set; }
    public DateTime Date { get; set; }

    public HashSet<Guid> RegisteredMemberIds { get; } = new();
    public HashSet<Guid> CheckedInMemberIds { get; } = new();

    /// <summary>Đăng ký tham dự; false nếu thành viên đã đăng ký trước đó.</summary>
    public bool Register(Guid memberId) => RegisteredMemberIds.Add(memberId);

    /// <summary>Check-in; chỉ thành viên đã đăng ký mới được phép.</summary>
    public void CheckIn(Guid memberId)
    {
        if (!RegisteredMemberIds.Contains(memberId))
            throw new InvalidOperationException("Member has not registered for this event");
        CheckedInMemberIds.Add(memberId);
    }
}
