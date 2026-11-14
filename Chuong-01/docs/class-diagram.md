# Sơ đồ lớp sơ bộ — EduHub

Thuộc tính chính, khoá ngoại (FK) và bội số quan hệ. Vẽ bằng Mermaid.

```mermaid
classDiagram
    class Member {
        +Guid Id
        +string Name
        +string Email
        +string StudentCode
        +DateTime JoinedAt
        +Create(name, email, studentCode) Member
        +IsActive() bool
    }
    class Club {
        +Guid Id
        +string Name
        +string Description
        +DateTime CreatedAt
        +IReadOnlyList~Member~ Members
        +Create(name, description) Club
        +AddMember(member) void
        +MemberCount() int
    }
    class Event {
        +Guid Id
        +Guid ClubId  «FK → Club»
        +string Title
        +DateTime Date
        +int Capacity
        +bool IsOnline
        +Create(clubId, title, date, capacity, isOnline) Event
        +HasCapacity(registeredCount) bool
    }
    class Registration {
        +Guid Id
        +Guid MemberId  «FK → Member»
        +Guid EventId  «FK → Event»
        +DateTime RegisteredAt
        +string Status  «Pending|Confirmed|Cancelled»
        +Create(memberId, eventId) Registration
        +Confirm() void
        +Cancel() void
    }
    class CheckIn {
        +Guid Id
        +Guid RegistrationId  «FK → Registration»
        +DateTime CheckedInAt
        +string Method  «Manual|QrCode»
        +Create(registration, method) CheckIn
        +Summary() string
    }

    Club "1" o-- "*" Member : có thành viên
    Club "1" --> "*" Event : tổ chức
    Member "1" --> "*" Registration : đăng ký
    Event "1" --> "*" Registration : nhận đăng ký
    Registration "1" --> "0..1" CheckIn : được ghi nhận
```

## Ghi chú

- `Registration` là bảng nối quan hệ nhiều–nhiều giữa `Member` và `Event`.
- `CheckIn` quan hệ 1–1 với `Registration` (0..1: chưa check-in thì chưa có bản ghi), chỉ tạo được khi phiếu `Confirmed`.
- `Club.Members` chỉ đọc; thêm thành viên bắt buộc qua `AddMember` (chặn null và trùng).
- Mọi entity có constructor private + factory `Create(...)` để kiểm tra dữ liệu ngay khi tạo.
