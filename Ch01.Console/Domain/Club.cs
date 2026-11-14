namespace EduHub.Ch01.Domain;

/// <summary>Câu lạc bộ sinh viên.</summary>
public class Club
{
    private readonly List<Member> _members = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>Chỉ đọc: bên ngoài phải dùng <see cref="AddMember"/> để thêm.</summary>
    public IReadOnlyList<Member> Members => _members.AsReadOnly();

    private Club(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static Club Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên câu lạc bộ không được để trống.", nameof(name));

        return new Club(name.Trim(), description?.Trim() ?? string.Empty);
    }

    public void AddMember(Member member)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (_members.Any(m => m.Id == member.Id || m.StudentCode == member.StudentCode))
            throw new InvalidOperationException($"Thành viên {member} đã có trong câu lạc bộ.");

        _members.Add(member);
    }

    public int MemberCount() => _members.Count;
}
