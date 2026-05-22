// ============================================================
// FormBookReservation.cs — TRANSACTION 2: Book Reservation
// Members can reserve an unavailable book. Librarians can
// fulfil or cancel pending reservations.
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
    public class FormBookReservation : Form
    {
        private ComboBox     cboMember = null!, cboBook = null!;
        private DateTimePicker dtpExpiry = null!;
        private TextBox      txtNotes = null!;
        private DataGridView dgvReservations = null!;
        private Label        lblStatus = null!;
        private int          _editId = -1;

        public FormBookReservation()
        {
            Text          = "Library IS — Book Reservations";
            Size          = new Size(1050, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = UIHelper.Cream;
            BuildUI();
            LoadCombos();
            LoadReservations();
        }

        private void BuildUI()
        {
            // Header
            var hdr = new Panel { Size = new Size(1050, 68), BackColor = UIHelper.White };
            hdr.Paint += (s, e) => e.Graphics.DrawLine(new Pen(UIHelper.Border), 0, 67, 1050, 67);
            Controls.Add(hdr);
            hdr.Controls.Add(UIHelper.MakeLabel("Book Reservations", UIHelper.FontTitle, UIHelper.Navy)
                             .Tap(l => l.Location = new Point(28, 14)));
            hdr.Controls.Add(UIHelper.MakeLabel("Home › Transactions › Reserve", UIHelper.FontSmall, UIHelper.Muted)
                             .Tap(l => l.Location = new Point(30, 46)));

            // Form card
            var card = UIHelper.MakeCard(430, 440);
            card.Location = new Point(28, 90);
            Controls.Add(card);

            int y = 20;
            card.Controls.Add(UIHelper.MakeLabel("New Reservation", UIHelper.FontH2, UIHelper.Navy)
                              .Tap(l => l.Location = new Point(20, y)));
            y += 38;
            card.Controls.Add(UIHelper.MakeDivider(430).Tap(l => l.Location = new Point(0, y)));
            y += 16;

            card.Controls.Add(UIHelper.MakeCapsLabel("MEMBER").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            cboMember = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontBody,
                                       Size = new Size(390, 28), Location = new Point(20, y) };
            card.Controls.Add(cboMember);
            y += 40;

            card.Controls.Add(UIHelper.MakeCapsLabel("BOOK TO RESERVE").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            cboBook = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontBody,
                                     Size = new Size(390, 28), Location = new Point(20, y) };
            card.Controls.Add(cboBook);
            y += 40;

            card.Controls.Add(UIHelper.MakeCapsLabel("EXPIRY DATE").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            dtpExpiry = new DateTimePicker { Font = UIHelper.FontBody, Size = new Size(390, 28),
                                             Location = new Point(20, y), Format = DateTimePickerFormat.Short,
                                             Value = DateTime.Today.AddDays(7) };
            card.Controls.Add(dtpExpiry);
            y += 44;

            card.Controls.Add(UIHelper.MakeCapsLabel("NOTES").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            txtNotes = new TextBox { Font = UIHelper.FontBody, Size = new Size(390, 54),
                                     Location = new Point(20, y), Multiline = true };
            card.Controls.Add(txtNotes);
            y += 66;

            lblStatus = new Label { Font = UIHelper.FontBody, ForeColor = UIHelper.Green,
                                    Size = new Size(390, 22), Location = new Point(20, y), AutoSize = false };
            card.Controls.Add(lblStatus);
            y += 26;

            var btnSave = UIHelper.MakePrimaryBtn("✔  Save Reservation", new Size(200, 40));
            btnSave.Location = new Point(20, y);
            btnSave.Click += BtnSave_Click;
            card.Controls.Add(btnSave);

            var btnClear = UIHelper.MakeOutlineBtn("✖  Clear", new Size(90, 40));
            btnClear.Location = new Point(232, y);
            btnClear.Click += BtnClear_Click;
            card.Controls.Add(btnClear);

            // Grid card
            var gridCard = UIHelper.MakeCard(560, 570);
            gridCard.Location = new Point(466, 90);
            Controls.Add(gridCard);

            gridCard.Controls.Add(UIHelper.MakeLabel("All Reservations", UIHelper.FontH2, UIHelper.Navy)
                                  .Tap(l => l.Location = new Point(16, 14)));

            // Action buttons above grid
            var btnFulfil = UIHelper.MakeTintBtn("✔ Fulfil", new Size(90, 30), Color.FromArgb(212, 237, 218), Color.FromArgb(21, 87, 36));
            btnFulfil.Location = new Point(320, 12);
            btnFulfil.Click += (s, e) => UpdateStatus("fulfilled");
            gridCard.Controls.Add(btnFulfil);

            var btnCancel = UIHelper.MakeTintBtn("✖ Cancel", new Size(90, 30), Color.FromArgb(248, 215, 218), UIHelper.Red);
            btnCancel.Location = new Point(416, 12);
            btnCancel.Click += (s, e) => UpdateStatus("cancelled");
            gridCard.Controls.Add(btnCancel);

            dgvReservations = UIHelper.MakeGrid();
            dgvReservations.Location = new Point(0, 50);
            dgvReservations.Size     = new Size(560, 520);
            dgvReservations.SelectionChanged += (s, e) => PopulateFormFromGrid();
            gridCard.Controls.Add(dgvReservations);
        }

        private void LoadCombos()
        {
            var db = DatabaseConnection.Instance;
            var membDt = db.FillDataTable("SELECT member_id, full_name FROM members WHERE status='active' ORDER BY full_name");
            cboMember.DataSource    = membDt;
            cboMember.DisplayMember = "full_name";
            cboMember.ValueMember   = "member_id";
            cboMember.SelectedIndex = -1;

            var bookDt = db.FillDataTable(
                @"SELECT b.book_id, CONCAT(b.title,' [',b.genre,']') AS label
                  FROM books b
                  WHERE b.book_id NOT IN (
                      SELECT book_id FROM borrowings WHERE status IN ('borrowed','overdue')
                  )
                  ORDER BY b.title");
            cboBook.DataSource    = bookDt;
            cboBook.DisplayMember = "label";
            cboBook.ValueMember   = "book_id";
            cboBook.SelectedIndex = -1;
        }

        private void LoadReservations()
        {
            var dt = DatabaseConnection.Instance.FillDataTable(
                @"SELECT r.reservation_id AS ID,
                         m.full_name      AS Member,
                         b.title          AS Book,
                         r.reserved_date  AS Reserved,
                         r.expiry_date    AS Expiry,
                         r.status         AS Status
                  FROM reservations r
                  JOIN members m ON m.member_id = r.member_id
                  JOIN books   b ON b.book_id   = r.book_id
                  ORDER BY r.reserved_date DESC");
            dgvReservations.DataSource = dt;
        }

        private void PopulateFormFromGrid()
        {
            if (dgvReservations.SelectedRows.Count == 0) return;
            var row = dgvReservations.SelectedRows[0];
            _editId = Convert.ToInt32(row.Cells["ID"].Value);
        }

        private void UpdateStatus(string status)
        {
            if (_editId < 0) { ShowMsg("Select a reservation first.", UIHelper.Red); return; }
            DatabaseConnection.Instance.ExecuteNonQuery(
                "UPDATE reservations SET status=@s WHERE reservation_id=@id",
                new MySqlParameter("@s",  status),
                new MySqlParameter("@id", _editId));
            ShowMsg($"Reservation marked as {status}.", UIHelper.Green);
            _editId = -1;
            LoadReservations();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cboMember.SelectedIndex < 0 || cboBook.SelectedIndex < 0)
            { ShowMsg("Please select member and book.", UIHelper.Red); return; }

            int memberId = (int)(cboMember.SelectedValue ?? 0);
            int bookId   = (int)(cboBook.SelectedValue ?? 0);

            // Prevent duplicate pending reservation
            var dup = DatabaseConnection.Instance.ExecuteScalar(
                "SELECT COUNT(*) FROM reservations WHERE member_id=@m AND book_id=@b AND status='pending'",
                new MySqlParameter("@m", memberId), new MySqlParameter("@b", bookId));
            if (Convert.ToInt32(dup) > 0)
            { ShowMsg("Member already has a pending reservation for this book.", UIHelper.Red); return; }

            DatabaseConnection.Instance.ExecuteNonQuery(
                @"INSERT INTO reservations (member_id, book_id, reserved_date, expiry_date, status, notes)
                  VALUES (@m, @b, @rd, @ex, 'pending', @n)",
                new MySqlParameter("@m",  memberId),
                new MySqlParameter("@b",  bookId),
                new MySqlParameter("@rd", DateTime.Today),
                new MySqlParameter("@ex", dtpExpiry.Value.Date),
                new MySqlParameter("@n",  txtNotes.Text.Trim()));

            ShowMsg("✔ Reservation saved!", UIHelper.Green);
            BtnClear_Click(null, EventArgs.Empty);
            LoadReservations();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        { cboMember.SelectedIndex = -1; cboBook.SelectedIndex = -1;
          dtpExpiry.Value = DateTime.Today.AddDays(7); txtNotes.Clear(); lblStatus.Text = ""; _editId = -1; }

        private void ShowMsg(string msg, Color c) { lblStatus.Text = msg; lblStatus.ForeColor = c; }
    }
}
