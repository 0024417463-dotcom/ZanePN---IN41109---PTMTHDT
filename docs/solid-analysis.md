# Phân tích vi phạm SOLID trong `ClubManager` (bản gốc)

Số dòng tham chiếu tới `Ch02.Refactor/ClubManager.cs` ở commit starter (trước khi refactor).

| # | Dòng | Nguyên tắc | Vi phạm | Hậu quả | Cách khắc phục |
|---|------|-----------|---------|---------|----------------|
| 1 | 28–116 | **SRP** | Một class vừa lưu trữ CLB/thành viên, vừa validate, ghi log, gửi thông báo, in báo cáo (5 lý do để thay đổi). | Sửa cách gửi mail hay định dạng báo cáo đều phải mở và test lại cả class; khó đọc, khó test riêng từng phần. | Tách `ClubService`, `EventService`, `ReportService`, repository, logger, notification. |
| 2 | 38–41, 72 | **SRP** (và lỗi đặt sai chỗ) | Luật nghiệp vụ (tên ≤ 100 ký tự, tối đa 50 thành viên) nằm trong lớp quản lý thay vì trong Domain; con số 100 và 50 là "magic number" lặp lại. | Luật bị phân tán, Domain chỉ là túi dữ liệu (anemic model); muốn test luật phải dựng cả `ClubManager`. | Chuyển vào `Club.Create` và `Club.HasRoom`, dùng hằng `MaxNameLength`, `MaxMembers`. |
| 3 | 48, 79, 94, 111 | **DIP** | Gọi thẳng `File.AppendAllText` với đường dẫn cứng `"club-log.txt"`. | Test chạy là ghi file thật; không đổi được nơi ghi log (console, DB) nếu không sửa 4 chỗ. | `IAppLogger` (`FileLogger`, `ConsoleLogger`) inject qua constructor. |
| 4 | 52–63 | **OCP** | Chuỗi `if/else` theo đuôi email (`@gmail.com`, `@yahoo.com`...) để chọn cách gửi. | Thêm SMS/Slack hoặc nhà cung cấp mới phải sửa method đang chạy ổn, dễ gây lỗi cho nhánh cũ. | `INotificationService` + `NotificationServiceFactory` (Factory) chọn theo `NotificationChannel`. |
| 5 | 82–83 | **OCP + DIP** | Gửi email và Slack bằng `Console.WriteLine` trực tiếp trong `AddMemberToClub`. | Thêm kênh phải sửa method; không kiểm tra được "đã gửi chưa" trong test. | Gửi qua `INotificationService` do factory tạo. |
| 6 | 88–99 | **SRP + OCP** | `CreateEvent` tự duyệt thành viên và gửi mail cho từng người. | Muốn thêm hành động khi có sự kiện (SMS, ghi audit...) phải sửa `CreateEvent`. | Observer: `IEventObserver` với `EmailNotifier`, `LogNotifier`; `EventService` chỉ gọi `OnEventCreated`. |
| 7 | 102–113 | **SRP + OCP** | `PrintClubReport` vừa tính số liệu, vừa in console, vừa ghi log. | Muốn xuất CSV phải sửa hoặc nhân bản method. | Strategy: `IReportStrategy` (`ConsoleReportStrategy`, `CsvReportStrategy`) và `ReportService.Generate(strategy)`. |
| 8 | 31–32, 116 | **DIP / đóng gói** | Dữ liệu là `List<>` riêng của class, `GetAllClubs()` trả thẳng list nội bộ. | Bên ngoài có thể `Add/Clear` làm hỏng trạng thái; không thay được nơi lưu trữ (DB, API). | `IClubRepository`, `IMemberRepository`, `IEventRepository`; trả `IReadOnlyList`. |
| 9 | 88–99 | **Lỗi chức năng** (hệ quả của SRP) | `CreateEvent` tạo `Event` nhưng không lưu ở đâu cả, nên đăng ký và check-in về sau không tìm lại được. | Sự kiện "biến mất" ngay sau khi tạo. | `IEventRepository` lưu sự kiện; `EventService.Register/CheckIn` tìm theo id. |
| 10 | 28 | **ISP / DIP** | Không có interface nào: người dùng class phải phụ thuộc vào cả 4 nhóm chức năng dù chỉ cần một. | Không thể mock hay thay thế khi test. | Interface nhỏ theo vai trò: `IClubService`, `IEventService`, `IReportService`... |

## Kết quả sau refactor

```
Program (composition root)
 ├─ ClubService      → IClubRepository (LoggingClubRepository ← Decorator), IMemberRepository,
 │                     INotificationServiceFactory (Factory), IAppLogger
 ├─ EventService     → IClubRepository, IEventRepository, IAppLogger, IEnumerable<IEventObserver> (Observer)
 └─ ReportService    → IClubRepository, IMemberRepository, IAppLogger, IReportStrategy (Strategy)
```

| Pattern | Vị trí | Thay thế cho |
|---------|--------|--------------|
| Factory | `Notifications/NotificationServiceFactory.cs` | chuỗi `if/else` gửi thông báo |
| Decorator | `Repositories/LoggingClubRepository.cs` | log rải rác trong `ClubManager` |
| Strategy | `Patterns/Strategies/*` | `PrintClubReport` |
| Observer | `Patterns/Observers/*` | vòng lặp gửi mail trong `CreateEvent` |
