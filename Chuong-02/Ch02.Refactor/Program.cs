using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;
using EduHub.Ch02.Services;

// ═══════════════════════════════════════════════════════════════
//  EduHub Ch02 — Composition root: nơi DUY NHẤT dùng `new` cho class cụ thể.
// ═══════════════════════════════════════════════════════════════

Console.OutputEncoding = System.Text.Encoding.UTF8;

IAppLogger logger = new ConsoleLogger();
var members = new InMemoryMemberRepository();
IClubRepository clubs = new LoggingClubRepository(new InMemoryClubRepository(), logger);   // Decorator
var factory = new NotificationServiceFactory();                                              // Factory

var clubService = new ClubService(clubs, members, logger, factory);
var eventService = new EventService(clubs);
var reportService = new ReportService(clubs, members);

Console.WriteLine("── 1. Tạo CLB và thêm thành viên ──");
var club = clubService.CreateClub("Tech Club", "Câu lạc bộ công nghệ", "president@gmail.com");
clubService.AddMember(club.Id, "Nguyễn Văn A", "a@student.dthu.edu.vn");
clubService.AddMember(club.Id, "Trần Thị B", "b@student.dthu.edu.vn");

Console.WriteLine("\n── 2. Observer: tạo sự kiện báo cho LogNotifier và EmailNotifier ──");
eventService.Subscribe(new LogNotifier(logger));
eventService.Subscribe(new EmailNotifier(members, factory.Create(NotificationChannel.Email)));
eventService.CreateEvent(club.Id, "Workshop SOLID", new DateTime(2026, 11, 20));

Console.WriteLine("\n── 3. Strategy: cùng dữ liệu, hai định dạng báo cáo ──");
reportService.Generate(new ConsoleReportStrategy(), Console.Out);
reportService.Generate(new CsvReportStrategy(), Console.Out);

Console.WriteLine("\n── 4. Factory: thêm kênh thông báo không cần sửa ClubService ──");
foreach (var channel in Enum.GetValues<NotificationChannel>())
    factory.Create(channel).Send("demo", $"kênh {channel}");

Console.WriteLine("\n── 5. Validate ở Domain ──");
try { clubService.CreateClub("  ", "x", "p@gmail.com"); }
catch (ArgumentException ex) { Console.WriteLine($"Bị chặn đúng: {ex.Message}"); }
