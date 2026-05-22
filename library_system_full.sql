-- ============================================================
-- Activity 3 + Activity 4 Combined SQL Schema
-- Library Information System — with Authentication Support
-- ============================================================

DROP DATABASE IF EXISTS library_system;
CREATE DATABASE library_system;
USE library_system;

-- ═══════════════════════════════════════════════
-- TABLES (Normalized)
-- ═══════════════════════════════════════════════

-- 1. MEMBERS (extended with auth columns for Activity 4)
CREATE TABLE members (
    member_id       INT PRIMARY KEY AUTO_INCREMENT,
    full_name       VARCHAR(100)  NOT NULL,
    email           VARCHAR(100)  UNIQUE NOT NULL,
    password        VARCHAR(255)  NOT NULL,          -- SHA-256 hashed
    recovery_pin    VARCHAR(6)    DEFAULT NULL,       -- 6-digit OTP for password recovery
    role            ENUM('member','librarian','admin') DEFAULT 'member',
    membership_date DATE,
    status          ENUM('active','inactive') DEFAULT 'active'
);

-- 2. AUTHORS
CREATE TABLE authors (
    author_id   INT PRIMARY KEY AUTO_INCREMENT,
    author_name VARCHAR(100)
);

-- 3. BOOKS
CREATE TABLE books (
    book_id   INT PRIMARY KEY AUTO_INCREMENT,
    title     VARCHAR(150),
    author_id INT,
    genre     VARCHAR(50),
    FOREIGN KEY (author_id) REFERENCES authors(author_id)
);

-- 4. BORROWINGS
CREATE TABLE borrowings (
    borrow_id          INT PRIMARY KEY AUTO_INCREMENT,
    member_id          INT,
    book_id            INT,
    borrow_date        DATE,
    return_date        DATE,
    actual_return_date DATE,
    status             ENUM('borrowed','returned','overdue'),
    FOREIGN KEY (member_id) REFERENCES members(member_id),
    FOREIGN KEY (book_id)   REFERENCES books(book_id)
);

-- 5. FINES
CREATE TABLE fines (
    fine_id   INT PRIMARY KEY AUTO_INCREMENT,
    borrow_id INT,
    amount    DECIMAL(6,2),
    paid      ENUM('yes','no'),
    FOREIGN KEY (borrow_id) REFERENCES borrowings(borrow_id)
);

-- ═══════════════════════════════════════════════
-- SAMPLE DATA
-- Passwords are SHA-256("password") hash
-- In C#: PasswordHelper.Hash("password")
-- ═══════════════════════════════════════════════

-- All accounts use password: "password"
-- SHA-256 of "password" = XohImNooBHFR0OnijDpYDIWr18Ru9R...
-- (Use the C# PasswordHelper.Hash() to generate real hashes)

INSERT INTO members (full_name, email, password, role, membership_date, status) VALUES
('Juan Dela Cruz',  'juan@email.com',  SHA2('password',256), 'admin',     '2025-01-01', 'active'),
('Maria Santos',    'maria@email.com', SHA2('password',256), 'member',    '2025-01-02', 'active'),
('Pedro Reyes',     'pedro@email.com', SHA2('password',256), 'member',    '2025-01-03', 'active'),
('Ana Lopez',       'ana@email.com',   SHA2('password',256), 'member',    '2025-01-04', 'active'),
('Carlo Gomez',     'carlo@email.com', SHA2('password',256), 'member',    '2025-01-05', 'inactive'),
('Liza Ramos',      'liza@email.com',  SHA2('password',256), 'librarian', '2025-01-06', 'active'),
('Mark Flores',     'mark@email.com',  SHA2('password',256), 'member',    '2025-01-07', 'active'),
('Jane Cruz',       'jane@email.com',  SHA2('password',256), 'member',    '2025-01-08', 'active'),
('Leo Tan',         'leo@email.com',   SHA2('password',256), 'member',    '2025-01-09', 'inactive'),
('Nina Cruz',       'nina@email.com',  SHA2('password',256), 'member',    '2025-01-10', 'active');

INSERT INTO authors (author_name) VALUES
('J.K. Rowling'), ('George Orwell'), ('J.R.R. Tolkien'), ('Agatha Christie'),
('Stephen King'), ('Dan Brown'), ('Rick Riordan'), ('Paulo Coelho'),
('Haruki Murakami'), ('Jane Austen');

INSERT INTO books (title, author_id, genre) VALUES
('Harry Potter',                 1, 'Fantasy'),
('1984',                         2, 'Dystopian'),
('Lord of the Rings',            3, 'Fantasy'),
('Murder on the Orient Express', 4, 'Mystery'),
('The Shining',                  5, 'Horror'),
('The Da Vinci Code',            6, 'Thriller'),
('Percy Jackson',                7, 'Fantasy'),
('The Alchemist',                8, 'Fiction'),
('Norwegian Wood',               9, 'Romance'),
('Pride and Prejudice',         10, 'Classic');

INSERT INTO borrowings (member_id, book_id, borrow_date, return_date, status) VALUES
(1,  1,  '2026-01-01', '2026-01-10', 'returned'),
(2,  2,  '2026-01-02', '2026-01-12', 'returned'),
(3,  3,  '2026-01-03', NULL,         'borrowed'),
(4,  4,  '2026-01-04', '2026-01-15', 'returned'),
(5,  5,  '2026-01-05', NULL,         'overdue'),
(6,  6,  '2026-01-06', '2026-01-16', 'returned'),
(7,  7,  '2026-01-07', NULL,         'borrowed'),
(8,  8,  '2026-01-08', '2026-01-18', 'returned'),
(9,  9,  '2026-01-09', NULL,         'overdue'),
(10, 10, '2026-01-10', '2026-01-20', 'returned');

INSERT INTO fines (borrow_id, amount, paid) VALUES
(1, 0,   'yes'), (2, 0,   'yes'), (3, 50,  'no'),
(4, 0,   'yes'), (5, 100, 'no'),  (6, 0,   'yes'),
(7, 30,  'no'),  (8, 0,   'yes'), (9, 120, 'no'),
(10, 0,  'yes');

-- ═══════════════════════════════════════════════
-- VIEWS (from Activity 3 — unchanged)
-- ═══════════════════════════════════════════════

CREATE VIEW view_borrowed_books AS
SELECT m.full_name, b.title, br.borrow_date, br.status
FROM borrowings br
JOIN members m ON br.member_id = m.member_id
JOIN books   b ON br.book_id   = b.book_id;

CREATE VIEW view_overdue_books AS
SELECT m.full_name, b.title, br.borrow_date
FROM borrowings br
JOIN members m ON br.member_id = m.member_id
JOIN books   b ON br.book_id   = b.book_id
WHERE br.status = 'overdue';

CREATE VIEW view_fine_summary AS
SELECT m.full_name, SUM(f.amount) AS total_fines
FROM fines f
JOIN borrowings br ON f.borrow_id = br.borrow_id
JOIN members    m  ON br.member_id = m.member_id
GROUP BY m.full_name;

-- ═══════════════════════════════════════════════
-- STORED PROCEDURES (from Activity 3 — unchanged)
-- ═══════════════════════════════════════════════

DELIMITER //
CREATE PROCEDURE GetMemberBorrowHistory(IN memberId INT)
BEGIN
    SELECT b.title, br.borrow_date, br.return_date, br.status
    FROM borrowings br
    JOIN books b ON br.book_id = b.book_id
    WHERE br.member_id = memberId;
END //
DELIMITER ;

DELIMITER //
CREATE FUNCTION GetTotalFines(memberId INT)
RETURNS DECIMAL(10,2)
DETERMINISTIC
BEGIN
    DECLARE total DECIMAL(10,2);
    SELECT SUM(f.amount) INTO total
    FROM fines f
    JOIN borrowings br ON f.borrow_id = br.borrow_id
    WHERE br.member_id = memberId;
    RETURN IFNULL(total, 0);
END //
DELIMITER ;

-- ═══════════════════════════════════════════════
-- TRIGGERS (from Activity 3 — unchanged)
-- ═══════════════════════════════════════════════

DELIMITER //
CREATE TRIGGER trg_after_borrow_insert
AFTER INSERT ON borrowings FOR EACH ROW
BEGIN
    IF NEW.status = 'overdue' THEN
        INSERT INTO fines (borrow_id, amount, paid) VALUES (NEW.borrow_id, 50, 'no');
    END IF;
END //
DELIMITER ;

DELIMITER //
CREATE TRIGGER trg_after_borrow_update
AFTER UPDATE ON borrowings FOR EACH ROW
BEGIN
    IF NEW.status = 'returned' THEN
        UPDATE fines SET paid = 'yes' WHERE borrow_id = NEW.borrow_id;
    END IF;
END //
DELIMITER ;

DELIMITER //
CREATE TRIGGER trg_before_borrow_delete
BEFORE DELETE ON borrowings FOR EACH ROW
BEGIN
    DECLARE fine_status VARCHAR(10);
    SELECT paid INTO fine_status FROM fines WHERE borrow_id = OLD.borrow_id LIMIT 1;
    IF fine_status = 'no' THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Cannot delete borrowing record with unpaid fines.';
    END IF;
END //
DELIMITER ;

-- ═══════════════════════════════════════════════
-- QUERIES USED BY THE C# APPLICATION
-- ═══════════════════════════════════════════════

-- LOGIN (Form1.cs)
-- SELECT member_id, full_name, email, password, role, status
-- FROM   members
-- WHERE  email = 'juan@email.com' AND status = 'active';

-- PASSWORD RECOVERY — check email (FormPasswordRecovery.cs)
-- SELECT member_id FROM members WHERE email = 'juan@email.com' LIMIT 1;

-- PASSWORD RECOVERY — save OTP
-- UPDATE members SET recovery_pin = '473821' WHERE email = 'juan@email.com';

-- PASSWORD RECOVERY — verify OTP
-- SELECT recovery_pin FROM members WHERE email = 'juan@email.com';

-- PASSWORD RECOVERY — reset password
-- UPDATE members SET password = SHA2('newpassword',256), recovery_pin = NULL
-- WHERE  email = 'juan@email.com';

-- USER MGMT — list all (FormUserManagement.cs)
-- SELECT member_id, full_name, email, role, membership_date, status
-- FROM   members ORDER BY member_id DESC;

-- USER MGMT — search
-- SELECT * FROM members WHERE full_name LIKE '%cruz%' OR email LIKE '%cruz%';

-- USER MGMT — add account
-- INSERT INTO members (full_name, email, password, role, membership_date, status)
-- VALUES ('New User','new@email.com', SHA2('pass123',256), 'member', CURDATE(), 'active');

-- USER MGMT — update profile
-- UPDATE members SET full_name='Updated Name', email='new@email.com',
--        role='librarian', status='active'
-- WHERE  member_id = 1;

-- USER MGMT — activate
-- UPDATE members SET status = 'active'   WHERE member_id = 5;

-- USER MGMT — deactivate
-- UPDATE members SET status = 'inactive' WHERE member_id = 5;

-- DASHBOARD — stats
-- SELECT COUNT(*) FROM members;
-- SELECT COUNT(*) FROM members WHERE status='active';
-- SELECT COUNT(*) FROM borrowings WHERE status='borrowed';
-- SELECT COUNT(*) FROM borrowings WHERE status='overdue';
-- SELECT COALESCE(SUM(amount),0) FROM fines WHERE paid='no';
