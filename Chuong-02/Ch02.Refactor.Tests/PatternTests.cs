using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;
using EduHub.Ch02.Services;

namespace Ch02.Refactor.Tests;

public class PatternTests
{
    [Theory]
    [InlineData(NotificationChannel.Email, typeof(EmailNotificationService))]
    [InlineData(NotificationChannel.Sms, typeof(SmsNotificationService))]
    [InlineData(NotificationChannel.Slack, typeof(SlackNotificationService))]
    public void Factory_Create_ReturnsServiceForChannel(NotificationChannel channel, Type expected)
    {
        // Arrange
        var factory = new NotificationServiceFactory();

        // Act
        var service = factory.Create(channel);

        // Assert
        Assert.IsType(expected, service);
    }

    [Fact]
    public void Factory_Create_UnknownChannel_Throws()
    {
        // Arrange
        var factory = new NotificationServiceFactory();

        // Act
        var act = () => factory.Create((NotificationChannel)99);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void LoggingClubRepository_Add_LogsAndDelegatesToInnerRepository()
    {
        // Arrange
        var logger = new ListLogger();
        var inner = new InMemoryClubRepository();
        var repo = new LoggingClubRepository(inner, logger);
        var club = Club.Create("Tech Club", "mô tả");

        // Act
        repo.Add(club);

        // Assert
        Assert.Single(inner.GetAll());
        Assert.Contains(logger.Lines, l => l.Contains("Tech Club"));
    }

    [Fact]
    public void EventService_CreateEvent_NotifiesAllSubscribedObservers()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var club = Club.Create("Tech Club", "mô tả");
        clubs.Add(club);
        var service = new EventService(clubs);
        var first = new SpyObserver();
        var second = new SpyObserver();
        service.Subscribe(first);
        service.Subscribe(second);

        // Act
        var ev = service.CreateEvent(club.Id, "Workshop", new DateTime(2026, 11, 20));

        // Assert
        Assert.Same(ev, first.Received);
        Assert.Same(ev, second.Received);
    }

    [Fact]
    public void EventService_CreateEvent_UnknownClub_Throws()
    {
        // Arrange
        var service = new EventService(new InMemoryClubRepository());

        // Act
        var act = () => service.CreateEvent(Guid.NewGuid(), "Workshop", DateTime.Now);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void CsvReportStrategy_Generate_WritesHeaderAndEscapedRows()
    {
        // Arrange
        var strategy = new CsvReportStrategy();
        var writer = new StringWriter();
        var stats = new List<ClubStat> { new("Tech, Club", 3) };

        // Act
        strategy.Generate(stats, writer);

        // Assert
        var lines = writer.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("Club,Members", lines[0]);
        Assert.Equal("\"Tech, Club\",3", lines[1]);
    }

    [Fact]
    public void ReportService_Generate_UsesGivenStrategyWithMemberCounts()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var members = new InMemoryMemberRepository();
        var club = Club.Create("Tech Club", "mô tả");
        clubs.Add(club);
        members.Add(new Member { Id = Guid.NewGuid(), Name = "A", Email = "a@x.vn", ClubId = club.Id });
        var writer = new StringWriter();

        // Act
        new ReportService(clubs, members).Generate(new ConsoleReportStrategy(), writer);

        // Assert
        Assert.Contains("Club: Tech Club | Members: 1", writer.ToString());
    }

    [Fact]
    public void ClubService_AddMember_WhenClubIsFull_ThrowsInvalidOperationException()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var members = new InMemoryMemberRepository();
        var club = Club.Create("Tech Club", "mô tả");
        clubs.Add(club);
        for (var i = 0; i < Club.MaxMembers; i++)
            members.Add(new Member { Id = Guid.NewGuid(), Name = $"M{i}", Email = $"m{i}@x.vn", ClubId = club.Id });
        var service = new ClubService(clubs, members, new ListLogger(), new NotificationServiceFactory());

        // Act
        var act = () => service.AddMember(club.Id, "Extra", "extra@x.vn");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    private sealed class ListLogger : IAppLogger
    {
        public List<string> Lines { get; } = new();
        public void Log(string message) => Lines.Add(message);
    }

    private sealed class SpyObserver : IEventObserver
    {
        public Event? Received { get; private set; }
        public void OnEventCreated(Event ev, Club club) => Received = ev;
    }
}
