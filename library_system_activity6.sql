-- ============================================================
-- Activity 6 — Transaction & Report Module additions
-- Run AFTER library_system_full.sql
-- ============================================================

USE library_system;

-- Add actual_return_date column if it doesn't exist (for existing databases)
ALTER TABLE borrowings ADD COLUMN IF NOT EXISTS actual_return_date DATE;

-- ── 1. BOOK RETURNS (detail rows that belong to a borrowing) ─
-- Already covered by borrowings.status; no extra table needed.

-- ── 2. BOOK RESERVATIONS ─────────────────────────────────────
CREATE TABLE IF NOT EXISTS reservations (
    reservation_id  INT PRIMARY KEY AUTO_INCREMENT,
    member_id       INT NOT NULL,
    book_id         INT NOT NULL,
    reserved_date   DATE NOT NULL,
    expiry_date     DATE NOT NULL,
    status          ENUM('pending','fulfilled','cancelled','expired') DEFAULT 'pending',
    notes           VARCHAR(255),
    FOREIGN KEY (member_id) REFERENCES members(member_id),
    FOREIGN KEY (book_id)   REFERENCES books(book_id)
);

-- ── 3. FINE PAYMENTS ─────────────────────────────────────────
-- Extends the existing fines table with payment records
CREATE TABLE IF NOT EXISTS fine_payments (
    payment_id      INT PRIMARY KEY AUTO_INCREMENT,
    fine_id         INT NOT NULL,
    amount_paid     DECIMAL(6,2) NOT NULL,
    payment_date    DATE NOT NULL,
    received_by     INT NOT NULL,          -- member_id of librarian
    payment_method  ENUM('cash','online','waived') DEFAULT 'cash',
    remarks         VARCHAR(255),
    FOREIGN KEY (fine_id)       REFERENCES fines(fine_id),
    FOREIGN KEY (received_by)   REFERENCES members(member_id)
);

-- ── SAMPLE DATA ───────────────────────────────────────────────
-- Reservations
INSERT INTO reservations (member_id, book_id, reserved_date, expiry_date, status) VALUES
(2, 3, CURDATE() - INTERVAL 3 DAY, CURDATE() + INTERVAL 4 DAY, 'pending'),
(3, 5, CURDATE() - INTERVAL 1 DAY, CURDATE() + INTERVAL 6 DAY, 'pending'),
(4, 1, CURDATE() - INTERVAL 7 DAY, CURDATE() - INTERVAL 0 DAY, 'fulfilled'),
(7, 6, CURDATE() - INTERVAL 2 DAY, CURDATE() + INTERVAL 5 DAY, 'pending'),
(8, 9, CURDATE() - INTERVAL 10 DAY, CURDATE() - INTERVAL 3 DAY, 'cancelled');

-- Borrow some books (if not already done)
INSERT IGNORE INTO borrowings (member_id, book_id, borrow_date, return_date, status) VALUES
(2, 2, CURDATE() - INTERVAL 15 DAY, CURDATE() - INTERVAL 8 DAY, 'returned'),
(3, 4, CURDATE() - INTERVAL 20 DAY, CURDATE() - INTERVAL 5 DAY, 'returned'),
(4, 6, CURDATE() - INTERVAL 5  DAY, CURDATE() + INTERVAL 9 DAY, 'borrowed'),
(7, 7, CURDATE() - INTERVAL 30 DAY, CURDATE() - INTERVAL 16 DAY,'overdue'),
(8, 1, CURDATE() - INTERVAL 3  DAY, CURDATE() + INTERVAL 11 DAY,'borrowed');

-- Fines for overdue
INSERT IGNORE INTO fines (borrow_id, amount, paid) VALUES
(4, 70.00, 'no'),
(5, 40.00, 'no');

-- Fine payments
INSERT INTO fine_payments (fine_id, amount_paid, payment_date, received_by, payment_method) VALUES
(1, 70.00, CURDATE(), 1, 'cash');

-- ── CLEANUP: Cancel pending reservations for books already borrowed ─
UPDATE reservations r
JOIN borrowings br ON br.book_id = r.book_id
SET r.status = 'cancelled'
WHERE r.status = 'pending'
AND br.status IN ('borrowed', 'overdue');
