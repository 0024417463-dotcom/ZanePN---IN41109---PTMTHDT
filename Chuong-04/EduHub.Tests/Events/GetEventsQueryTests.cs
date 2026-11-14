using EduHub.Application.Events;
using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using Moq;

namespace EduHub.Tests.Events;

public class GetEventsQueryTests
{
    private static Event Ev(string name) => Event.Create(name, Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 10);

    [Fact]
    public async Task Handle_MapsEntitiesToDtoAndKeepsTotalCount()
    {
        var repo = new Mock<IEventRepository>();
        repo.Setup(r => r.SearchAsync(null, null, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { Ev("A"), Ev("B") }, 42));

        var result = await new GetEventsQueryHandler(repo.Object).HandleAsync(new GetEventsQuery());

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(42, result.TotalCount);
        Assert.Equal("A", result.Items[0].Name);
    }

    [Theory]
    [InlineData(0, 0, 1, 1, 0)]      // page, pageSize không hợp lệ -> bị chuẩn hóa
    [InlineData(3, 10, 3, 10, 20)]   // trang 3, mỗi trang 10 -> bỏ qua 20 dòng
    [InlineData(1, 500, 1, 100, 0)]  // pageSize tối đa 100
    public async Task Handle_NormalizesPagingBeforeCallingRepository(int page, int pageSize, int expPage, int expSize, int expSkip)
    {
        var repo = new Mock<IEventRepository>();
        repo.Setup(r => r.SearchAsync(null, null, expSkip, expSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Event>(), 0)).Verifiable();

        var result = await new GetEventsQueryHandler(repo.Object)
            .HandleAsync(new GetEventsQuery(Page: page, PageSize: pageSize));

        repo.Verify();
        Assert.Equal(expPage, result.Page);
        Assert.Equal(expSize, result.PageSize);
    }
}
