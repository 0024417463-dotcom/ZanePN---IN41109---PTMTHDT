using EduHub.Ch01.Domain;

// ═══════════════════════════════════════════════════════════════
//  EduHub Ch01 — Console demo (luồng nghiệp vụ đầy đủ)
// ═══════════════════════════════════════════════════════════════

const string Separator = "──────────────────────────────────────────────";
const string Title = "=== EduHub — Club & Event Management ===";

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine(Title);

// 1. Tạo CLB
var club = Club.Create("Tech Club", "Câu lạc bộ công nghệ của khoa Công nghệ và Kỹ thuật");
Console.WriteLine($"\n{Separator}\n[CLB] {club.Name} — {club.Description}\n     Id: {club.Id} | Tạo lúc: {club.CreatedAt:dd/MM/yyyy HH:mm} UTC");

// 2. Tạo 3 thành viên và thêm vào CLB
var members = new[]
{
    Member.Create("Nguyễn Văn A", "a@student.dthu.edu.vn", "0024000001"),
    Member.Create("Trần Thị B",   "b@student.dthu.edu.vn", "0024000002"),
    Member.Create("Lê Văn C",     "c@student.dthu.edu.vn", "0024000003"),
};
foreach (var m in members) club.AddMember(m);

Console.WriteLine($"\n{Separator}\n[Thành viên] {club.MemberCount()} thành viên:");
foreach (var m in club.Members)
    Console.WriteLine($" - {m} | {m.Email} | hoạt động: {(m.IsActive() ? "có" : "không")}");

// 3. Tạo sự kiện
var ev = Event.Create(club.Id, "Workshop AI", new DateTime(2026, 11, 20, 9, 0, 0), capacity: 50);

// 4. Đăng ký (kiểm tra sức chứa) → Pending → Confirm
var registrations = new List<Registration>();
foreach (var m in members.Take(2))
{
    if (!ev.HasCapacity(registrations.Count))
    {
        Console.WriteLine($"Sự kiện đã đủ chỗ, không đăng ký được cho {m}.");
        continue;
    }
    var reg = Registration.Create(m.Id, ev.Id);
    reg.Confirm();
    registrations.Add(reg);
}

Console.WriteLine($"\n{Separator}\n[Sự kiện] {ev.Title} — {ev.Date:dd/MM/yyyy HH:mm} ({(ev.IsOnline ? "online" : "offline")})");
Console.WriteLine($"          Đăng ký: {registrations.Count}/{ev.Capacity} | còn chỗ: {(ev.HasCapacity(registrations.Count) ? "có" : "không")}");

// 5. Một thành viên check-in
var checkIns = new List<CheckIn> { CheckIn.Create(registrations[0], CheckIn.QrCode) };
Console.WriteLine($"\n{Separator}\n[Check-in]");
foreach (var c in checkIns) Console.WriteLine($" - {c.Summary()}");

// Minh hoạ ràng buộc nghiệp vụ: phiếu Pending / đã huỷ không check-in được
var pending = Registration.Create(members[2].Id, ev.Id);
try { CheckIn.Create(pending, CheckIn.Manual); }
catch (InvalidOperationException ex) { Console.WriteLine($" ! Bị chặn đúng: {ex.Message}"); }

// 6. Báo cáo tham dự
Console.WriteLine($"\n{Separator}\n[Báo cáo tham dự] {ev.Title}");
Console.WriteLine($" Số đăng ký: {registrations.Count} | Số check-in: {checkIns.Count}");
var byId = club.Members.ToDictionary(m => m.Id);
foreach (var r in registrations)
{
    var attended = checkIns.Any(c => c.RegistrationId == r.Id);
    Console.WriteLine($" - {byId[r.MemberId].Name,-15} {r.Status,-10} {(attended ? "✔ có mặt" : "✘ vắng")}");
}

Console.WriteLine("\n✅ Demo complete!");
