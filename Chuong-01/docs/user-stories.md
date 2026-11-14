# User Stories — EduHub

Hệ thống quản lý câu lạc bộ và sự kiện sinh viên. Ba vai trò: **Sinh viên**, **Ban CLB**, **Admin**.

## US-01 · Sinh viên: đăng ký tham dự sự kiện

Là sinh viên, tôi muốn đăng ký tham dự một sự kiện của CLB, để giữ chỗ và được ban tổ chức xác nhận.

- Tiêu chí 1: Given sự kiện còn chỗ (số đăng ký < sức chứa) When tôi đăng ký Then hệ thống tạo phiếu đăng ký ở trạng thái Pending.
- Tiêu chí 2: Given sự kiện đã đủ chỗ When tôi đăng ký Then hệ thống từ chối và báo "hết chỗ".
- Tiêu chí 3: Given tôi đã có phiếu đăng ký còn hiệu lực cho sự kiện này When tôi đăng ký lần nữa Then hệ thống không tạo phiếu thứ hai.

## US-02 · Sinh viên: huỷ đăng ký

Là sinh viên, tôi muốn huỷ đăng ký khi không thể tham dự, để nhường chỗ cho người khác.

- Tiêu chí 1: Given phiếu của tôi ở trạng thái Pending hoặc Confirmed When tôi huỷ Then trạng thái chuyển thành Cancelled và chỗ được giải phóng.
- Tiêu chí 2: Given phiếu đã Cancelled When tôi huỷ lần nữa Then hệ thống báo lỗi, trạng thái không đổi.

## US-03 · Ban CLB: tạo sự kiện

Là thành viên Ban CLB, tôi muốn tạo sự kiện với tiêu đề, thời gian, sức chứa và hình thức online/offline, để sinh viên có thể đăng ký.

- Tiêu chí 1: Given tôi nhập tiêu đề không rỗng và sức chứa > 0 When tôi tạo sự kiện Then sự kiện được gắn với CLB của tôi và hiện trong danh sách.
- Tiêu chí 2: Given tiêu đề rỗng hoặc sức chứa ≤ 0 When tôi tạo sự kiện Then hệ thống từ chối và nêu rõ trường sai.

## US-04 · Ban CLB: xác nhận đăng ký

Là Ban CLB, tôi muốn xác nhận các phiếu đăng ký Pending, để chốt danh sách tham dự.

- Tiêu chí 1: Given phiếu ở trạng thái Pending When tôi xác nhận Then phiếu chuyển sang Confirmed.
- Tiêu chí 2: Given phiếu ở trạng thái Cancelled hoặc đã Confirmed When tôi xác nhận Then hệ thống báo lỗi.

## US-05 · Ban CLB: check-in người tham dự

Là Ban CLB, tôi muốn check-in sinh viên tại sự kiện (thủ công hoặc quét QR), để ghi nhận người có mặt thực tế.

- Tiêu chí 1: Given phiếu đã Confirmed When tôi check-in bằng "Manual" hoặc "QrCode" Then hệ thống lưu thời điểm và phương thức check-in.
- Tiêu chí 2: Given phiếu còn Pending hoặc đã Cancelled When tôi check-in Then hệ thống từ chối.

## US-06 · Admin: quản lý thành viên của CLB

Là Admin, tôi muốn thêm thành viên vào CLB và xem danh sách, để dữ liệu thành viên luôn chính xác.

- Tiêu chí 1: Given thành viên có tên, mã sinh viên và email chứa "@" When tôi thêm vào CLB Then thành viên xuất hiện trong danh sách CLB.
- Tiêu chí 2: Given thành viên đã có trong CLB (trùng mã sinh viên) When tôi thêm lần nữa Then hệ thống từ chối.
- Tiêu chí 3: Given email không hợp lệ When tôi tạo thành viên Then hệ thống báo lỗi.

## US-07 · Admin: xem báo cáo tham dự

Là Admin, tôi muốn xem báo cáo tham dự của một sự kiện (số đăng ký, số check-in, danh sách tên), để đánh giá hiệu quả hoạt động CLB.

- Tiêu chí 1: Given sự kiện có đăng ký và check-in When tôi mở báo cáo Then thấy số đăng ký, số check-in và tên từng người kèm trạng thái có mặt/vắng.
- Tiêu chí 2: Given sự kiện chưa có đăng ký When tôi mở báo cáo Then báo cáo hiển thị 0 đăng ký, 0 check-in, không lỗi.
