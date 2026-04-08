# Role/Permission Matrix (Draft)

| Module | Admin | Sales | Warehouse | Production | Accountant | Manager |
|---|---|---|---|---|---|---|
| Dashboard | CRUD | R | R | R | R | R |
| Employees | CRUD | - | - | - | - | R |
| Customers | CRUD | CRUD | R | R | R | R |
| Orders | CRUD | CRUD | R | U(status/material) | R | R |
| Estimate | CRUD | CRUD | R | R | R | R |
| Materials | CRUD | R | CRUD | R | R | R |
| Stock In | CRUD | - | CRUD | - | R | R |
| Stock Out | CRUD | - | CRUD | R | R | R |
| Payments | CRUD | R | - | - | CRUD | R |
| Reports | CRUD | R | R | R | R | R |
| Forecast | CRUD | R | - | - | R | CRUD |
| Menu Config | CRUD | - | - | - | - | - |
| Role/Permission Config | CRUD | - | - | - | - | - |

## Ghi chú
- `R`: Read, `U`: Update, `CRUD`: Create/Read/Update/Delete.
- Cần tách quyền chi tiết theo action-level (`orders.update_status`, `payments.create`, `menus.manage`, ...).
- Quyền URL phải kiểm tra độc lập với hiển thị menu.
