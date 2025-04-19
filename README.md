

## DB insert 語法

### nug_users

#### create
```sql
CREATE TABLE nug_users (
    uuid UUID PRIMARY KEY DEFAULT gen_random_uuid(), 
    id VARCHAR(50),
    password VARCHAR(255) NOT NULL,
    name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    address TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

#### trigger
```sql
-- 1. 建立 function
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
   NEW.updated_at = CURRENT_TIMESTAMP;
   RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 2. 建立 trigger，掛在 nug_users 資料表上
CREATE TRIGGER set_updated_at
BEFORE UPDATE ON nug_users
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();
```

#### INSERT
```sql
INSERT INTO nug_users (id, password, name, phone, address)
VALUES ('USR001', 'password123', '王小明', '0912345678', '台北市大安區...');
```

```
ALTER TABLE nug_users ADD CONSTRAINT uq_user_id UNIQUE(id);
```

### nug_store

```sql
CREATE TABLE nug_store (
    uuid UUID PRIMARY KEY DEFAULT gen_random_uuid(),           -- 商店唯一識別碼
    owner_id UUID NOT NULL REFERENCES nug_users(uuid),         -- 商店擁有者，連結使用者表
    store_name VARCHAR(100) NOT NULL,                          -- 商店名稱
    store_type VARCHAR(50),                                    -- 商店類型（例如 咖啡、服飾）
    phone VARCHAR(20),                                         -- 聯絡電話
    address TEXT,                                              -- 商店地址
    is_active BOOLEAN DEFAULT TRUE,                            -- 是否營業中
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,            -- 建立時間
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP             -- 更新時間
);
```

#### create
```sql
CREATE TABLE nug_store (
    uuid UUID PRIMARY KEY DEFAULT gen_random_uuid(),      -- 主鍵，不對外用
    store_id VARCHAR(50) UNIQUE NOT NULL,                 -- 商店登入帳號／識別用
    owner_id UUID NOT NULL REFERENCES nug_users(uuid),    -- 擁有者
    store_name VARCHAR(100) NOT NULL,                     -- 商店名稱
    store_type VARCHAR(50),                               -- 商店類型
    phone VARCHAR(20),
    address TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

#### trigger
```sql
-- 建立自動更新 updated_at 的 function
CREATE OR REPLACE FUNCTION update_store_updated_at()
RETURNS TRIGGER AS $$
BEGIN
   NEW.updated_at = CURRENT_TIMESTAMP;
   RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 設定 trigger 掛在 nug_store 表格上
CREATE TRIGGER trg_update_store_updated_at
BEFORE UPDATE ON nug_store
FOR EACH ROW
EXECUTE FUNCTION update_store_updated_at();
```

#### insert 
```sql
INSERT INTO nug_store (store_id, owner_id, store_name, store_type, phone, address)
VALUES (
    'store001',
    (SELECT uuid FROM nug_users WHERE id = 'USR001'),
    '小明的雜貨店',
    '雜貨',
    '02-8765-4321',
    '台中市西屯區某某街99號'
);

```

###

```sql
CREATE TABLE nug_products (
    id SERIAL PRIMARY KEY,
    store_id VARCHAR(50) NOT NULL,    -- 修改為字符串類型
    name VARCHAR(255) NOT NULL,
    category VARCHAR(100),
    cost DECIMAL(10, 2),
    price DECIMAL(10, 2),
    unit VARCHAR(50),
    unit_count INTEGER,
    use_yn CHAR(1) DEFAULT 'Y',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 可能需要的索引
CREATE INDEX idx_nug_products_store_id ON nug_products(store_id);
CREATE INDEX idx_nug_products_category ON nug_products(category);

```

```sql
-- 創建觸發器函數
CREATE OR REPLACE FUNCTION update_product_updated_at()
RETURNS TRIGGER AS $$
BEGIN
   NEW.updated_at = CURRENT_TIMESTAMP;
   RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 設定 trigger 掛在 nug_products 表格上
CREATE TRIGGER trg_update_product_updated_at
BEFORE UPDATE ON nug_products
FOR EACH ROW
EXECUTE FUNCTION update_product_updated_at();
```

```sql
INSERT INTO nug_products (store_id, name, category, cost, price, unit, unit_count, use_yn) 
VALUES ($1, $2, $3, $4, $5, $6, $7, $8);
```