using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;

namespace EduHub.Application.Events;

public record GetEventsQuery(Guid? ClubId = null, string? Search = null, int Page = 1, int PageSize = 10);

public class GetEventsQueryHandler(IEventRepository events)
{
    // Lọc theo club + từ khóa, phân trang trong SQL, trả DTO
    public async Task<PagedResult<EventDto>> HandleAsync(GetEventsQuery query, CancellationToken ct = default)
    {
        var paging = PageRequest.Of(query.Page, query.PageSize);
        var (items, total) = await events.SearchAsync(query.ClubId, query.Search, paging.Skip, paging.PageSize, ct);
        return new PagedResult<EventDto>(items.Select(e => e.ToDto()).ToList(), total, paging.Page, paging.PageSize);
    }
}
