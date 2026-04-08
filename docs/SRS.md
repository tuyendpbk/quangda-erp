# SRS - Hệ thống quản lý nhà in (Print ERP)

## 1. Thông tin tài liệu
- **Tên**: Software Requirements Specification - Print ERP
- **Mục tiêu**: Đồng bộ nghiệp vụ giữa BA/Dev/QA/Stakeholders, làm đầu vào thiết kế DB/API/UI và kế hoạch triển khai.
- **Phạm vi**: Quản lý khách hàng, đơn hàng, kho, thanh toán, báo cáo, forecast doanh thu, dynamic menu và phân quyền.

## 2. Công nghệ bắt buộc
- ASP.NET Core MVC
- PostgreSQL
- EF Core hoặc Dapper
- Chart.js cho biểu đồ
- Thư viện xuất PDF (QuestPDF/Rotativa/DinkToPdf)

## 3. Actor
- Admin
- Sales
- Warehouse
- Production
- Accountant
- Manager

## 4. Module nghiệp vụ
1. Authentication/Authorization
2. Dynamic Menu
3. Dashboard
4. Employees
5. Customers
6. Orders
7. Estimate pricing
8. Materials
9. Stock In
10. Stock Out
11. Payments
12. Reports
13. PDF export
14. Revenue forecast

## 5. Functional Requirements (rút gọn theo ID)

### 5.1 Auth & Permission
- **FR-AUTH-01..05**: Đăng nhập/đăng xuất, active account, RBAC, chặn truy cập trái phép.

### 5.2 Dynamic Menu
- **FR-MENU-01..07**: Menu load từ DB, đa cấp, lọc theo quyền, sort, map route, cache, admin quản trị menu.

### 5.3 Dashboard
- **FR-DASH-01..04**: KPI đơn hàng/doanh thu, chart doanh thu, trạng thái đơn, cảnh báo tồn kho.

### 5.4 Employees
- **FR-EMP-01..05**: CRUD nhân viên, khóa/mở tài khoản, tìm kiếm, xem chi tiết.

### 5.5 Customers
- **FR-CUS-01..05**: CRUD khách hàng, tìm kiếm, lịch sử đơn, doanh thu/công nợ, không xóa vật lý khi có giao dịch.

### 5.6 Orders
- **FR-ORD-01..10**: Tạo đơn + nhiều item, tính tiền, trạng thái, lịch sử trạng thái, lọc/tìm kiếm, tạo nhanh từ customer.

### 5.7 Estimate
- **FR-EST-01..09**: Tính diện tích/chi phí/giá đề xuất/lợi nhuận, cho override giá chốt, lưu estimate + final, recalculate.

### 5.8 Materials
- **FR-MAT-01..05**: CRUD vật tư, tồn kho, cảnh báo min stock, tra lịch sử nhập/xuất.

### 5.9 Stock In
- **FR-STIN-01..07**: Tạo phiếu nhập + item, cộng tồn, cập nhật average cost, xem chi tiết/list, xuất PDF.

### 5.10 Stock Out
- **FR-STOUT-01..08**: Tạo phiếu xuất, chặn xuất quá tồn, trừ tồn, liên kết order, xuất PDF.

### 5.11 Material Usage
- **FR-MUS-01..04**: Mapping vật tư theo order item, planned vs actual, tính cost item.

### 5.12 Payments
- **FR-PAY-01..06**: Ghi nhận thanh toán, chống vượt total, cập nhật payment status, xem lịch sử.

### 5.13 Reports
- **FR-REP-01..08**: Báo cáo doanh thu/đơn/tồn/khách hàng/vật tư với filter, chart + table, export PDF/Excel.

### 5.14 PDF
- **FR-PDF-01..06**: Xuất báo giá, đơn hàng, phiếu nhập/xuất, báo cáo và lưu log export.

### 5.15 Forecast
- **FR-FOR-01..05**: Chọn kỳ + dữ liệu lịch sử, tính forecast, lưu lịch sử, so sánh thực tế.

## 6. Business Rules
- 1 order thuộc 1 customer, có >=1 order item.
- Không cho xuất kho vượt tồn.
- Payment không vượt `orders.total_amount`.
- Đơn `DELIVERED`/`CANCELLED` hạn chế chỉnh sửa.
- Estimate là giá đề xuất, final có thể override nhưng phải lưu cả hai.
- Menu hiển thị theo `is_active`, `is_visible`, role/permission.

## 7. NFR
- Tải màn hình phổ biến < 3s.
- Bảo mật: auth, authorization, password hash, CSRF, audit logs.
- Kiến trúc dễ bảo trì theo layer.
- Hỗ trợ mở rộng nhiều kho/chi nhánh trong tương lai.

## 8. Out of scope (MVP)
- Multi-branch phức tạp
- Multi-warehouse phức tạp
- Workflow phê duyệt nhiều cấp
- Mobile app riêng
- Tích hợp kế toán ngoài

## 9. Acceptance tiêu biểu
- Tạo đơn + estimate thành công
- Nhập tăng tồn/xuất giảm tồn + chặn xuất vượt tồn
- Payment status đúng UNPAID/PARTIAL/PAID
- Menu hiển thị đúng quyền và URL vẫn bị chặn nếu không quyền
- Export PDF thành công
