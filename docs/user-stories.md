# User Stories — EduHub

Hệ thống EduHub quản lý câu lạc bộ sinh viên và sự kiện của câu lạc bộ. Có 3 vai trò: **Sinh viên**, **Ban CLB** và **Admin**.

## US-01 · Sinh viên: Đăng ký tham gia sự kiện

Là sinh viên, tôi muốn đăng ký tham gia một sự kiện của CLB, để giữ chỗ và được ban tổ chức ghi nhận.

- Tiêu chí 1: Given sự kiện còn chỗ (số đăng ký < sức chứa), When tôi bấm "Đăng ký", Then hệ thống tạo phiếu đăng ký với trạng thái `Pending`.
- Tiêu chí 2: Given sự kiện đã đủ sức chứa, When tôi bấm "Đăng ký", Then hệ thống từ chối và hiển thị thông báo "Sự kiện đã đầy".
- Tiêu chí 3: Given tôi đã có phiếu đăng ký còn hiệu lực cho sự kiện này, When tôi đăng ký lại, Then hệ thống từ chối đăng ký trùng.

## US-02 · Sinh viên: Huỷ đăng ký

Là sinh viên, tôi muốn huỷ đăng ký khi không thể tham dự, để nhường chỗ cho bạn khác.

- Tiêu chí 1: Given phiếu của tôi đang `Pending` hoặc `Confirmed`, When tôi huỷ, Then trạng thái chuyển sang `Cancelled` và số chỗ trống tăng lên 1.
- Tiêu chí 2: Given phiếu đã `Cancelled`, When tôi huỷ lần nữa, Then hệ thống báo lỗi vì phiếu đã bị huỷ.

## US-03 · Ban CLB: Tạo sự kiện

Là thành viên Ban CLB, tôi muốn tạo sự kiện với tiêu đề, ngày, sức chứa và hình thức online/offline, để sinh viên có thể đăng ký tham gia.

- Tiêu chí 1: Given tôi nhập đủ tiêu đề, ngày và sức chứa > 0, When tôi lưu, Then sự kiện được tạo và gắn với CLB của tôi.
- Tiêu chí 2: Given tiêu đề để trống hoặc sức chứa ≤ 0, When tôi lưu, Then hệ thống từ chối và chỉ ra trường nhập sai.

## US-04 · Ban CLB: Xác nhận đăng ký và check-in

Là thành viên Ban CLB, tôi muốn xác nhận phiếu đăng ký và check-in người tham dự tại sự kiện, để thống kê số người thực sự có mặt.

- Tiêu chí 1: Given phiếu đang `Pending`, When tôi xác nhận, Then trạng thái chuyển sang `Confirmed`.
- Tiêu chí 2: Given phiếu `Confirmed`, When tôi check-in (thủ công hoặc quét QR), Then hệ thống ghi nhận thời điểm và phương thức check-in.
- Tiêu chí 3: Given phiếu chưa `Confirmed` hoặc đã `Cancelled`, When tôi check-in, Then hệ thống từ chối.

## US-05 · Ban CLB: Xem báo cáo tham dự

Là thành viên Ban CLB, tôi muốn xem báo cáo tham dự của sự kiện, để đánh giá hiệu quả và lập kế hoạch cho sự kiện sau.

- Tiêu chí 1: Given sự kiện đã diễn ra, When tôi mở báo cáo, Then thấy số đăng ký, số check-in và danh sách tên người tham dự.
- Tiêu chí 2: Given có người đăng ký nhưng không check-in, When tôi xem báo cáo, Then họ được đánh dấu "vắng".

## US-06 · Admin: Quản lý câu lạc bộ

Là Admin, tôi muốn tạo câu lạc bộ mới và thêm thành viên vào CLB, để quản lý danh sách CLB của trường tập trung.

- Tiêu chí 1: Given tên CLB không trống, When tôi tạo CLB, Then CLB được lưu với mô tả và thời điểm tạo.
- Tiêu chí 2: Given sinh viên đã là thành viên của CLB, When tôi thêm lại sinh viên đó, Then hệ thống từ chối thêm trùng.
- Tiêu chí 3: Given email sinh viên không chứa '@', When tôi thêm thành viên, Then hệ thống báo lỗi dữ liệu không hợp lệ.

## US-07 · Admin: Giám sát số liệu toàn hệ thống

Là Admin, tôi muốn xem tổng số thành viên và số sự kiện của từng CLB, để nắm mức độ hoạt động của các CLB.

- Tiêu chí 1: Given hệ thống có nhiều CLB, When tôi mở trang thống kê, Then thấy số thành viên và số sự kiện của từng CLB.
- Tiêu chí 2: Given một CLB chưa có sự kiện, When tôi xem thống kê, Then số sự kiện hiển thị là 0 (không báo lỗi).
