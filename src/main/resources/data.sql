-- Seed Data for DocVault Legacy
-- Users (passwords are bcrypt of 'password123' — yes, all the same)
INSERT INTO users (username, email, password_hash, role, full_name, phone, address) VALUES
('admin', 'admin@docvault.com', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'ADMIN', 'Admin User', '555-0100', '123 Admin St'),
('johndoe', 'john@example.com', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'CUSTOMER', 'John Doe', '555-0101', '456 Oak Ave, Springfield IL 62701'),
('janedoe', 'jane@example.com', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'CUSTOMER', 'Jane Doe', '555-0102', '789 Elm St, Portland OR 97201');

-- Documents (products table repurposed for document records)
INSERT INTO products (name, description, price, category, stock_quantity, image_url, is_active, weight_kg, brand, sku) VALUES
('Annual Report 2023.pdf', 'Consolidated financial statements and board commentary for fiscal year 2023.', 0.00, 'REPORTS', 1, '/docs/annual-report-2023.pdf', true, 0.0, 'Finance', 'FIN-AR-2023'),
('Employee Handbook v3.docx', 'Company policies, benefits overview, and code of conduct. Updated March 2024.', 0.00, 'POLICIES', 1, '/docs/employee-handbook-v3.docx', true, 0.0, 'HR', 'HR-EH-003'),
('Q4 Revenue Analysis.xlsx', 'Regional revenue breakdown with YoY comparisons and forecasting models.', 0.00, 'SPREADSHEETS', 1, '/docs/q4-revenue-analysis.xlsx', true, 0.0, 'Finance', 'FIN-QR-Q4'),
('NDA Template - Standard.pdf', 'Mutual non-disclosure agreement template. Approved by legal January 2024.', 0.00, 'CONTRACTS', 1, '/docs/nda-template-standard.pdf', true, 0.0, 'Legal', 'LEG-NDA-001'),
('Product Roadmap 2024.pptx', 'Strategic product roadmap presentation for executive leadership review.', 0.00, 'PRESENTATIONS', 1, '/docs/product-roadmap-2024.pptx', true, 0.0, 'Product', 'PRD-RM-2024'),
('Server Architecture Diagram.png', 'Production infrastructure topology including load balancers and failover paths.', 0.00, 'TECHNICAL', 1, '/docs/server-architecture.png', true, 0.0, 'Engineering', 'ENG-SA-001'),
('Vendor Onboarding Checklist.pdf', 'Step-by-step checklist for onboarding new vendors. Compliance-required.', 0.00, 'PROCEDURES', 1, '/docs/vendor-onboarding.pdf', true, 0.0, 'Procurement', 'PROC-VO-001'),
('GDPR Compliance Audit.pdf', 'Annual GDPR compliance audit results and remediation action items.', 0.00, 'COMPLIANCE', 1, '/docs/gdpr-audit-2024.pdf', true, 0.0, 'Legal', 'LEG-GDPR-001'),
('Marketing Campaign Brief.docx', 'Q1 2024 marketing campaign brief with target audiences and budget allocation.', 0.00, 'MARKETING', 1, '/docs/campaign-brief-q1.docx', true, 0.0, 'Marketing', 'MKT-CB-Q1'),
('Board Meeting Minutes Dec.pdf', 'Minutes from December 2023 board meeting. Confidential — restricted access.', 0.00, 'GOVERNANCE', 0, '/docs/board-minutes-dec.pdf', true, 0.0, 'Executive', 'EXC-BM-DEC');

-- Orders
INSERT INTO orders (user_id, total_amount, status, shipping_address, payment_method, payment_reference, notes, created_at) VALUES
(2, 79.98, 'DELIVERED', '456 Oak Ave, Springfield IL 62701', 'CREDIT_CARD', 'ch_1abc123', NULL, TIMESTAMP '2024-01-15 10:30:00'),
(2, 129.99, 'SHIPPED', '456 Oak Ave, Springfield IL 62701', 'CREDIT_CARD', 'ch_2def456', 'Please leave at door', TIMESTAMP '2024-02-20 14:15:00'),
(3, 37.98, 'DELIVERED', '789 Elm St, Portland OR 97201', 'PAYPAL', 'pp_3ghi789', NULL, TIMESTAMP '2024-01-28 09:45:00'),
(3, 89.99, 'PROCESSING', '789 Elm St, Portland OR 97201', 'CREDIT_CARD', 'ch_4jkl012', 'Birthday gift — no receipt please', TIMESTAMP '2024-03-01 16:20:00'),
(2, 54.99, 'PENDING', '456 Oak Ave, Springfield IL 62701', 'CREDIT_CARD', NULL, NULL, TIMESTAMP '2024-03-10 11:00:00');

-- Order Items
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
(1, 1, 1, 54.99),
(1, 5, 2, 12.99),
(2, 3, 1, 129.99),
(3, 5, 1, 12.99),
(3, 4, 1, 24.99),
(4, 2, 1, 89.99),
(5, 1, 1, 54.99);

-- Cart Items (active carts)
INSERT INTO cart_items (user_id, product_id, quantity) VALUES
(2, 7, 2),
(2, 8, 1),
(3, 1, 1);

-- Audit Log
INSERT INTO audit_log (user_id, action, entity_type, entity_id, details, ip_address) VALUES
(1, 'LOGIN', 'USER', 1, 'Admin login', '192.168.1.1'),
(2, 'PLACE_ORDER', 'ORDER', 5, 'Order placed: $54.99', '10.0.0.15'),
(1, 'UPDATE_PRODUCT', 'PRODUCT', 10, 'Stock quantity set to 0 (out of stock)', '192.168.1.1');
