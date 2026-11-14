using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Application.Clubs;

public record CreateClubCommand(string Name, string Description);

public class CreateClubCommandHandler
{
    private const int MaxNameLength = 50;
    private readonly IClubRepository _repository;

    public CreateClubCommandHandler(IClubRepository repository)
    {
        _repository = repository;
    }

    public async Task<Club> HandleAsync(CreateClubCommand command, CancellationToken ct = default)
    {
        // 4a: tên không được rỗng / toàn khoảng trắng
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Tên câu lạc bộ không được để trống.");

        // 4b: tên tối đa 50 ký tự
        var name = command.Name.Trim();
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Tên câu lạc bộ tối đa {MaxNameLength} ký tự.");

        // 4c: tạo entity và lưu
        var club = Club.Create(name, command.Description?.Trim() ?? string.Empty);
        await _repository.AddAsync(club, ct);

        // 4d: trả về club vừa tạo
        return club;
    }
}
