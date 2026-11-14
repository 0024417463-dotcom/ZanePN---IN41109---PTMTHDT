# Sơ đồ lớp sơ bộ — EduHub

Gồm 5 class: `Member`, `Club`, `Event`, `Registration`, `CheckIn`. Khoá ngoại (FK) ghi trong thuộc tính, bội số ghi trên đường nối.

```mermaid
classDiagram
    class Member {
        +Guid Id
        +string Name
        +string Email
        +string StudentCode
        +DateTime JoinedAt
        +Create(name, email, studentCode)$ Member
        +IsActive() bool
        +ToString() string
    }
    class Club {
        +Guid Id
        +string Name
        +string Description
        +DateTime CreatedAt
        +IReadOnlyList~Member~ Members
        +Create(name, description)$ Club
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
        +Create(clubId, title, date, capacity, isOnline)$ Event
        +HasCapacity(registeredCount) bool
    }
    class Registration {
        +Guid Id
        +Guid MemberId  «FK → Member»
        +Guid EventId  «FK → Event»
        +DateTime RegisteredAt
        +string Status  «Pending / Confirmed / Cancelled»
        +Create(memberId, eventId)$ Registration
        +Confirm() void
        +Cancel() void
    }
    class CheckIn {
        +Guid Id
        +Guid RegistrationId  «FK → Registration, unique»
        +DateTime CheckedInAt
        +string Method  «Manual / QrCode»
        +Create(registration, method)$ CheckIn
        +Summary() string
    }

    Club "1" --> "*" Event : tổ chức
    Club "*" --> "*" Member : có thành viên
    Member "1" --> "*" Registration : đăng ký
    Event "1" --> "*" Registration : nhận đăng ký
    Registration "1" --> "0..1" CheckIn : được check-in
```

## Giải thích bội số

| Quan hệ | Bội số | Ý nghĩa |
|---|---|---|
| Club – Event | 1 – * | Một CLB tổ chức nhiều sự kiện; mỗi sự kiện thuộc đúng một CLB (`Event.ClubId`). |
| Club – Member | * – * | Một CLB có nhiều thành viên; một sinh viên có thể tham gia nhiều CLB. |
| Member – Event | * – * | Quan hệ nhiều–nhiều, được tách thành bảng nối `Registration`. |
| Member – Registration | 1 – * | Một thành viên có thể có nhiều phiếu đăng ký. |
| Event – Registration | 1 – * | Một sự kiện có nhiều phiếu đăng ký (số lượng bị giới hạn bởi `Capacity`). |
| Registration – CheckIn | 1 – 0..1 | Một phiếu có tối đa một lần check-in; chưa đến sự kiện thì chưa có. |
