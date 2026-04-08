# QuangDa ERP - Hệ thống quản lý nhà in

Repo khởi tạo cho **mini ERP quản lý nhà in** theo SRS đã chốt:

- ASP.NET Core MVC (định hướng triển khai)
- PostgreSQL
- Quản lý nhân viên, khách hàng, đơn hàng
- Quản lý kho (nguyên vật liệu, nhập/xuất)
- Thanh toán, báo cáo, xuất PDF
- Forecast doanh thu
- Dynamic menu từ DB
- Estimate giá khi tạo order

## 1) Mục tiêu

Chuẩn hóa một codebase để team có thể bắt đầu triển khai ngay với:

- Tài liệu SRS đầy đủ
- Khung dữ liệu PostgreSQL cho nghiệp vụ chính
- Quy ước module và backlog MVP
- Môi trường local DB bằng Docker
- CI cơ bản để kiểm tra thay đổi tài liệu/SQL

## 2) Cấu trúc repo

```text
.
├── .github/workflows/ci.yml
├── database/schema.sql
├── docs/
│   ├── MVP_BACKLOG.md
│   ├── ROLE_PERMISSION_MATRIX.md
│   └── SRS.md
├── src/PrintERP.Web/README.md
├── .env.example
├── docker-compose.yml
└── README.md
```

## 3) Khởi động PostgreSQL local

```bash
cp .env.example .env
docker compose up -d
```

Database schema:

```bash
docker compose exec postgres psql -U $POSTGRES_USER -d $POSTGRES_DB -f /docker-entrypoint-initdb.d/schema.sql
```

## 4) Lộ trình triển khai kỹ thuật đề xuất

### Phase 1 (Core)
- Auth + RBAC + Dynamic menu
- Customers, Employees
- Orders + Estimate
- Materials + Stock In/Out

### Phase 2 (Finance & Reporting)
- Payments + công nợ
- Revenue/Inventory reports
- Export PDF

### Phase 3 (Advanced)
- Forecast doanh thu
- Phân tích cost/profit nâng cao
- Tối ưu recipe vật tư

## 5) Định hướng code (khi bắt đầu implement ASP.NET)

```text
src/
  PrintERP.Web/             # MVC UI
  PrintERP.Application/     # Services / UseCases
  PrintERP.Domain/          # Entities / Business rules
  PrintERP.Infrastructure/  # EF Core / Repositories / Integrations
tests/
  PrintERP.UnitTests/
  PrintERP.IntegrationTests/
```

## 6) Ghi chú

Repo hiện là **foundation** để team bắt đầu sprint 0/sprint 1. Nếu bạn muốn, bước tiếp theo mình có thể tạo luôn:

1. ASP.NET Core MVC skeleton (solution + projects)
2. Entity models + DbContext từ `database/schema.sql`
3. Seed data (roles/permissions/menus)
4. API/Controller cho module Orders + Estimate
