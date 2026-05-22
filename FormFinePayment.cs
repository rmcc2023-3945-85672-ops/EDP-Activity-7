// ============================================================
// FormFinePayment.cs — TRANSACTION 3: Fine Payment
// Records payment (cash / online / waived) for overdue fines.
// Updates the fines table and logs to fine_payments.
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
    public class FormFinePayment : Form
    {
        private DataGridView dgvUnpaid = null!, dgvHistory = null!;
        private ComboBox     cboMethod = null!;
        private TextBox      txtRemarks = null!;
        private Label        lblFineInfo = null!, lblStatus = null!;
        private Button       btnPay = null!;
        private int          _selectedFineId = -1;
        private decimal      _selectedAmount  = 0;
        private readonly int _librarianId;

        public FormFinePayment(int librarianId)
        {
            _librarianId  = librarianId;
            Text          = "Library IS — Fine Payment";
            Size          = new Size(1050, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = UIHelper.Cream;
            BuildUI();
            LoadUnpaidFines();
            LoadPaymentHistory();
        }

        private void BuildUI()
        {
            // Header
            var hdr = new Panel { Size = new Size(1050, 68), BackColor = UIHelper.White };
            hdr.Paint += (s, e) => e.Graphics.DrawLine(new Pen(UIHelper.Border), 0, 67, 1050, 67);
            Controls.Add(hdr);
            hdr.Controls.Add(UIHelper.MakeLabel("Fine Payment", UIHelper.FontTitle, UIHelper.Navy)
                             .Tap(l => l.Location = new Point(28, 14)));
            hdr.Controls.Add(UIHelper.MakeLabel("Home › Transactions › Fines", UIHelper.FontSmall, UIHelper.Muted)
                             .Tap(l => l.Location = new Point(30, 46)));

            // LEFT — Unpaid fines
            var leftCard = UIHelper.MakeCard(490, 360);
            leftCard.Location = new Point(28, 90);
            Controls.Add(leftCard);
            leftCard.Controls.Add(UIHelper.MakeLabel("Unpaid Fines", UIHelper.FontH2, UIHelper.Navy)
                                  .Tap(l => l.Location = new Point(16, 14)));
            leftCard.Controls.Add(UIHelper.MakeLabel("Click a row to select for payment", UIHelper.FontSmall, UIHelper.Muted)
                                  .Tap(l => l.Location = new Point(16, 40)));
            dgvUnpaid = UIHelper.MakeGrid();
            dgvUnpaid.Location = new Point(0, 58);
            dgvUnpaid.Size     = new Size(490, 302);
            dgvUnpaid.SelectionChanged += DgvUnpaid_SelectionChanged;
            leftCard.Controls.Add(dgvUnpaid);

            // LEFT — Payment form (below unpaid fines)
            var payCard = UIHelper.MakeCard(490, 240);
            payCard.Location = new Point(28, 462);
            Controls.Add(payCard);

            int y = 16;
            payCard.Controls.Add(UIHelper.MakeLabel("Record Payment", UIHelper.FontH2, UIHelper.Navy)
                                  .Tap(l => l.Location = new Point(20, y)));
            y += 36;

            lblFineInfo = UIHelper.MakeLabel("No fine selected.", UIHelper.FontBody, UIHelper.Muted);
            lblFineInfo.Location = new Point(20, y);
            lblFineInfo.AutoSize = false;
            lblFineInfo.Size     = new Size(450, 24);
            payCard.Controls.Add(lblFineInfo);
            y += 32;

            payCard.Controls.Add(UIHelper.MakeCapsLabel("PAYMENT METHOD").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            cboMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontBody,
                                       Size = new Size(220, 28), Location = new Point(20, y) };
            cboMethod.Items.AddRange(new[] { "cash", "online", "waived" });
            cboMethod.SelectedIndex = 0;
            payCard.Controls.Add(cboMethod);
            y += 40;

            payCard.Controls.Add(UIHelper.MakeCapsLabel("REMARKS").Tap(l => l.Location = new Point(20, y)));
            y += 20;
            txtRemarks = new TextBox { Font = UIHelper.FontBody, Size = new Size(450, 24),
                                       Location = new Point(20, y) };
            payCard.Controls.Add(txtRemarks);
            y += 36;

            lblStatus = new Label { Font = UIHelper.FontBody, ForeColor = UIHelper.Green,
                                    Size = new Size(300, 22), Location = new Point(20, y), AutoSize = false };
            payCard.Controls.Add(lblStatus);

            btnPay = UIHelper.MakePrimaryBtn("💳  Record Payment", new Size(200, 40));
            btnPay.Location = new Point(270, y - 4);
            btnPay.Click += BtnPay_Click;
            btnPay.Enabled = false;
            payCard.Controls.Add(btnPay);

            // RIGHT — Payment history
            var rightCard = UIHelper.MakeCard(508, 610);
            rightCard.Location = new Point(530, 90);
            Controls.Add(rightCard);
            rightCard.Controls.Add(UIHelper.MakeLabel("Payment History", UIHelper.FontH2, UIHelper.Navy)
                                   .Tap(l => l.Location = new Point(16, 14)));
            dgvHistory = UIHelper.MakeGrid();
            dgvHistory.Location = new Point(0, 50);
            dgvHistory.Size     = new Size(508, 560);
            rightCard.Controls.Add(dgvHistory);
        }

        private void LoadUnpaidFines()
        {
            var dt = DatabaseConnection.Instance.FillDataTable(
                @"SELECT f.fine_id        AS ID,
                         m.full_name      AS Member,
                         b.title          AS Book,
                         br.return_date   AS Due,
                         f.amount         AS 'Amount (₱)'
                  FROM fines f
                  JOIN borrowings br ON br.borrow_id = f.borrow_id
                  JOIN members    m  ON m.member_id  = br.member_id
                  JOIN books      b  ON b.book_id    = br.book_id
                  WHERE f.paid = 'no'
                  ORDER BY f.amount DESC");
            dgvUnpaid.DataSource = dt;
        }

        private void LoadPaymentHistory()
        {
            var dt = DatabaseConnection.Instance.FillDataTable(
                @"SELECT fp.payment_id       AS ID,
                         m.full_name         AS Member,
                         b.title             AS Book,
                         fp.amount_paid      AS 'Paid (₱)',
                         fp.payment_date     AS Date,
                         fp.payment_method   AS Method,
                         r.full_name         AS 'Received By'
                  FROM fine_payments fp
                  JOIN fines      f  ON f.fine_id    = fp.fine_id
                  JOIN borrowings br ON br.borrow_id = f.borrow_id
                  JOIN members    m  ON m.member_id  = br.member_id
                  JOIN books      b  ON b.book_id    = br.book_id
                  JOIN members    r  ON r.member_id  = fp.received_by
                  ORDER BY fp.payment_date DESC");
            dgvHistory.DataSource = dt;
        }

        private void DgvUnpaid_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvUnpaid.SelectedRows.Count == 0) { ResetSelection(); return; }
            var row = dgvUnpaid.SelectedRows[0];
            _selectedFineId = Convert.ToInt32(row.Cells["ID"].Value);
            _selectedAmount = Convert.ToDecimal(row.Cells["Amount (₱)"].Value);
            lblFineInfo.Text     = $"Fine #{_selectedFineId} — {row.Cells["Member"].Value} | {row.Cells["Book"].Value} | ₱{_selectedAmount:F2}";
            lblFineInfo.ForeColor = UIHelper.Navy;
            btnPay.Enabled       = true;
        }

        private void ResetSelection()
        { _selectedFineId = -1; _selectedAmount = 0;
          lblFineInfo.Text = "No fine selected."; lblFineInfo.ForeColor = UIHelper.Muted; btnPay.Enabled = false; }

        private void BtnPay_Click(object? sender, EventArgs e)
        {
            if (_selectedFineId < 0) { ShowMsg("Select an unpaid fine first.", UIHelper.Red); return; }

            using var conn = DatabaseConnection.Instance.GetConnection();
            using var trx  = conn.BeginTransaction();
            try
            {
                // Insert payment record
                using var cmd1 = new MySqlCommand(
                    @"INSERT INTO fine_payments (fine_id, amount_paid, payment_date, received_by, payment_method, remarks)
                      VALUES (@fid, @amt, @dt, @rcv, @mth, @rem)", conn, trx);
                cmd1.Parameters.AddWithValue("@fid", _selectedFineId);
                cmd1.Parameters.AddWithValue("@amt", _selectedAmount);
                cmd1.Parameters.AddWithValue("@dt",  DateTime.Today);
                cmd1.Parameters.AddWithValue("@rcv", _librarianId);
                cmd1.Parameters.AddWithValue("@mth", cboMethod.SelectedItem!.ToString());
                cmd1.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());
                cmd1.ExecuteNonQuery();

                // Mark fine as paid
                using var cmd2 = new MySqlCommand("UPDATE fines SET paid='yes' WHERE fine_id=@fid", conn, trx);
                cmd2.Parameters.AddWithValue("@fid", _selectedFineId);
                cmd2.ExecuteNonQuery();

                // Return the book — close the borrowing linked to this fine
                using var cmd3 = new MySqlCommand(
                    @"UPDATE borrowings SET status='returned', actual_return_date=@dt
                      WHERE borrow_id = (SELECT borrow_id FROM fines WHERE fine_id=@fid)",
                    conn, trx);
                cmd3.Parameters.AddWithValue("@dt",  DateTime.Today);
                cmd3.Parameters.AddWithValue("@fid", _selectedFineId);
                cmd3.ExecuteNonQuery();

                trx.Commit();
                ShowMsg("✔ Payment recorded successfully!", UIHelper.Green);
                txtRemarks.Clear();
                ResetSelection();
                LoadUnpaidFines();
                LoadPaymentHistory();
            }
            catch (Exception ex)
            {
                trx.Rollback();
                ShowMsg("Error: " + ex.Message, UIHelper.Red);
            }
        }

        private void ShowMsg(string msg, Color c) { lblStatus.Text = msg; lblStatus.ForeColor = c; }
    }
}
