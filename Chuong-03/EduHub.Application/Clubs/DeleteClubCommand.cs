using EduHub.Domain.Interfaces;

namespace EduHub.Application.Clubs;

public record DeleteClubCommand(Guid Id);

public class DeleteClubCommandHandler
{
    private readonly IClubRepository _repository;

    public DeleteClubCommandHandler(IClubRepository repository)
    {
        _repository = repository;
    }

    public Task HandleAsync(DeleteClubCommand command, CancellationToken ct = default)
        => _repository.DeleteAsync(command.Id, ct);
}
