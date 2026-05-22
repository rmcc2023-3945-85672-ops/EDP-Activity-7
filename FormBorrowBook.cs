// ============================================================
// FormBorrowBook.cs — TRANSACTION 1: Book Borrowing
// Allows a librarian/admin to issue a book to a member.
// Validates stock (not already borrowed), records the
// borrow_date and expected return_date (+14 days default).
// ============================================================

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using LibrarySystem.Database;
using LibrarySystem.UI;

namespace LibrarySystem.Forms
{
    public class FormBorrowBook : Form
    {
        private ComboBox    cboMember = null!, cboBook = null!;
        private DateTimePicker dtpBorrow = null!, dtpReturn = null!;
        private TextBox     txtNotes = null!;
        private DataGridView dgvCurrent = null!;
        private Label       lblStatus = null!;

        public FormBorrowBook()
        {
            Text          = "Library IS — Book Borrowing";
            Size          = new Size(1050, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = UIHelper.Cream;
            BuildUI();
            LoadCombos();
            LoadCurrentBorrowings();
        }

        // ── UI ────────────────────────────────────────────────
        private void BuildUI()
        {
            // Header bar
            var hdr = new Panel { Size = new Size(1050, 68), BackColor = UIHelper.White };
            hdr.Paint += (s, e) => e.Graphics.DrawLine(new Pen(UIHelper.Border), 0, 67, 1050, 67);
            Controls.Add(hdr);

            hdr.Controls.Add(UIHelper.MakeLabel("Book Borrowing", UIHelper.FontTitle, UIHelper.Navy)
                             .Tap(l => l.Location = new Point(28, 14)));
            hdr.Controls.Add(UIHelper.MakeLabel("Home › Transactions › Borrow", UIHelper.FontSmall, UIHelper.Muted)
                             .Tap(l => l.Location = new Point(30, 46)));

            // Form card (left)
            var card = UIHelper.MakeCard(430, 480);
            card.Location = new Point(28, 90);
            Controls.Add(card);

            int y = 20;
            card.Controls.Add(UIHelper.MakeLabel("Issue Book to Member", UIHelper.FontH2, UIHelper.Navy)
                              .Tap(l => l.Location = new Point(20, y)));
            y += 38;
            card.Controls.Add(UIHelper.MakeDivider(430).Tap(l => l.Location = new Point(0, y)));
            y += 16;

            // Member
            card.Controls.Add(UIHelper.MakeCapsLabel("MEMBER").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            cboMember = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontBody,
                                       Size = new Size(390, 28), Location = new Point(20, y) };
            card.Controls.Add(cboMember);
            y += 40;

            // Book
            card.Controls.Add(UIHelper.MakeCapsLabel("BOOK").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            cboBook = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontBody,
                                     Size = new Size(390, 28), Location = new Point(20, y) };
            card.Controls.Add(cboBook);
            y += 40;

            // Borrow date
            card.Controls.Add(UIHelper.MakeCapsLabel("BORROW DATE").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            dtpBorrow = new DateTimePicker { Font = UIHelper.FontBody, Size = new Size(185, 28),
                                             Location = new Point(20, y), Format = DateTimePickerFormat.Short,
                                             Value = DateTime.Today };
            dtpBorrow.ValueChanged += (s, e) => dtpReturn.Value = dtpBorrow.Value.AddDays(14);
            card.Controls.Add(dtpBorrow);

            card.Controls.Add(UIHelper.MakeCapsLabel("RETURN DATE").Tap(l => l.Location = new Point(225, y - 20)));
            dtpReturn = new DateTimePicker { Font = UIHelper.FontBody, Size = new Size(185, 28),
                                             Location = new Point(225, y), Format = DateTimePickerFormat.Short,
                                             Value = DateTime.Today.AddDays(14) };
            card.Controls.Add(dtpReturn);
            y += 44;

            // Notes
            card.Controls.Add(UIHelper.MakeCapsLabel("NOTES (optional)").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            txtNotes = new TextBox { Font = UIHelper.FontBody, Size = new Size(390, 60),
                                     Location = new Point(20, y), Multiline = true };
            card.Controls.Add(txtNotes);
            y += 72;

            // Status label
            lblStatus = UIHelper.MakeLabel("", UIHelper.FontBody, UIHelper.Green);
            lblStatus.Location = new Point(20, y);
            lblStatus.Size     = new Size(390, 24);
            lblStatus.AutoSize = false;
            card.Controls.Add(lblStatus);
            y += 28;

            // Buttons
            var btnSave   = UIHelper.MakePrimaryBtn("✔  Issue Book", new Size(185, 40));
            btnSave.Location = new Point(20, y);
            btnSave.Click += BtnSave_Click;
            card.Controls.Add(btnSave);

            var btnClear = UIHelper.MakeOutlineBtn("✖  Clear", new Size(100, 40));
            btnClear.Location = new Point(220, y);
            btnClear.Click += (s, e) => { cboMember.SelectedIndex = -1; cboBook.SelectedIndex = -1;
                                          dtpBorrow.Value = DateTime.Today; dtpReturn.Value = DateTime.Today.AddDays(14);
                                          txtNotes.Clear(); lblStatus.Text = ""; };
            card.Controls.Add(btnClear);

            // Current borrowings grid (right)
            var gridCard = UIHelper.MakeCard(550, 570);
            gridCard.Location = new Point(476, 90);
            Controls.Add(gridCard);

            gridCard.Controls.Add(UIHelper.MakeLabel("Active Borrowings", UIHelper.FontH2, UIHelper.Navy)
                                  .Tap(l => l.Location = new Point(16, 14)));

            dgvCurrent = UIHelper.MakeGrid();
            dgvCurrent.Location = new Point(0, 50);
            dgvCurrent.Size     = new Size(550, 520);
            gridCard.Controls.Add(dgvCurrent);
        }

        // ── Load combos ───────────────────────────────────────
        private void LoadCombos()
        {
            var db = DatabaseConnection.Instance;

            var membDt = db.FillDataTable("SELECT member_id, full_name FROM members WHERE status='active' ORDER BY full_name");
            cboMember.DataSource    = membDt;
            cboMember.DisplayMember = "full_name";
            cboMember.ValueMember   = "member_id";
            cboMember.SelectedIndex = -1;

            // Books that are NOT currently borrowed AND not reserved by someone else
            var bookDt = db.FillDataTable(
                @"SELECT b.book_id, CONCAT(b.title,' – ',a.author_name) AS book_label
                  FROM books b
                  JOIN authors a ON a.author_id = b.author_id
                  WHERE b.book_id NOT IN (
                      SELECT book_id FROM borrowings WHERE status='borrowed' OR status='overdue'
                  )
                  AND b.book_id NOT IN (
                      SELECT book_id FROM reservations WHERE status='pending'
                  )
                  ORDER BY b.title");
            cboBook.DataSource    = bookDt;
            cboBook.DisplayMember = "book_label";
            cboBook.ValueMember   = "book_id";
            cboBook.SelectedIndex = -1;
        }

        private void LoadCurrentBorrowings()
        {
            var dt = DatabaseConnection.Instance.FillDataTable(
                @"SELECT br.borrow_id AS ID,
                         m.full_name  AS Member,
                         b.title      AS Book,
                         br.borrow_date AS 'Borrowed',
                         br.return_date AS 'Due',
                         br.status     AS Status
                  FROM borrowings br
                  JOIN members m ON m.member_id = br.member_id
                  JOIN books   b ON b.book_id   = br.book_id
                  WHERE br.status IN ('borrowed','overdue')
                  ORDER BY br.return_date");
            dgvCurrent.DataSource = dt;
        }

        // ── Save ──────────────────────────────────────────────
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cboMember.SelectedIndex < 0 || cboBook.SelectedIndex < 0)
            { ShowMsg("Please select both a member and a book.", UIHelper.Red); return; }

            int memberId = (int)(cboMember.SelectedValue ?? 0);
            int bookId   = (int)(cboBook.SelectedValue ?? 0);

            // Double-check availability
            var check = DatabaseConnection.Instance.ExecuteScalar(
                "SELECT COUNT(*) FROM borrowings WHERE book_id=@b AND status IN ('borrowed','overdue')",
                new MySqlParameter("@b", bookId));
            if (Convert.ToInt32(check) > 0)
            { ShowMsg("Book is already borrowed by another member.", UIHelper.Red); return; }

            // Check if the book is reserved by someone else
            var reservedBy = DatabaseConnection.Instance.ExecuteScalar(
                "SELECT member_id FROM reservations WHERE book_id=@b AND status='pending' LIMIT 1",
                new MySqlParameter("@b", bookId));
            if (reservedBy != null && reservedBy != DBNull.Value && Convert.ToInt32(reservedBy) != memberId)
            { ShowMsg("This book is reserved by another member. Fulfil their reservation first.", UIHelper.Red); return; }

            DatabaseConnection.Instance.ExecuteNonQuery(
                @"INSERT INTO borrowings (member_id, book_id, borrow_date, return_date, status)
                  VALUES (@m, @b, @bd, @rd, 'borrowed')",
                new MySqlParameter("@m",  memberId),
                new MySqlParameter("@b",  bookId),
                new MySqlParameter("@bd", dtpBorrow.Value.Date),
                new MySqlParameter("@rd", dtpReturn.Value.Date));

            ShowMsg("✔ Book issued successfully!", UIHelper.Green);
            LoadCombos();
            LoadCurrentBorrowings();
            txtNotes.Clear();
            cboMember.SelectedIndex = -1;
            cboBook.SelectedIndex   = -1;
        }

        private void ShowMsg(string msg, Color c)
        { lblStatus.Text = msg; lblStatus.ForeColor = c; }
    }

}
