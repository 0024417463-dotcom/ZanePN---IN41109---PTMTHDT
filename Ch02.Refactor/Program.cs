using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;
using EduHub.Ch02.Services;

// ═══════════════════════════════════════════════════════════════
//  EduHub Ch02: composition root. Mọi phụ thuộc được lắp ráp ở đây,
//  các service chỉ biết interface (DIP).
// ═══════════════════════════════════════════════════════════════

IAppLogger logger = new FileLogger("club-log.txt");
INotificationServiceFactory notifications = new NotificationServiceFactory();

// Decorator: repository thật được bọc bởi lớp ghi log
IClubRepository clubRepo = new LoggingClubRepository(new InMemoryClubRepository(), logger);
IMemberRepository memberRepo = new InMemoryMemberRepository();
IEventRepository eventRepo = new InMemoryEventRepository();

// Observer: đăng ký những bên cần biết khi có sự kiện mới
var observers = new IEventObserver[]
{
    new EmailNotifier(memberRepo, notifications.Create(NotificationChannel.Email)),
    new LogNotifier(logger)
};

IClubService clubService = new ClubService(clubRepo, memberRepo, notifications, logger);
IEventService eventService = new EventService(clubRepo, eventRepo, logger, observers);
IReportService reportService = new ReportService(clubRepo, memberRepo, logger);

var club = clubService.CreateClub("Tech Club", "Câu lạc bộ công nghệ", "president@gmail.com");
var member = clubService.AddMember(club.Id, "Nguyễn Văn A", "a@student.dthu.edu.vn");
var ev = eventService.CreateEvent(club.Id, "Workshop SOLID", new DateTime(2026, 11, 20));
eventService.Register(ev.Id, member.Id);
eventService.CheckIn(ev.Id, member.Id);

// Strategy: cùng dữ liệu, hai cách xuất
Console.WriteLine("\n--- Console report ---");
reportService.Generate(new ConsoleReportStrategy());
Console.WriteLine("\n--- CSV report ---");
reportService.Generate(new CsvReportStrategy());
