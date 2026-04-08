-- Print ERP initial schema for PostgreSQL

CREATE TABLE IF NOT EXISTS roles (
    id BIGSERIAL PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS permissions (
    id BIGSERIAL PRIMARY KEY,
    code VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(150) NOT NULL,
    module VARCHAR(100),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS role_permissions (
    role_id BIGINT NOT NULL REFERENCES roles(id),
    permission_id BIGINT NOT NULL REFERENCES permissions(id),
    PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE IF NOT EXISTS employees (
    id BIGSERIAL PRIMARY KEY,
    employee_code VARCHAR(50) UNIQUE NOT NULL,
    full_name VARCHAR(150) NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(150),
    address TEXT,
    department VARCHAR(100),
    title VARCHAR(100),
    join_date DATE,
    username VARCHAR(80) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS employee_roles (
    employee_id BIGINT NOT NULL REFERENCES employees(id),
    role_id BIGINT NOT NULL REFERENCES roles(id),
    PRIMARY KEY (employee_id, role_id)
);

CREATE TABLE IF NOT EXISTS customers (
    id BIGSERIAL PRIMARY KEY,
    customer_code VARCHAR(50) UNIQUE NOT NULL,
    customer_type VARCHAR(20) NOT NULL,
    name VARCHAR(200) NOT NULL,
    contact_name VARCHAR(150),
    phone VARCHAR(20),
    email VARCHAR(150),
    address TEXT,
    tax_code VARCHAR(50),
    note TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS material_categories (
    id BIGSERIAL PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(150) NOT NULL
);

CREATE TABLE IF NOT EXISTS materials (
    id BIGSERIAL PRIMARY KEY,
    material_code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(200) NOT NULL,
    category_id BIGINT REFERENCES material_categories(id),
    unit VARCHAR(20) NOT NULL,
    current_stock NUMERIC(18,4) NOT NULL DEFAULT 0,
    min_stock_level NUMERIC(18,4) NOT NULL DEFAULT 0,
    average_cost NUMERIC(18,2) NOT NULL DEFAULT 0,
    specification TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS menus (
    id BIGSERIAL PRIMARY KEY,
    code VARCHAR(100) UNIQUE NOT NULL,
    title VARCHAR(150) NOT NULL,
    parent_id BIGINT REFERENCES menus(id),
    menu_type VARCHAR(20) NOT NULL DEFAULT 'ITEM',
    area VARCHAR(100),
    controller VARCHAR(100),
    action VARCHAR(100),
    url VARCHAR(300),
    route_values JSONB,
    icon VARCHAR(100),
    sort_order INT NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_visible BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS menu_roles (
    menu_id BIGINT NOT NULL REFERENCES menus(id),
    role_id BIGINT NOT NULL REFERENCES roles(id),
    PRIMARY KEY (menu_id, role_id)
);

CREATE TABLE IF NOT EXISTS orders (
    id BIGSERIAL PRIMARY KEY,
    order_code VARCHAR(50) UNIQUE NOT NULL,
    customer_id BIGINT NOT NULL REFERENCES customers(id),
    sales_employee_id BIGINT REFERENCES employees(id),
    owner_employee_id BIGINT REFERENCES employees(id),
    order_date DATE NOT NULL,
    delivery_date DATE,
    status VARCHAR(20) NOT NULL DEFAULT 'NEW',
    payment_status VARCHAR(20) NOT NULL DEFAULT 'UNPAID',
    note TEXT,
    discount_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    tax_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    subtotal_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    total_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS order_items (
    id BIGSERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_type VARCHAR(100),
    item_name VARCHAR(200) NOT NULL,
    description TEXT,
    width NUMERIC(18,4),
    height NUMERIC(18,4),
    unit VARCHAR(20),
    quantity NUMERIC(18,4) NOT NULL,
    area NUMERIC(18,4),
    material_description TEXT,
    print_type VARCHAR(100),
    finishing VARCHAR(150),
    estimated_unit_price NUMERIC(18,2),
    estimated_line_total NUMERIC(18,2),
    final_unit_price NUMERIC(18,2),
    final_line_total NUMERIC(18,2),
    estimated_cost NUMERIC(18,2),
    estimated_profit NUMERIC(18,2),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS order_status_histories (
    id BIGSERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    old_status VARCHAR(20),
    new_status VARCHAR(20) NOT NULL,
    changed_by BIGINT REFERENCES employees(id),
    note TEXT,
    changed_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS stock_in (
    id BIGSERIAL PRIMARY KEY,
    stock_in_code VARCHAR(50) UNIQUE NOT NULL,
    supplier_name VARCHAR(150),
    stock_in_date DATE NOT NULL,
    note TEXT,
    created_by BIGINT REFERENCES employees(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS stock_in_items (
    id BIGSERIAL PRIMARY KEY,
    stock_in_id BIGINT NOT NULL REFERENCES stock_in(id) ON DELETE CASCADE,
    material_id BIGINT NOT NULL REFERENCES materials(id),
    quantity NUMERIC(18,4) NOT NULL,
    unit_price NUMERIC(18,2) NOT NULL,
    line_total NUMERIC(18,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS stock_out (
    id BIGSERIAL PRIMARY KEY,
    stock_out_code VARCHAR(50) UNIQUE NOT NULL,
    stock_out_date DATE NOT NULL,
    order_id BIGINT REFERENCES orders(id),
    purpose VARCHAR(150),
    note TEXT,
    created_by BIGINT REFERENCES employees(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS stock_out_items (
    id BIGSERIAL PRIMARY KEY,
    stock_out_id BIGINT NOT NULL REFERENCES stock_out(id) ON DELETE CASCADE,
    material_id BIGINT NOT NULL REFERENCES materials(id),
    quantity NUMERIC(18,4) NOT NULL,
    unit_cost NUMERIC(18,2) NOT NULL,
    line_total NUMERIC(18,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS material_usages (
    id BIGSERIAL PRIMARY KEY,
    order_item_id BIGINT NOT NULL REFERENCES order_items(id) ON DELETE CASCADE,
    material_id BIGINT NOT NULL REFERENCES materials(id),
    planned_quantity NUMERIC(18,4) NOT NULL DEFAULT 0,
    actual_quantity NUMERIC(18,4) NOT NULL DEFAULT 0,
    unit_cost NUMERIC(18,2) NOT NULL DEFAULT 0,
    total_cost NUMERIC(18,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS payments (
    id BIGSERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    payment_date DATE NOT NULL,
    amount NUMERIC(18,2) NOT NULL,
    method VARCHAR(50),
    reference_code VARCHAR(100),
    note TEXT,
    created_by BIGINT REFERENCES employees(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS report_exports (
    id BIGSERIAL PRIMARY KEY,
    report_type VARCHAR(100) NOT NULL,
    file_name VARCHAR(250) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    filter_json JSONB,
    exported_by BIGINT REFERENCES employees(id),
    exported_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS revenue_forecasts (
    id BIGSERIAL PRIMARY KEY,
    forecast_period_type VARCHAR(20) NOT NULL,
    target_period VARCHAR(20) NOT NULL,
    history_window_months INT NOT NULL,
    predicted_revenue NUMERIC(18,2) NOT NULL,
    model_name VARCHAR(100) NOT NULL,
    input_summary JSONB,
    created_by BIGINT REFERENCES employees(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_orders_customer_id ON orders(customer_id);
CREATE INDEX IF NOT EXISTS idx_orders_status ON orders(status);
CREATE INDEX IF NOT EXISTS idx_orders_order_date ON orders(order_date);
CREATE INDEX IF NOT EXISTS idx_order_items_order_id ON order_items(order_id);
CREATE INDEX IF NOT EXISTS idx_materials_stock ON materials(current_stock, min_stock_level);
CREATE INDEX IF NOT EXISTS idx_payments_order_id ON payments(order_id);
