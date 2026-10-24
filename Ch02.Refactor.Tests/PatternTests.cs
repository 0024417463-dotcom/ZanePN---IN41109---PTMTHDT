using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;
using EduHub.Ch02.Services;

namespace Ch02.Refactor.Tests;

// Fake thủ công: nhờ DIP, service test được mà không cần file hay console thật.
internal sealed class FakeLogger : IAppLogger
{
    public List<string> Lines { get; } = new();
    public void Info(string message) => Lines.Add(message);
}

internal sealed class FakeNotification : INotificationService
{
    public List<(string To, string Message)> Sent { get; } = new();
    public void Send(string recipient, string message) => Sent.Add((recipient, message));
}

internal sealed class FakeFactory : INotificationServiceFactory
{
    public Dictionary<NotificationChannel, FakeNotification> Created { get; } = new();
    public INotificationService Create(NotificationChannel channel)
    {
        if (!Created.TryGetValue(channel, out var svc))
            Created[channel] = svc = new FakeNotification();
        return svc;
    }
}

internal sealed class FakeObserver : IEventObserver
{
    public List<Event> Received { get; } = new();
    public void OnEventCreated(Event ev, Club club) => Received.Add(ev);
}

public class ServiceTests
{
    private static (ClubService Service, FakeFactory Notifications, FakeLogger Logger, InMemoryMemberRepository Members)
        CreateClubService()
    {
        var notifications = new FakeFactory();
        var logger = new FakeLogger();
        var members = new InMemoryMemberRepository();
        var service = new ClubService(new InMemoryClubRepository(), members, notifications, logger);
        return (service, notifications, logger, members);
    }

    [Fact]
    public void CreateClub_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var (service, _, _, _) = CreateClubService();

        // Act
        var act = () => service.CreateClub("", "mô tả", "p@gmail.com");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void CreateClub_ValidInput_SavesLogsAndNotifiesPresident()
    {
        // Arrange
        var (service, notifications, logger, _) = CreateClubService();

        // Act
        var club = service.CreateClub("Tech Club", "mô tả", "p@gmail.com");

        // Assert
        Assert.Single(service.GetAllClubs());
        Assert.Contains(logger.Lines, l => l.Contains("Tech Club"));
        Assert.Equal("p@gmail.com", notifications.Created[NotificationChannel.Email].Sent.Single().To);
        Assert.Equal(club.Id, service.GetAllClubs()[0].Id);
    }

    [Fact]
    public void AddMember_UnknownClub_ThrowsInvalidOperationException()
    {
        // Arrange
        var (service, _, _, _) = CreateClubService();

        // Act
        var act = () => service.AddMember(Guid.NewGuid(), "A", "a@x.com");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void AddMember_WhenClubIsFull_ThrowsInvalidOperationException()
    {
        // Arrange
        var (service, _, _, _) = CreateClubService();
        var club = service.CreateClub("Tech Club", "mô tả", "p@gmail.com");
        for (var i = 0; i < Club.MaxMembers; i++)
            service.AddMember(club.Id, $"M{i}", $"m{i}@x.com");

        // Act
        var act = () => service.AddMember(club.Id, "Extra", "extra@x.com");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void AddMember_ValidInput_SendsEmailAndSlackNotifications()
    {
        // Arrange
        var (service, notifications, _, _) = CreateClubService();
        var club = service.CreateClub("Tech Club", "mô tả", "p@gmail.com");

        // Act
        service.AddMember(club.Id, "A", "a@x.com");

        // Assert
        Assert.Contains(notifications.Created[NotificationChannel.Email].Sent, s => s.To == "a@x.com");
        Assert.Single(notifications.Created[NotificationChannel.Slack].Sent);
    }
}

public class PatternTests
{
    // Factory
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
    public void Factory_Create_UnknownChannel_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var factory = new NotificationServiceFactory();

        // Act
        var act = () => factory.Create((NotificationChannel)999);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    // Decorator
    [Fact]
    public void LoggingClubRepository_Add_DelegatesToInnerAndLogs()
    {
        // Arrange
        var inner = new InMemoryClubRepository();
        var logger = new FakeLogger();
        var repo = new LoggingClubRepository(inner, logger);
        var club = Club.Create("Tech Club", "mô tả");

        // Act
        repo.Add(club);

        // Assert
        Assert.Same(club, inner.GetById(club.Id));
        Assert.Single(logger.Lines);
    }

    [Fact]
    public void LoggingClubRepository_GetById_ReturnsInnerResultAndLogs()
    {
        // Arrange
        var inner = new InMemoryClubRepository();
        var club = Club.Create("Tech Club", "mô tả");
        inner.Add(club);
        var logger = new FakeLogger();
        var repo = new LoggingClubRepository(inner, logger);

        // Act
        var found = repo.GetById(club.Id);

        // Assert
        Assert.Same(club, found);
        Assert.Single(logger.Lines);
    }

    // Observer
    [Fact]
    public void EventService_CreateEvent_NotifiesAllObservers()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var club = Club.Create("Tech Club", "mô tả");
        clubs.Add(club);
        var o1 = new FakeObserver();
        var o2 = new FakeObserver();
        var service = new EventService(clubs, new InMemoryEventRepository(), new FakeLogger(), new[] { o1, o2 });

        // Act
        var ev = service.CreateEvent(club.Id, "Workshop", new DateTime(2026, 11, 20));

        // Assert
        Assert.Same(ev, Assert.Single(o1.Received));
        Assert.Same(ev, Assert.Single(o2.Received));
    }

    [Fact]
    public void EventService_CreateEvent_UnknownClub_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new EventService(new InMemoryClubRepository(), new InMemoryEventRepository(),
            new FakeLogger(), Array.Empty<IEventObserver>());

        // Act
        var act = () => service.CreateEvent(Guid.NewGuid(), "Workshop", DateTime.Now);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void EmailNotifier_OnEventCreated_EmailsEveryClubMember()
    {
        // Arrange
        var members = new InMemoryMemberRepository();
        var club = Club.Create("Tech Club", "mô tả");
        members.Add(new Member { Id = Guid.NewGuid(), Name = "A", Email = "a@x.com", ClubId = club.Id });
        members.Add(new Member { Id = Guid.NewGuid(), Name = "B", Email = "b@x.com", ClubId = club.Id });
        members.Add(new Member { Id = Guid.NewGuid(), Name = "C", Email = "c@x.com", ClubId = Guid.NewGuid() });
        var email = new FakeNotification();
        var notifier = new EmailNotifier(members, email);
        var ev = new Event { Title = "Workshop", ClubId = club.Id };

        // Act
        notifier.OnEventCreated(ev, club);

        // Assert
        Assert.Equal(new[] { "a@x.com", "b@x.com" }, email.Sent.Select(s => s.To));
    }

    [Fact]
    public void LogNotifier_OnEventCreated_WritesLogLine()
    {
        // Arrange
        var logger = new FakeLogger();
        var notifier = new LogNotifier(logger);
        var club = Club.Create("Tech Club", "mô tả");

        // Act
        notifier.OnEventCreated(new Event { Title = "Workshop", ClubId = club.Id }, club);

        // Assert
        Assert.Contains("Workshop", Assert.Single(logger.Lines));
    }

    // Strategy
    private static ReportService CreateReport(out Club club)
    {
        var clubs = new InMemoryClubRepository();
        var members = new InMemoryMemberRepository();
        club = Club.Create("Tech, \"Club\"", "mô tả");
        clubs.Add(club);
        members.Add(new Member { Id = Guid.NewGuid(), Name = "A", Email = "a@x.com", ClubId = club.Id });
        return new ReportService(clubs, members, new FakeLogger());
    }

    [Fact]
    public void ReportService_Generate_WithConsoleStrategy_PrintsClubLine()
    {
        // Arrange
        var report = CreateReport(out _);
        var writer = new StringWriter();

        // Act
        report.Generate(new ConsoleReportStrategy(writer));

        // Assert
        Assert.Contains("Members: 1", writer.ToString());
    }

    [Fact]
    public void ReportService_Generate_WithCsvStrategy_WritesHeaderAndEscapedRow()
    {
        // Arrange
        var report = CreateReport(out _);
        var writer = new StringWriter();

        // Act
        report.Generate(new CsvReportStrategy(writer));

        // Assert
        var lines = writer.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("ClubName,MemberCount", lines[0]);
        Assert.Equal("\"Tech, \"\"Club\"\"\",1", lines[1]);
    }
}
