// ============================================================
// Form1.cs — LOGIN FORM
// UI inspired by LibraryIS_App.html design system
// Two-column layout: Navy hero left | Cream form right
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
    public class Form1 : Form
    {
        private string _selectedRole = "member";

        // Role tabs
        private Panel pnlRoleMember, pnlRoleLibrarian, pnlRoleAdmin;

        // Inputs
        private TextBox txtEmail, txtPassword;
        private Button  btnSignIn;
        private LinkLabel lnkForgot;
        private Label   lblError;

        public Form1()
        {
            this.Text            = "Library Information System — Sign In";
            this.Size            = new Size(960, 620);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = UIHelper.Navy;

            BuildUI();
        }

        private void BuildUI()
        {
            // ── LEFT hero panel ───────────────────────────────────────
            var left = new Panel
            {
                Size      = new Size(520, 620),
                Location  = new Point(0, 0),
                BackColor = UIHelper.Navy,
            };
            left.Paint += PaintHeroLeft;
            this.Controls.Add(left);

            // Brand
            var brandIcon = new Label
            {
                Text      = "📚",
                Font      = new Font("Segoe UI", 20),
                ForeColor = UIHelper.Navy,
                BackColor = UIHelper.Gold,
                Size      = new Size(52, 52),
                Location  = new Point(52, 44),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            left.Controls.Add(brandIcon);

            left.Controls.Add(new Label
            {
                Text      = "Library IS",
                Font      = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = UIHelper.White,
                Location  = new Point(114, 40),
                AutoSize  = true,
                BackColor = Color.Transparent,
            });
            left.Controls.Add(new Label
            {
                Text      = "INFORMATION SYSTEM",
                Font      = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UIHelper.Gold,
                Location  = new Point(115, 78),
                AutoSize  = true,
                BackColor = Color.Transparent,
            });

            // Hero text
            left.Controls.Add(new Label
            {
                Text      = "Your Library,\nSmarter &\nConnected.",
                Font      = new Font("Segoe UI", 34, FontStyle.Bold),
                ForeColor = UIHelper.White,
                Location  = new Point(52, 180),
                AutoSize  = true,
                BackColor = Color.Transparent,
            });
            left.Controls.Add(new Label
            {
                Text      = "Centralize member data, track borrowings,\nmanage fines — all in one place.",
                Font      = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(140, 160, 180),
                Location  = new Point(52, 352),
                AutoSize  = true,
                BackColor = Color.Transparent,
            });

            // Bottom stats
            AddHeroStat(left, "2,400+", "MEMBERS",  new Point(52,  470));
            AddHeroStat(left, "8,000+", "BOOKS",    new Point(182, 470));
            AddHeroStat(left, "99.9%",  "UPTIME",   new Point(312, 470));

            // ── RIGHT form panel ──────────────────────────────────────
            var right = new Panel
            {
                Size      = new Size(440, 620),
                Location  = new Point(520, 0),
                BackColor = UIHelper.Cream,
            };
            this.Controls.Add(right);

            right.Controls.Add(new Label
            {
                Text      = "WELCOME BACK",
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UIHelper.Gold,
                Location  = new Point(40, 52),
                AutoSize  = true,
            });
            right.Controls.Add(new Label
            {
                Text     = "Sign In",
                Font     = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = UIHelper.Navy,
                Location  = new Point(38, 72),
                AutoSize  = true,
            });
            right.Controls.Add(new Label
            {
                Text     = "Access your library account below.",
                Font     = UIHelper.FontBody,
                ForeColor = UIHelper.Muted,
                Location  = new Point(40, 130),
                AutoSize  = true,
            });

            // ── Role tabs ──────────────────────────────────────────
            var ctrl1 = UIHelper.MakeCapsLabel("SELECT ROLE");

            ctrl1.Location = new Point(40, 166);

            right.Controls.Add(ctrl1);

            pnlRoleMember    = MakeRoleTab("👤  Member",    new Point(40, 184));
            pnlRoleLibrarian = MakeRoleTab("📋  Librarian", new Point(168, 184));
            pnlRoleAdmin     = MakeRoleTab("⚙  Admin",     new Point(296, 184));
            right.Controls.AddRange(new Control[] { pnlRoleMember, pnlRoleLibrarian, pnlRoleAdmin });
            SetRoleTab("member");

            pnlRoleMember.Click    += (s,e) => SetRoleTab("member");
            pnlRoleLibrarian.Click += (s,e) => SetRoleTab("librarian");
            pnlRoleAdmin.Click     += (s,e) => SetRoleTab("admin");
            // Forward clicks from child labels
            foreach(Control c in pnlRoleMember.Controls)    c.Click += (s,e) => SetRoleTab("member");
            foreach(Control c in pnlRoleLibrarian.Controls) c.Click += (s,e) => SetRoleTab("librarian");
            foreach(Control c in pnlRoleAdmin.Controls)     c.Click += (s,e) => SetRoleTab("admin");

            // ── Email field ────────────────────────────────────────
            var ctrl2 = UIHelper.MakeCapsLabel("EMAIL ADDRESS");

            ctrl2.Location = new Point(40, 238);

            right.Controls.Add(ctrl2);
            txtEmail = UIHelper.MakeInput("juan@email.com");
            txtEmail.Size = new Size(360, 36);

            txtEmail.Location = new Point(40, 256);
            right.Controls.Add(txtEmail);

            // ── Password field ─────────────────────────────────────
            var ctrl3 = UIHelper.MakeCapsLabel("PASSWORD");

            ctrl3.Location = new Point(40, 308);

            right.Controls.Add(ctrl3);
            txtPassword = UIHelper.MakeInput("••••••••", password: true);
            txtPassword.Size = new Size(360, 36);

            txtPassword.Location = new Point(40, 326);
            right.Controls.Add(txtPassword);

            // Forgot password
            lnkForgot = new LinkLabel
            {
                Text      = "Forgot password?",
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Location  = new Point(40, 374),
                AutoSize  = true,
                LinkColor = UIHelper.Gold,
                ActiveLinkColor = UIHelper.Gold2,
            };
            lnkForgot.LinkClicked += (s,e) => { new FormPasswordRecovery().ShowDialog(this); };
            right.Controls.Add(lnkForgot);

            // ── Error label ────────────────────────────────────────
            lblError = new Label
            {
                Text      = "",
                Font      = UIHelper.FontSmall,
                ForeColor = UIHelper.Red,
                Location  = new Point(40, 400),
                Size      = new Size(360, 18),
                Visible   = false,
            };
            right.Controls.Add(lblError);

            // ── Sign In button ─────────────────────────────────────
            btnSignIn      = UIHelper.MakePrimaryBtn("Sign In  →", new Size(360, 46));
            btnSignIn.Location = new Point(40, 422);

            btnSignIn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnSignIn.Click += BtnSignIn_Click;
            right.Controls.Add(btnSignIn);

            // Version
            right.Controls.Add(new Label
            {
                Text      = "Library IS v1.0.0  ·  .NET 6 + MySQL",
                Font      = UIHelper.FontSmall,
                ForeColor = UIHelper.Muted,
                Location  = new Point(40, 560),
                AutoSize  = true,
            });
        }

        // ── Hero left gradient + accent circle ────────────────────────
        private void PaintHeroLeft(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Radial-like gold accent (bottom-left)
            using var br = new SolidBrush(Color.FromArgb(22, 232, 169, 35));
            g.FillEllipse(br, -100, 380, 400, 400);

            // Subtle top-right circle
            using var br2 = new SolidBrush(Color.FromArgb(10, 232, 169, 35));
            g.FillEllipse(br2, 300, -80, 300, 300);
        }

        private void AddHeroStat(Panel p, string num, string label, Point loc)
        {
            p.Controls.Add(new Label
            {
                Text      = num,
                Font      = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = UIHelper.Gold,
                Location  = loc,
                AutoSize  = true,
                BackColor = Color.Transparent,
            });
            p.Controls.Add(new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 100, 120),
                Location  = new Point(loc.X, loc.Y + 28),
                AutoSize  = true,
                BackColor = Color.Transparent,
            });
        }

        // ── Role tab factory ──────────────────────────────────────────
        private Panel MakeRoleTab(string text, Point loc)
        {
            var p = new Panel
            {
                Size      = new Size(120, 40),
                Location  = loc,
                BackColor = UIHelper.White,
                Cursor    = Cursors.Hand,
            };
            p.Paint += (s, e) =>
            {
                bool active = p.Tag?.ToString() == "active";
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = UIHelper.RoundRect(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10);
                e.Graphics.FillPath(active ? new SolidBrush(UIHelper.Navy) : Brushes.White, path);
                e.Graphics.DrawPath(new Pen(active ? UIHelper.Navy : UIHelper.Border, 2), path);
            };
            var lbl = new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UIHelper.Muted,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor    = Cursors.Hand,
                BackColor = Color.Transparent,
            };
            p.Controls.Add(lbl);
            return p;
        }

        private void SetRoleTab(string role)
        {
            _selectedRole = role;
            pnlRoleMember.Tag    = role == "member"    ? "active" : "";
            pnlRoleLibrarian.Tag = role == "librarian" ? "active" : "";
            pnlRoleAdmin.Tag     = role == "admin"     ? "active" : "";

            // Update label colors
            void Style(Panel p)
            {
                bool a = p.Tag?.ToString() == "active";
                if (p.Controls.Count > 0)
                    p.Controls[0].ForeColor = a ? UIHelper.White : UIHelper.Muted;
                p.Invalidate();
            }
            Style(pnlRoleMember);
            Style(pnlRoleLibrarian);
            Style(pnlRoleAdmin);
        }

        // ── Sign In logic ─────────────────────────────────────────────
        private void BtnSignIn_Click(object? sender, EventArgs e)
        {
            lblError.Visible = false;
            string email    = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please fill in all fields.");
                return;
            }

            

            try
            {
                string sql = @"SELECT member_id, full_name, email, role, status, password
                               FROM   members
                               WHERE  email  = @email
                                 AND  status = 'active'
                               LIMIT  1";

                DataTable dt = DatabaseConnection.Instance.FillDataTable(sql,
                    new MySqlParameter("@email", email));

                if (dt.Rows.Count == 0)
                {
                    ShowError("Invalid credentials or account is inactive.");
                    return;
                }

                DataRow row   = dt.Rows[0];
                string dbHash = row["password"].ToString() ?? "";
                string dbRole = row["role"].ToString() ?? "";

                if (!PasswordHelper.Verify(password, dbHash))
                {
                    ShowError("Invalid password.");
                    return;
                }

                if (_selectedRole != "member" && dbRole != _selectedRole)
                {
                    ShowError($"Access denied — this account is not a {_selectedRole}.");
                    return;
                }

                int    memberId = Convert.ToInt32(row["member_id"]);
                string fullName = row["full_name"].ToString() ?? "User";

                var dashboard = new FormDashboard(memberId, fullName, dbRole);
                dashboard.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                ShowError("DB error: " + ex.Message);
            }
        }

        private void ShowError(string msg)
        {
            lblError.Text    = msg;
            lblError.Visible = true;
        }
    }
}
