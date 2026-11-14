# Phân tích vi phạm SOLID — `ClubManager` (bản starter)

Số dòng theo file `Ch02.Refactor/ClubManager.cs` của starter (trước khi refactor).

| # | Vị trí | Nguyên tắc | Vấn đề | Hậu quả | Cách sửa |
|---|---|---|---|---|---|
| 1 | `CreateClub` (dòng 35–64) | **SRP** | Một method vừa validate, lưu, ghi log, gửi thông báo | Đổi một việc (ví dụ cách log) phải sửa method nghiệp vụ; khó test từng phần | Validate về `Club.Create`; lưu qua `IClubRepository`; log qua `IAppLogger`; gửi qua `INotificationService`; điều phối ở `ClubService` |
| 2 | `File.AppendAllText(_logFilePath, …)` ở cả 4 method | **DIP** | Logic nghiệp vụ phụ thuộc trực tiếp file hệ thống | Test unit để lại file rác, không thay được bằng console/DB; không kiểm tra được nội dung log | Phụ thuộc `IAppLogger`, nhận qua constructor; `FileLogger`/`ConsoleLogger` chọn ở composition root |
| 3 | Chuỗi `if/else` Gmail/Yahoo/generic (dòng 52–61) | **OCP** | Thêm nhà cung cấp hoặc kênh (SMS) phải sửa lại method đã chạy ổn | Dễ phá hành vi cũ, method phình to, trùng lặp | `INotificationService` + `NotificationServiceFactory` (Factory) |
| 4 | `AddMemberToClub` (dòng 67–84) | **SRP + OCP** | Vừa kiểm tra sĩ số, lưu thành viên, vừa hardcode gửi email và Slack | Muốn tắt Slack phải sửa nghiệp vụ; luật "tối đa 50" là số ma thuật nằm trong service | Luật sĩ số về `Club.HasRoom` / `Club.MaxMembers`; thông báo qua factory |
| 5 | `CreateEvent` (dòng 88–99) | **SRP + OCP** | Tạo sự kiện nhưng tự duyệt thành viên để in email | Thêm người nhận mới (ví dụ push) phải sửa `CreateEvent` | Observer: `EventService` chỉ gọi `IEventObserver.OnEventCreated`; `EmailNotifier`, `LogNotifier` đăng ký ở ngoài |
| 6 | `PrintClubReport` (dòng 103–113) | **SRP + OCP + DIP** | Báo cáo trộn với nghiệp vụ, in thẳng ra `Console`, ghi file ngay trong lúc in; thêm CSV phải sửa method | Không xuất được định dạng khác, không test được đầu ra | `ReportService.Generate(IReportStrategy, TextWriter)` với `ConsoleReportStrategy`, `CsvReportStrategy` (Strategy) |
| 7 | `_clubs`, `_members` (dòng 30–31); `GetAllClubs()` (dòng 116) trả về chính list nội bộ | **DIP + đóng gói** | Lưu trữ cố định trong class, trả list gốc ra ngoài cho phép sửa trực tiếp | Không đổi được nơi lưu (DB), bên ngoài làm hỏng trạng thái | `IClubRepository`, `IMemberRepository`; trả `IReadOnlyList` (ISP: interface nhỏ theo nhu cầu) |
| 8 | Toàn bộ class | **SRP (tổng thể)** | God class gánh CLB, thành viên, sự kiện, thông báo, log, báo cáo | Mọi thay đổi đều chạm vào một file, khó làm việc nhóm, khó test | Tách thành `ClubService`, `EventService`, `ReportService` và các interface |

## Hai vi phạm khác không có chú thích trong starter

- **Validate bằng chuỗi lỗi lặp lại** (`"Club name too long"`, số `100` rải trong service): luật nghiệp vụ nằm sai tầng, đã chuyển vào `Club.Create` kèm hằng `MaxNameLength`.
- **Khoá cứng định dạng thông báo theo đuôi email** (`Contains("@gmail.com")`): dùng chuỗi con của địa chỉ để suy ra hành vi, dễ sai (ví dụ `x@gmail.com.vn`). Kênh gửi nay được chọn tường minh bằng `NotificationChannel`.

## Pattern đã áp dụng

| Pattern | Class | Giải quyết |
|---|---|---|
| Factory | `INotificationServiceFactory`, `NotificationServiceFactory` | Vi phạm 3, 4 |
| Decorator | `LoggingClubRepository` bọc `IClubRepository` | Vi phạm 2 (log mỗi lần truy cập mà không sửa repository gốc) |
| Strategy | `IReportStrategy`, `ConsoleReportStrategy`, `CsvReportStrategy` | Vi phạm 6 |
| Observer | `IEventObserver`, `EmailNotifier`, `LogNotifier` | Vi phạm 5 |
