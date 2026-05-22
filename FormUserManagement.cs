// ============================================================
// FormUserManagement.cs — USER MANAGEMENT
// UI inspired by LibraryIS_App.html design system
// Add, Update, Activate, Deactivate, Search, List
// ============================================================

using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using LibrarySystem.Database;
using LibrarySystem.Utils;
using LibrarySystem.UI;

namespace LibrarySystem.Forms
{
    public class FormUserManagement : Form
    {
        private int    _editingId   = -1;
        private string _searchQuery = "";

        // Controls
        private DataGridView dgvMembers = null!;
        private TextBox      txtSearch = null!, txtFullName = null!,
                             txtEmail = null!, txtPassword = null!;
        private ComboBox     cboRole = null!, cboStatus = null!;
        private Button       btnAdd = null!, btnEdit = null!,
                             btnActivate = null!, btnDeactivate = null!,
                             btnSave = null!, btnCancel = null!;
        private Label        lblTotal = null!, lblFormTitle = null!;
        private Panel        pnlForm = null!;

        public FormUserManagement()
        {
            this.Text            = "Library IS — User Management";
            this.Size            = new Size(1020, 720);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = UIHelper.Cream;
            BuildUI();
            LoadMembers();
        }

        private void BuildUI()
        {
            // ── Page header bar ──────────────────────────────────────
            var header = new Panel { Size = new Size(1020, 70), Location = new Point(0,0), BackColor = UIHelper.White };
            header.Paint += (s,e) => e.Graphics.DrawLine(new Pen(UIHelper.Border), 0, 69, 1020, 69);
            this.Controls.Add(header);

            var ctrl1 = UIHelper.MakeLabel("User Management", UIHelper.FontTitle, UIHelper.Navy);


            ctrl1.Location = new Point(32, 14);


            header.Controls.Add(ctrl1);
            var ctrl2 = UIHelper.MakeLabel("Home › Members", UIHelper.FontSmall, UIHelper.Muted);

            ctrl2.Location = new Point(34, 44);

            header.Controls.Add(ctrl2);

            // ── Main card ────────────────────────────────────────────
            var card = UIHelper.MakeCard(960, 600);
            card.Location = new Point(30, 88);
            this.Controls.Add(card);

            // ── Toolbar row ──────────────────────────────────────────
            txtSearch = UIHelper.MakeInput("🔍  Search members...");
            txtSearch.Size = new Size(280, 32);

            txtSearch.Location = new Point(18, 16);
            txtSearch.TextChanged += (s,e) => { _searchQuery = txtSearch.Text.Trim(); LoadMembers(_searchQuery); };
            card.Controls.Add(txtSearch);

            lblTotal = UIHelper.MakeLabel("Total: 0 members", UIHelper.FontSmall, UIHelper.Muted);
            lblTotal.Location = new Point(310, 22);
            card.Controls.Add(lblTotal);

            btnAdd = UIHelper.MakePrimaryBtn("＋  Add Account", new Size(148, 34));
            btnAdd.Location = new Point(800, 14);
            btnAdd.Click   += (s,e) => OpenAddForm();
            card.Controls.Add(btnAdd);

            // ── Action buttons ───────────────────────────────────────
            btnEdit = UIHelper.MakeTintBtn("✏  Edit", new Size(96, 32),
                Color.FromArgb(230, 240, 255), Color.FromArgb(26, 82, 166));
            btnEdit.Location = new Point(18, 58);
            btnEdit.Click   += (s,e) => OpenEditForm();
            card.Controls.Add(btnEdit);

            btnActivate = UIHelper.MakeTintBtn("✅  Activate", new Size(114, 32),
                Color.FromArgb(234, 250, 241), Color.FromArgb(30, 132, 73));
            btnActivate.Location = new Point(124, 58);
            btnActivate.Click   += (s,e) => SetStatus("active");
            card.Controls.Add(btnActivate);

            btnDeactivate = UIHelper.MakeTintBtn("🔴  Deactivate", new Size(124, 32),
                Color.FromArgb(253, 242, 242), Color.FromArgb(192, 57, 43));
            btnDeactivate.Location = new Point(248, 58);
            btnDeactivate.Click   += (s,e) => SetStatus("inactive");
            card.Controls.Add(btnDeactivate);

            // ── Data grid ────────────────────────────────────────────
            dgvMembers = UIHelper.MakeGrid();
            dgvMembers.Size = new Size(920, 390);

            dgvMembers.Location = new Point(18, 102);
            // Alternating row color
            dgvMembers.RowsDefaultCellStyle.BackColor          = UIHelper.White;
            dgvMembers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 248, 244);
            card.Controls.Add(dgvMembers);

            // ── Slide-in Add/Edit form panel ─────────────────────────
            pnlForm = new Panel
            {
                Size      = new Size(480, 600),
                Location  = new Point(540, 70),
                BackColor = UIHelper.White,
                Visible   = false,
            };
            pnlForm.Paint += (s,e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = UIHelper.RoundRect(new Rectangle(0,0,pnlForm.Width-1,pnlForm.Height-1), 14);
                e.Graphics.DrawPath(new Pen(UIHelper.Border), path);
            };
            this.Controls.Add(pnlForm);
            pnlForm.BringToFront();

            lblFormTitle = UIHelper.MakeLabel("Add New Account", UIHelper.FontTitle, UIHelper.Navy);
            lblFormTitle.Location = new Point(30, 24);
            pnlForm.Controls.Add(lblFormTitle);

            var ctrl3 = UIHelper.MakeDivider(420);


            ctrl3.Location = new Point(30, 62);


            pnlForm.Controls.Add(ctrl3);

            // Full name
            var ctrl4 = UIHelper.MakeCapsLabel("FULL NAME");

            ctrl4.Location = new Point(30, 78);

            pnlForm.Controls.Add(ctrl4);
            txtFullName = UIHelper.MakeInput("Juan Dela Cruz");
            txtFullName.Size = new Size(420, 36);

            txtFullName.Location = new Point(30, 96);
            pnlForm.Controls.Add(txtFullName);

            // Email
            var ctrl5 = UIHelper.MakeCapsLabel("EMAIL");

            ctrl5.Location = new Point(30, 146);

            pnlForm.Controls.Add(ctrl5);
            txtEmail = UIHelper.MakeInput("juan@email.com");
            txtEmail.Size = new Size(420, 36);

            txtEmail.Location = new Point(30, 164);
            pnlForm.Controls.Add(txtEmail);

            // Password
            var ctrl6 = UIHelper.MakeCapsLabel("PASSWORD");

            ctrl6.Location = new Point(30, 214);

            pnlForm.Controls.Add(ctrl6);
            txtPassword = UIHelper.MakeInput("Min. 6 characters", password: true);
            txtPassword.Size = new Size(420, 36);

            txtPassword.Location = new Point(30, 232);
            pnlForm.Controls.Add(txtPassword);

            // Role
            var ctrl7 = UIHelper.MakeCapsLabel("ROLE");

            ctrl7.Location = new Point(30, 282);

            pnlForm.Controls.Add(ctrl7);
            cboRole = new ComboBox { Font = UIHelper.FontInput, Size = new Size(420, 36), Location = new Point(30, 300), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRole.Items.AddRange(new[] { "member", "librarian", "admin" });
            cboRole.SelectedIndex = 0;
            pnlForm.Controls.Add(cboRole);

            // Status
            var ctrl8 = UIHelper.MakeCapsLabel("STATUS");

            ctrl8.Location = new Point(30, 348);

            pnlForm.Controls.Add(ctrl8);
            cboStatus = new ComboBox { Font = UIHelper.FontInput, Size = new Size(420, 36), Location = new Point(30, 366), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new[] { "active", "inactive" });
            cboStatus.SelectedIndex = 0;
            pnlForm.Controls.Add(cboStatus);

            var ctrl9 = UIHelper.MakeDivider(420);


            ctrl9.Location = new Point(30, 420);


            pnlForm.Controls.Add(ctrl9);

            // Buttons
            btnSave = UIHelper.MakeGoldBtn("💾  Save Account", new Size(200, 44));
            btnSave.Location = new Point(30, 438);
            btnSave.Click   += BtnSave_Click;
            pnlForm.Controls.Add(btnSave);

            btnCancel = UIHelper.MakeOutlineBtn("Cancel", new Size(204, 44));
            btnCancel.Location = new Point(246, 438);
            btnCancel.Click   += (s,e) => pnlForm.Visible = false;
            pnlForm.Controls.Add(btnCancel);
        }

        // ── Load / Search ────────────────────────────────────────────
        private void LoadMembers(string search = "")
        {
            try
            {
                string sql = @"SELECT member_id AS 'ID',
                                      full_name   AS 'Full Name',
                                      email       AS 'Email',
                                      role        AS 'Role',
                                      membership_date AS 'Member Since',
                                      status      AS 'Status'
                               FROM   members
                               WHERE  full_name LIKE @q OR email LIKE @q
                                  OR  role LIKE @q OR status LIKE @q
                               ORDER  BY member_id DESC";

                DataTable dt = DatabaseConnection.Instance.FillDataTable(sql,
                    new MySqlParameter("@q", $"%{search}%"));

                dgvMembers.DataSource = dt;
                lblTotal.Text = $"Total: {dt.Rows.Count} members";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading members:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Open add form ────────────────────────────────────────────
        private void OpenAddForm()
        {
            _editingId = -1;
            lblFormTitle.Text = "Add New Account";
            txtFullName.Clear(); txtEmail.Clear(); txtPassword.Clear();
            txtPassword.Enabled = true;
            cboRole.SelectedIndex   = 0;
            cboStatus.SelectedIndex = 0;
            pnlForm.Visible = true;
        }

        // ── Open edit form ───────────────────────────────────────────
        private void OpenEditForm()
        {
            if (dgvMembers.SelectedRows.Count == 0)
            { MessageBox.Show("Please select a member to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            DataRow row = ((DataRowView)dgvMembers.SelectedRows[0].DataBoundItem!).Row;
            _editingId = Convert.ToInt32(row["ID"]);

            lblFormTitle.Text       = "Update Account Profile";
            txtFullName.Text        = row["Full Name"].ToString();
            txtEmail.Text           = row["Email"].ToString();
            txtPassword.Text        = "";
            txtPassword.Enabled     = false;
            cboRole.Text            = row["Role"].ToString();
            cboStatus.Text          = row["Status"].ToString();
            pnlForm.Visible         = true;
        }

        // ── Save ─────────────────────────────────────────────────────
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string name   = txtFullName.Text.Trim();
            string email  = txtEmail.Text.Trim();
            string pw     = txtPassword.Text;
            string role   = cboRole.Text;
            string status = cboStatus.Text;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            { MessageBox.Show("Name and email are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                if (_editingId == -1)
                {
                    // ADD
                    if (pw.Length < 6)
                    { MessageBox.Show("Password must be at least 6 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                    int exists = Convert.ToInt32(DatabaseConnection.Instance.ExecuteScalar(
                        "SELECT COUNT(*) FROM members WHERE email=@e", new MySqlParameter("@e", email)));
                    if (exists > 0) { MessageBox.Show("Email already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                    DatabaseConnection.Instance.ExecuteNonQuery(
                        "INSERT INTO members (full_name,email,password,role,membership_date,status) VALUES(@n,@e,@p,@r,CURDATE(),@s)",
                        new MySqlParameter("@n", name),
                        new MySqlParameter("@e", email),
                        new MySqlParameter("@p", PasswordHelper.Hash(pw)),
                        new MySqlParameter("@r", role),
                        new MySqlParameter("@s", status));

                    MessageBox.Show($"Account for '{name}' created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // UPDATE
                    int exists = Convert.ToInt32(DatabaseConnection.Instance.ExecuteScalar(
                        "SELECT COUNT(*) FROM members WHERE email=@e AND member_id<>@id",
                        new MySqlParameter("@e", email), new MySqlParameter("@id", _editingId)));
                    if (exists > 0) { MessageBox.Show("Email already used by another account.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                    DatabaseConnection.Instance.ExecuteNonQuery(
                        "UPDATE members SET full_name=@n,email=@e,role=@r,status=@s WHERE member_id=@id",
                        new MySqlParameter("@n",  name),
                        new MySqlParameter("@e",  email),
                        new MySqlParameter("@r",  role),
                        new MySqlParameter("@s",  status),
                        new MySqlParameter("@id", _editingId));

                    MessageBox.Show($"Account for '{name}' updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                pnlForm.Visible = false;
                LoadMembers(_searchQuery);
            }
            catch (Exception ex)
            { MessageBox.Show("Error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ── Activate / Deactivate ────────────────────────────────────
        private void SetStatus(string newStatus)
        {
            if (dgvMembers.SelectedRows.Count == 0)
            { MessageBox.Show("Please select a member first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            DataRow row  = ((DataRowView)dgvMembers.SelectedRows[0].DataBoundItem!).Row;
            int     id   = Convert.ToInt32(row["ID"]);
            string  name = row["Full Name"].ToString() ?? "";

            if (MessageBox.Show($"Are you sure you want to set '{name}' to {newStatus}?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                DatabaseConnection.Instance.ExecuteNonQuery(
                    "UPDATE members SET status=@s WHERE member_id=@id",
                    new MySqlParameter("@s",  newStatus),
                    new MySqlParameter("@id", id));

                MessageBox.Show($"'{name}' is now {newStatus}.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMembers(_searchQuery);
            }
            catch (Exception ex)
            { MessageBox.Show("Error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
