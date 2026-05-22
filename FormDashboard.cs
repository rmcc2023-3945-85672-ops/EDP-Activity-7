// ============================================================
// FormDashboard.cs — MAIN DASHBOARD
// UI inspired by LibraryIS_App.html design system
// Navy sidebar | Cream content | Stat cards | Data grids
// ============================================================

using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using LibrarySystem.Database;
using LibrarySystem.Forms;
using LibrarySystem.UI;

namespace LibrarySystem
{
    public class FormDashboard : Form
    {
        private readonly int    _memberId;
        private readonly string _memberName;
        private readonly string _memberRole;

        // Sidebar nav buttons
        private Button btnNavDash = null!, btnNavUsers = null!, btnNavBooks = null!,
                       btnNavBorrow = null!, btnNavFines = null!, btnNavAbout = null!,
                       btnSignOut = null!;

        // Content panels
        private Panel pnlDash = null!, pnlUsers = null!, pnlBooks = null!,
                      pnlBorrow = null!, pnlFines = null!, pnlAbout = null!;

        // Stat labels
        private Label lblStatMembers = null!, lblStatBooks = null!,
                      lblStatBorrowed = null!, lblStatOverdue = null!;

        // Grids
        private DataGridView dgvRecent = null!, dgvUsers = null!, dgvBooks = null!,
                             dgvBorrow = null!, dgvFines = null!;

        // Content offset (sidebar width)
        private const int SideW = 260;

        public FormDashboard(int memberId, string memberName, string memberRole)
        {
            _memberId   = memberId;
            _memberName = memberName;
            _memberRole = memberRole;

            this.Text          = "Library Information System";
            this.Size          = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState   = FormWindowState.Maximized;
            this.BackColor     = UIHelper.Cream;

            BuildSidebar();
            BuildPanels();
            ShowPanel("dashboard");
        }

        // ══════════════════════════════════════════════════════
        // SIDEBAR
        // ══════════════════════════════════════════════════════
        private void BuildSidebar()
        {
            var sidebar = new Panel
            {
                Size      = new Size(SideW, 2000),
                Location  = new Point(0, 0),
                BackColor = UIHelper.Navy,
            };
            this.Controls.Add(sidebar);
            sidebar.BringToFront();

            // Brand
            var brandIcon = new Label
            {
                Text      = "📚",
                Font      = new Font("Segoe UI", 18),
                ForeColor = UIHelper.Navy,
                BackColor = UIHelper.Gold,
                Size      = new Size(46, 46),
                Location  = new Point(24, 22),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            sidebar.Controls.Add(brandIcon);

            var ctrl1 = UIHelper.MakeLabel("Library IS", new Font("Segoe UI", 14, FontStyle.Bold), UIHelper.White);


            ctrl1.Location = new Point(80, 24);


            ctrl1.BackColor = Color.Transparent;


            sidebar.Controls.Add(ctrl1);
            var ctrl2 = UIHelper.MakeLabel("v1.0.0", UIHelper.FontSmall, Color.FromArgb(60, 90, 120));

            ctrl2.Location = new Point(82, 48);

            ctrl2.BackColor = Color.Transparent;

            sidebar.Controls.Add(ctrl2);

            var ctrl3 = UIHelper.MakeDivider(210);


            ctrl3.Location = new Point(24, 76);


            sidebar.Controls.Add(ctrl3);

            var ctrl4 = UIHelper.MakeLabel("MAIN MENU", UIHelper.FontCaps, Color.FromArgb(60, 90, 120));


            ctrl4.Location = new Point(24, 92);


            ctrl4.BackColor = Color.Transparent;


            sidebar.Controls.Add(ctrl4);

            btnNavDash   = AddNavBtn(sidebar, "🏠  Dashboard",   new Point(0, 116));
            btnNavUsers  = AddNavBtn(sidebar, "👥  Members",     new Point(0, 156));
            btnNavBooks  = AddNavBtn(sidebar, "📖  Books",       new Point(0, 196));
            btnNavBorrow = AddNavBtn(sidebar, "🔄  Borrowings",  new Point(0, 236));
            btnNavFines  = AddNavBtn(sidebar, "💰  Fines",       new Point(0, 276));

            // ── ACTIVITY 6 NAV BUTTONS ────────────────────────
            var divTx = UIHelper.MakeDivider(210);
            divTx.Location = new Point(24, 316);
            sidebar.Controls.Add(divTx);

            var lblTx = UIHelper.MakeLabel("TRANSACTIONS", UIHelper.FontCaps, Color.FromArgb(60, 90, 120));
            lblTx.Location  = new Point(24, 328);
            lblTx.BackColor = Color.Transparent;
            sidebar.Controls.Add(lblTx);

            var btnNavIssuebook = AddNavBtn(sidebar, "📤  Issue Book",   new Point(0, 346));
            var btnNavReserve   = AddNavBtn(sidebar, "🔖  Reservations", new Point(0, 386));
            var btnNavFinePay   = AddNavBtn(sidebar, "💳  Fine Payment", new Point(0, 426));
            var btnNavReports   = AddNavBtn(sidebar, "📊  Reports",      new Point(0, 466));

            btnNavIssuebook.Click += (s, e) => new Forms.FormBorrowBook().ShowDialog();
            btnNavReserve.Click   += (s, e) => new Forms.FormBookReservation().ShowDialog();
            btnNavFinePay.Click   += (s, e) => new Forms.FormFinePayment(_memberId).ShowDialog();
            btnNavReports.Click   += (s, e) => new Forms.FormReports().ShowDialog();

            var ctrl5 = UIHelper.MakeDivider(210);


            ctrl5.Location = new Point(24, 506);


            sidebar.Controls.Add(ctrl5);
            var ctrl6 = UIHelper.MakeLabel("SETTINGS", UIHelper.FontCaps, Color.FromArgb(60, 90, 120));

            ctrl6.Location = new Point(24, 518);

            ctrl6.BackColor = Color.Transparent;

            sidebar.Controls.Add(ctrl6);

            btnNavAbout = AddNavBtn(sidebar, "ℹ️  About",    new Point(0, 546));
            btnSignOut  = AddNavBtn(sidebar, "🚪  Sign Out", new Point(0, 586));

            btnNavDash.Click   += (s,e) => ShowPanel("dashboard");
            btnNavUsers.Click  += (s,e) => ShowPanel("users");
            btnNavBooks.Click  += (s,e) => ShowPanel("books");
            btnNavBorrow.Click += (s,e) => ShowPanel("borrowings");
            btnNavFines.Click  += (s,e) => ShowPanel("fines");
            btnNavAbout.Click  += (s,e) => ShowPanel("about");
            btnSignOut.Click   += (s,e) =>
            {
                new Forms.Form1().Show();
                this.Close();
            };

            // User chip at bottom
            var ctrl7 = UIHelper.MakeDivider(210);

            ctrl7.Location = new Point(24, 630);

            sidebar.Controls.Add(ctrl7);

            string initials = _memberName.Length > 0 ? _memberName.Substring(0, 1).ToUpper() : "?";
            var avatar = new Label
            {
                Text      = initials,
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UIHelper.Navy,
                BackColor = UIHelper.Gold,
                Size      = new Size(40, 40),
                Location  = new Point(22, 646),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            sidebar.Controls.Add(avatar);
            var ctrl8 = UIHelper.MakeLabel(_memberName, UIHelper.FontBold, UIHelper.White);

            ctrl8.Location = new Point(70, 646);

            ctrl8.BackColor = Color.Transparent;

            sidebar.Controls.Add(ctrl8);
            var ctrl9 = UIHelper.MakeLabel(_memberRole, UIHelper.FontSmall, Color.FromArgb(80, 110, 140));

            ctrl9.Location = new Point(71, 668);

            ctrl9.BackColor = Color.Transparent;

            sidebar.Controls.Add(ctrl9);
        }

        private Button AddNavBtn(Panel parent, string text, Point loc)
        {
            var b = new Button
            {
                Text      = text,
                Font      = UIHelper.FontNav,
                Size      = new Size(SideW, 38),
                Location  = loc,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(140, 165, 190),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(20, 0, 0, 0),
                Cursor    = Cursors.Hand,
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, 38, 58);
            parent.Controls.Add(b);
            return b;
        }

        // ══════════════════════════════════════════════════════
        // BUILD ALL CONTENT PANELS
        // ══════════════════════════════════════════════════════
        private void BuildPanels()
        {
            // ── Dashboard ──────────────────────────────────────────
            pnlDash = MakeContentPanel();
            AddPageHeader(pnlDash, "Dashboard Overview", "Home › Dashboard");

            // Stat cards row
            lblStatMembers  = AddStatCard(pnlDash, "Total Members",     "—", UIHelper.Blue,  new Point(28,  90));
            lblStatBooks    = AddStatCard(pnlDash, "Total Books",        "—", UIHelper.Green, new Point(260, 90));
            lblStatBorrowed = AddStatCard(pnlDash, "Currently Borrowed", "—", UIHelper.Gold,  new Point(492, 90));
            lblStatOverdue  = AddStatCard(pnlDash, "Overdue",            "—", UIHelper.Red,   new Point(724, 90));

            // Recent members card
            var recentCard = AddCard(pnlDash, "Recently Registered Members", new Point(28, 218), new Size(900, 330));
            dgvRecent = UIHelper.MakeGrid();
            dgvRecent.Size = new Size(868, 280);

            dgvRecent.Location = new Point(16, 42);
            recentCard.Controls.Add(dgvRecent);

            // ── Users ──────────────────────────────────────────────
            pnlUsers = MakeContentPanel();
            AddPageHeader(pnlUsers, "Member Management", "Home › Members");

            var btnOpenMgmt = UIHelper.MakePrimaryBtn("⚙  Open User Management", new Size(260, 44));
            btnOpenMgmt.Location = new Point(28, 90);
            btnOpenMgmt.Click   += (s,e) => { new FormUserManagement().ShowDialog(this); LoadDashboard(); };
            btnOpenMgmt.Visible  = (_memberRole == "admin");
            pnlUsers.Controls.Add(btnOpenMgmt);

            var usersCard = AddCard(pnlUsers, "All Members", new Point(28, 154), new Size(900, 420));
            dgvUsers = UIHelper.MakeGrid();
            dgvUsers.Size = new Size(868, 374);

            dgvUsers.Location = new Point(16, 38);
            usersCard.Controls.Add(dgvUsers);

            // ── Books ──────────────────────────────────────────────
            pnlBooks = MakeContentPanel();
            AddPageHeader(pnlBooks, "Books", "Home › Books");
            var booksCard = AddCard(pnlBooks, "Book Catalogue", new Point(28, 90), new Size(900, 490));
            dgvBooks = UIHelper.MakeGrid();
            dgvBooks.Size = new Size(868, 444);

            dgvBooks.Location = new Point(16, 38);
            booksCard.Controls.Add(dgvBooks);

            // ── Borrowings ─────────────────────────────────────────
            pnlBorrow = MakeContentPanel();
            AddPageHeader(pnlBorrow, "Borrowings", "Home › Borrowings");
            var borrowCard = AddCard(pnlBorrow, "Borrowing Records", new Point(28, 90), new Size(900, 490));
            dgvBorrow = UIHelper.MakeGrid();
            dgvBorrow.Size = new Size(868, 444);

            dgvBorrow.Location = new Point(16, 38);
            borrowCard.Controls.Add(dgvBorrow);

            // ── Fines ──────────────────────────────────────────────
            pnlFines = MakeContentPanel();
            AddPageHeader(pnlFines, "Fines", "Home › Fines");
            var finesCard = AddCard(pnlFines, "Fine Summary", new Point(28, 90), new Size(900, 490));
            dgvFines = UIHelper.MakeGrid();
            dgvFines.Size = new Size(868, 444);

            dgvFines.Location = new Point(16, 38);
            finesCard.Controls.Add(dgvFines);

            // ── About ──────────────────────────────────────────────
            pnlAbout = MakeContentPanel();
            AddPageHeader(pnlAbout, "About the Program", "Home › About");
            BuildAboutPanel();

            this.Controls.AddRange(new Control[] { pnlDash, pnlUsers, pnlBooks, pnlBorrow, pnlFines, pnlAbout });
        }

        private Panel MakeContentPanel()
            => new Panel
            {
                Size      = new Size(this.Width - SideW, 2000),
                Location  = new Point(SideW, 0),
                BackColor = UIHelper.Cream,
                Visible   = false,
                AutoScroll = true,
            };

        private void AddPageHeader(Panel p, string title, string crumb)
        {
            var ctrl10 = UIHelper.MakeLabel(title, UIHelper.FontTitle, UIHelper.Navy);

            ctrl10.Location = new Point(28, 28);

            p.Controls.Add(ctrl10);
            var ctrl11 = UIHelper.MakeLabel(crumb, UIHelper.FontSmall, UIHelper.Muted);

            ctrl11.Location = new Point(30, 62);

            p.Controls.Add(ctrl11);
            var ctrl12 = UIHelper.MakeDivider(900);

            ctrl12.Location = new Point(28, 78);

            p.Controls.Add(ctrl12);
        }

        private Label AddStatCard(Panel parent, string key, string val, Color accent, Point loc)
        {
            var card = new Panel { Size = new Size(220, 100), Location = loc, BackColor = UIHelper.White };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path  = UIHelper.RoundRect(new Rectangle(0, 0, card.Width-1, card.Height-1), 12);
                g.FillPath(Brushes.White, path);
                g.DrawPath(new Pen(UIHelper.Border), path);
                // Left accent bar
                g.FillRectangle(new SolidBrush(accent), 0, 0, 5, card.Height);
            };

            var lblVal = UIHelper.MakeLabel(val, new Font("Segoe UI", 26, FontStyle.Bold), UIHelper.Navy, autoSize: true);
            lblVal.Location = new Point(18, 14);
            card.Controls.Add(lblVal);
            var ctrl13 = UIHelper.MakeLabel(key, UIHelper.FontSmall, UIHelper.Muted, autoSize: false);
            ctrl13.Size = new Size(196, 18);
            ctrl13.Location = new Point(18, 66);

            card.Controls.Add(ctrl13);
            parent.Controls.Add(card);
            return lblVal;
        }

        private Panel AddCard(Panel parent, string title, Point loc, Size size)
        {
            var card = new Panel { Size = size, Location = loc, BackColor = UIHelper.White };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = UIHelper.RoundRect(new Rectangle(0, 0, card.Width-1, card.Height-1), 12);
                e.Graphics.DrawPath(new Pen(UIHelper.Border), path);
            };
            var ctrl14 = UIHelper.MakeLabel(title, UIHelper.FontH2, UIHelper.Navy);

            ctrl14.Location = new Point(16, 12);

            card.Controls.Add(ctrl14);
            parent.Controls.Add(card);
            return card;
        }

        private void BuildAboutPanel()
        {
            var card = AddCard(pnlAbout, "Library IS — Version 1.0.0", new Point(28, 90), new Size(900, 440));

            card.Controls.Add(new Label { Text="📚", Font=new Font("Segoe UI",36), Location=new Point(22,50), AutoSize=true });
            var ctrl15 = UIHelper.MakeLabel("Library Information System v1.0.0", new Font("Segoe UI",14,FontStyle.Bold), UIHelper.Navy);

            ctrl15.Location = new Point(88, 56);

            card.Controls.Add(ctrl15);
            var ctrl16 = UIHelper.MakeLabel("A desktop application built with C# Windows Forms and MySQL.", UIHelper.FontBody, UIHelper.Muted);

            ctrl16.Location = new Point(88, 84);

            card.Controls.Add(ctrl16);

            var ctrl17 = UIHelper.MakeDivider(860);


            ctrl17.Location = new Point(20, 130);


            card.Controls.Add(ctrl17);

            var ctrl18 = UIHelper.MakeLabel("Purpose", UIHelper.FontBold, UIHelper.Navy);


            ctrl18.Location = new Point(22, 148);


            card.Controls.Add(ctrl18);
            card.Controls.Add(new Label { Text = "Centralize all library member data, reduce manual paperwork,\nand improve decision-making through real-time reporting.", Font = UIHelper.FontBody, ForeColor = UIHelper.Muted, Location = new Point(22, 170), AutoSize = true });

            var ctrl19 = UIHelper.MakeLabel("Technology Stack", UIHelper.FontBold, UIHelper.Navy);


            ctrl19.Location = new Point(22, 226);


            card.Controls.Add(ctrl19);
            var ctrl20 = UIHelper.MakeLabel("C# Windows Forms  ·  MySQL 8.0  ·  MySql.Data NuGet  ·  .NET 6+  ·  SHA-256 Auth", UIHelper.FontBody, UIHelper.Muted);

            ctrl20.Location = new Point(22, 248);

            card.Controls.Add(ctrl20);

            var ctrl21 = UIHelper.MakeLabel("Features", UIHelper.FontBold, UIHelper.Navy);


            ctrl21.Location = new Point(22, 290);


            card.Controls.Add(ctrl21);
            var ctrl22 = UIHelper.MakeLabel("User Authentication  ·  Password Recovery (OTP)  ·  Add / Update / Activate / Deactivate / Search Members", UIHelper.FontBody, UIHelper.Muted);

            ctrl22.Location = new Point(22, 312);

            card.Controls.Add(ctrl22);
            var ctrl23 = UIHelper.MakeLabel("Book Catalogue  ·  Borrowing Records  ·  Fine Summary  ·  Role-based Access", UIHelper.FontBody, UIHelper.Muted);

            ctrl23.Location = new Point(22, 334);

            card.Controls.Add(ctrl23);

            var ctrl24 = UIHelper.MakeDivider(860);


            ctrl24.Location = new Point(20, 374);


            card.Controls.Add(ctrl24);
            var ctrl25 = UIHelper.MakeLabel($"Logged in as: {_memberName}  ({_memberRole})", UIHelper.FontSmall, UIHelper.Muted);

            ctrl25.Location = new Point(22, 392);

            card.Controls.Add(ctrl25);
        }

        // ══════════════════════════════════════════════════════
        // NAVIGATION
        // ══════════════════════════════════════════════════════
        private void ShowPanel(string name)
        {
            pnlDash.Visible   = name == "dashboard";
            pnlUsers.Visible  = name == "users";
            pnlBooks.Visible  = name == "books";
            pnlBorrow.Visible = name == "borrowings";
            pnlFines.Visible  = name == "fines";
            pnlAbout.Visible  = name == "about";

            // Reset all nav button styles
            foreach (var b in new[] { btnNavDash, btnNavUsers, btnNavBooks, btnNavBorrow, btnNavFines, btnNavAbout, btnSignOut })
            {
                b.ForeColor = Color.FromArgb(140, 165, 190);
                b.BackColor = Color.Transparent;
            }

            // Highlight active
            var active = name switch
            {
                "dashboard"  => btnNavDash,
                "users"      => btnNavUsers,
                "books"      => btnNavBooks,
                "borrowings" => btnNavBorrow,
                "fines"      => btnNavFines,
                _            => btnNavAbout,
            };
            active.ForeColor = UIHelper.White;
            active.BackColor = Color.FromArgb(35, 232, 169, 35);   // gold tint

            // Load data for the panel
            if (name == "dashboard")  LoadDashboard();
            if (name == "users")      LoadUsers();
            if (name == "books")      LoadBooks();
            if (name == "borrowings") LoadBorrowings();
            if (name == "fines")      LoadFines();
        }

        // ══════════════════════════════════════════════════════
        // DATA LOADERS
        // ══════════════════════════════════════════════════════
        private void LoadDashboard()
        {
            try
            {
                var db = DatabaseConnection.Instance;
                lblStatMembers.Text  = db.ExecuteScalar("SELECT COUNT(*) FROM members")?.ToString() ?? "—";
                lblStatBooks.Text    = db.ExecuteScalar("SELECT COUNT(*) FROM books")?.ToString() ?? "—";
                lblStatBorrowed.Text = db.ExecuteScalar("SELECT COUNT(*) FROM borrowings WHERE status='borrowed'")?.ToString() ?? "—";
                lblStatOverdue.Text  = db.ExecuteScalar("SELECT COUNT(*) FROM borrowings WHERE status='overdue'")?.ToString() ?? "—";

                dgvRecent.DataSource = db.FillDataTable(
                    "SELECT member_id AS 'ID', full_name AS 'Name', email AS 'Email', status AS 'Status', membership_date AS 'Date' FROM members ORDER BY member_id DESC LIMIT 8");
            }
            catch (Exception ex)
            {
                lblStatMembers.Text = "—";
                MessageBox.Show("DB Error: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadUsers()
        {
            try
            {
                dgvUsers.DataSource = DatabaseConnection.Instance.FillDataTable(
                    "SELECT member_id AS 'ID', full_name AS 'Full Name', email AS 'Email', role AS 'Role', membership_date AS 'Since', status AS 'Status' FROM members ORDER BY member_id DESC");
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void LoadBooks()
        {
            try
            {
                dgvBooks.DataSource = DatabaseConnection.Instance.FillDataTable(
                    "SELECT b.book_id AS 'ID', b.title AS 'Title', a.author_name AS 'Author', b.genre AS 'Genre' FROM books b JOIN authors a ON b.author_id=a.author_id ORDER BY b.book_id");
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void LoadBorrowings()
        {
            try { dgvBorrow.DataSource = DatabaseConnection.Instance.FillDataTable("SELECT * FROM view_borrowed_books"); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void LoadFines()
        {
            try { dgvFines.DataSource = DatabaseConnection.Instance.FillDataTable("SELECT * FROM view_fine_summary"); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}
