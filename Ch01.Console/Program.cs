using EduHub.Ch01.Domain;

// ═══════════════════════════════════════════════════════════════
//  EduHub Ch01 — Console demo (luồng nghiệp vụ đầy đủ)
// ═══════════════════════════════════════════════════════════════

const string Separator = "────────────────────────────────────────────";

Console.WriteLine("=== EduHub — Club & Event Management ===");

// 1. Tạo CLB
Console.WriteLine($"\n{Separator}\n[1] Tạo câu lạc bộ\n{Separator}");
var club = Club.Create("Tech Club", "Câu lạc bộ công nghệ: workshop, hackathon và chia sẻ kiến thức lập trình.");
Console.WriteLine($"CLB: {club.Name} | Id: {club.Id}");
Console.WriteLine($"Mô tả: {club.Description}");

// 2. Tạo 3 thành viên và thêm vào CLB
Console.WriteLine($"\n{Separator}\n[2] Thêm thành viên\n{Separator}");
var alice = Member.Create("Nguyễn Văn An", "an.nv@student.edu.vn", "SV001");
var binh  = Member.Create("Trần Thị Bình", "binh.tt@student.edu.vn", "SV002");
var chi   = Member.Create("Lê Minh Chi", "chi.lm@student.edu.vn", "SV003");
foreach (var m in new[] { alice, binh, chi })
{
    club.AddMember(m);
    Console.WriteLine($"+ {m} | {m.Email} | active: {m.IsActive()}");
}
Console.WriteLine($"Tổng số thành viên: {club.MemberCount()}");

// 3. Tạo sự kiện
Console.WriteLine($"\n{Separator}\n[3] Tạo sự kiện\n{Separator}");
var ev = Event.Create(club.Id, "Workshop AI", DateTime.Today.AddDays(14), capacity: 50);
Console.WriteLine($"Sự kiện: {ev.Title} | Ngày: {ev.Date:dd/MM/yyyy} | Online: {ev.IsOnline}");
Console.WriteLine($"Số đăng ký / sức chứa: 0/{ev.Capacity}");

// 4. Đăng ký (kiểm tra HasCapacity) → Pending → Confirm
Console.WriteLine($"\n{Separator}\n[4] Đăng ký và xác nhận\n{Separator}");
var registrations = new List<Registration>();
var memberLookup = club.Members.ToDictionary(m => m.Id);

foreach (var m in new[] { alice, binh })
{
    if (!ev.HasCapacity(registrations.Count))
    {
        Console.WriteLine($"! Sự kiện đã đầy, không nhận thêm {m}");
        continue;
    }
    var reg = Registration.Create(m.Id, ev.Id);
    registrations.Add(reg);
    Console.WriteLine($"{m} đăng ký → {reg.Status}");
    reg.Confirm();
    Console.WriteLine($"{m} được xác nhận → {reg.Status}");
}
Console.WriteLine($"Số đăng ký / sức chứa: {registrations.Count}/{ev.Capacity}");

// 5. Một thành viên check-in
Console.WriteLine($"\n{Separator}\n[5] Check-in\n{Separator}");
var checkIns = new List<CheckIn>();
var checkIn = CheckIn.Create(registrations[0], CheckInMethod.QrCode);
checkIns.Add(checkIn);
Console.WriteLine(checkIn.Summary());

// 6. Báo cáo tham dự
Console.WriteLine($"\n{Separator}\n[6] Báo cáo tham dự — {ev.Title}\n{Separator}");
Console.WriteLine($"Số đăng ký: {registrations.Count}");
Console.WriteLine($"Số check-in: {checkIns.Count}");
var checkedInRegIds = checkIns.Select(c => c.RegistrationId).ToHashSet();
foreach (var r in registrations)
{
    var attended = checkedInRegIds.Contains(r.Id) ? "có mặt" : "vắng";
    Console.WriteLine($"- {memberLookup[r.MemberId].Name,-18} {r.Status,-10} {attended}");
}

// 7. Kiểm tra quy tắc nghiệp vụ
Console.WriteLine($"\n{Separator}\n[7] Kiểm tra quy tắc nghiệp vụ\n{Separator}");
var cancelled = Registration.Create(chi.Id, ev.Id);
cancelled.Cancel();
TryRule("Confirm phiếu đã Cancelled", () => cancelled.Confirm());
TryRule("Check-in phiếu chưa Confirmed", () => CheckIn.Create(cancelled, CheckInMethod.Manual));
TryRule("Thêm thành viên trùng vào CLB", () => club.AddMember(alice));
TryRule("Tạo Member với email sai", () => Member.Create("X", "khong-co-a-cong", "SV999"));
TryRule("Tạo Event với capacity = 0", () => Event.Create(club.Id, "Sai", DateTime.Today, 0));

Console.WriteLine("\n✅ Demo complete!");

static void TryRule(string name, Action action)
{
    try
    {
        action();
        Console.WriteLine($"✗ {name}: KHÔNG bị chặn (lỗi!)");
    }
    catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
    {
        Console.WriteLine($"✓ {name} → {ex.GetType().Name}: {ex.Message}");
    }
}
