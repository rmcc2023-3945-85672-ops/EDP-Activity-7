// ============================================================
// FormPasswordRecovery.cs — PASSWORD RECOVERY (3-step OTP)
// UI inspired by LibraryIS_App.html — white card, step indicator
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
    public class FormPasswordRecovery : Form
    {
        private string _recoveryEmail = "";
        private string _otpPin        = "";

        // Step panels
        private Panel pnlStep1 = null!, pnlStep2 = null!, pnlStep3 = null!;

        // Step circles
        private Label[] stepCircles = new Label[3];
        private Label[] stepLines   = new Label[2];

        // Step 1
        private TextBox txtRecoveryEmail = null!;

        // Step 2
        private TextBox[] otpBoxes = new TextBox[6];
        private Label     lblOtpHint = null!;

        // Step 3
        private TextBox txtNewPassword = null!, txtConfirmPassword = null!;

        public FormPasswordRecovery()
        {
            this.Text            = "Library IS — Password Recovery";
            this.Size            = new Size(560, 640);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = UIHelper.Cream;
            BuildUI();
            GoStep(1);
        }

        private void BuildUI()
        {
            // ── Top navbar strip ──────────────────────────────────────
            var nav = new Panel { Size = new Size(560, 52), Location = new Point(0,0), BackColor = UIHelper.Navy };
            this.Controls.Add(nav);
            nav.Controls.Add(new Label { Text="📚", Font=new Font("Segoe UI",16), ForeColor=UIHelper.Gold, Location=new Point(20,10), AutoSize=true, BackColor=Color.Transparent });
            nav.Controls.Add(new Label { Text="Library IS", Font=new Font("Segoe UI",12,FontStyle.Bold), ForeColor=UIHelper.White, Location=new Point(52,14), AutoSize=true, BackColor=Color.Transparent });

            // ── Step indicator ────────────────────────────────────────
            string[] stepLabels = { "Identify", "Verify OTP", "New Password" };
            var indicator = new Panel { Size = new Size(480, 64), Location = new Point(40, 72), BackColor = Color.Transparent };
            this.Controls.Add(indicator);

            for (int i = 0; i < 3; i++)
            {
                int x = i * 160;
                stepCircles[i] = new Label
                {
                    Text      = (i + 1).ToString(),
                    Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                    Size      = new Size(36, 36),
                    Location  = new Point(x, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.FromArgb(220, 220, 220),
                    ForeColor = UIHelper.Muted,
                };
                indicator.Controls.Add(stepCircles[i]);

                indicator.Controls.Add(new Label
                {
                    Text      = stepLabels[i],
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = UIHelper.Muted,
                    Location  = new Point(x, 42),
                    AutoSize  = true,
                    BackColor = Color.Transparent,
                });

                if (i < 2)
                {
                    stepLines[i] = new Label
                    {
                        Size      = new Size(116, 2),
                        Location  = new Point(x + 38, 17),
                        BackColor = Color.FromArgb(220, 220, 220),
                    };
                    indicator.Controls.Add(stepLines[i]);
                }
            }

            // ── White card ────────────────────────────────────────────
            var card = new Panel { Size = new Size(480, 440), Location = new Point(40, 156), BackColor = UIHelper.White };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = UIHelper.RoundRect(new Rectangle(0, 0, card.Width-1, card.Height-1), 14);
                e.Graphics.DrawPath(new Pen(UIHelper.Border), path);
            };
            this.Controls.Add(card);

            // ── STEP 1 ────────────────────────────────────────────────
            pnlStep1 = MakeStepPanel(card);

            pnlStep1.Controls.Add(new Label { Text="✉️", Font=new Font("Segoe UI",28), Location=new Point(30,28), AutoSize=true, BackColor=Color.Transparent });
            pnlStep1.Controls.Add(new Label { Text="PASSWORD RECOVERY", Font=UIHelper.FontCaps, ForeColor=UIHelper.Gold, Location=new Point(30,92), AutoSize=true });
            pnlStep1.Controls.Add(new Label { Text="Enter Your Email", Font=UIHelper.FontTitle, ForeColor=UIHelper.Navy, Location=new Point(28,112), AutoSize=true });
            pnlStep1.Controls.Add(new Label
            {
                Text     = "We'll generate a 6-digit verification code\nfor your registered email address.",
                Font     = UIHelper.FontBody, ForeColor = UIHelper.Muted,
                Location = new Point(30,148), AutoSize=true,
            });

            var ctrl1 = UIHelper.MakeCapsLabel("EMAIL ADDRESS");


            ctrl1.Location = new Point(30, 206);


            pnlStep1.Controls.Add(ctrl1);
            txtRecoveryEmail = UIHelper.MakeInput("juan@email.com");
            txtRecoveryEmail.Size = new Size(420, 36);

            txtRecoveryEmail.Location = new Point(30, 224);
            pnlStep1.Controls.Add(txtRecoveryEmail);

            var btnSend = UIHelper.MakeGoldBtn("Send Verification Code  →", new Size(420, 44));
            btnSend.Location = new Point(30, 278);
            btnSend.Click   += BtnSendCode_Click;
            pnlStep1.Controls.Add(btnSend);

            var btnBack1 = UIHelper.MakeOutlineBtn("← Back to Login", new Size(420, 40));
            btnBack1.Location = new Point(30, 334);
            btnBack1.Click   += (s,e) => this.Close();
            pnlStep1.Controls.Add(btnBack1);

            // ── STEP 2 ────────────────────────────────────────────────
            pnlStep2 = MakeStepPanel(card);

            pnlStep2.Controls.Add(new Label { Text="✉️", Font=new Font("Segoe UI",28), Location=new Point(30,20), AutoSize=true, BackColor=Color.Transparent });
            pnlStep2.Controls.Add(new Label { Text="PASSWORD RECOVERY", Font=UIHelper.FontCaps, ForeColor=UIHelper.Gold, Location=new Point(30,80), AutoSize=true });
            pnlStep2.Controls.Add(new Label { Text="Check Your Email", Font=UIHelper.FontTitle, ForeColor=UIHelper.Navy, Location=new Point(28,100), AutoSize=true });

            lblOtpHint = new Label { Text="We sent a code to your email.", Font=UIHelper.FontBody, ForeColor=UIHelper.Muted, Location=new Point(30,136), AutoSize=true };
            pnlStep2.Controls.Add(lblOtpHint);

            // Green success banner
            var banner = new Panel { Size=new Size(420,34), Location=new Point(30,162), BackColor=Color.FromArgb(234,250,241) };
            banner.Paint += (s,e) => e.Graphics.DrawRectangle(new Pen(Color.FromArgb(46,204,113)), 0,0,banner.Width-1,banner.Height-1);
            banner.Controls.Add(new Label { Text="✅  Code sent! Check your email or note the popup.", Font=UIHelper.FontSmall, ForeColor=Color.FromArgb(30,132,73), Location=new Point(8,8), AutoSize=true });
            pnlStep2.Controls.Add(banner);

            var ctrl2 = UIHelper.MakeCapsLabel("VERIFICATION CODE");


            ctrl2.Location = new Point(30, 208);


            pnlStep2.Controls.Add(ctrl2);

            // 6 OTP boxes
            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                otpBoxes[i] = new TextBox
                {
                    Font        = new Font("Segoe UI", 22, FontStyle.Bold),
                    Size        = new Size(52, 58),
                    Location    = new Point(30 + i * 65, 228),
                    TextAlign   = HorizontalAlignment.Center,
                    MaxLength   = 1,
                    BorderStyle = BorderStyle.FixedSingle,
                };
                otpBoxes[i].TextChanged += (s, e) =>
                {
                    if (otpBoxes[idx].Text.Length == 1 && idx < 5)
                        otpBoxes[idx + 1].Focus();
                };
                pnlStep2.Controls.Add(otpBoxes[i]);
            }

            pnlStep2.Controls.Add(new Label { Text="Didn't receive it? Check spam or restart.", Font=UIHelper.FontSmall, ForeColor=UIHelper.Muted, Location=new Point(30,300), AutoSize=true });

            var btnVerify = UIHelper.MakeGoldBtn("Verify Code  →", new Size(420, 44));
            btnVerify.Location = new Point(30, 326);
            btnVerify.Click   += BtnVerifyCode_Click;
            pnlStep2.Controls.Add(btnVerify);

            var btnBack2 = UIHelper.MakeOutlineBtn("← Back to Login", new Size(420, 40));
            btnBack2.Location = new Point(30, 382);
            btnBack2.Click   += (s,e) => this.Close();
            pnlStep2.Controls.Add(btnBack2);

            // ── STEP 3 ────────────────────────────────────────────────
            pnlStep3 = MakeStepPanel(card);

            pnlStep3.Controls.Add(new Label { Text="🔒", Font=new Font("Segoe UI",28), Location=new Point(30,28), AutoSize=true, BackColor=Color.Transparent });
            pnlStep3.Controls.Add(new Label { Text="PASSWORD RECOVERY", Font=UIHelper.FontCaps, ForeColor=UIHelper.Gold, Location=new Point(30,92), AutoSize=true });
            pnlStep3.Controls.Add(new Label { Text="Set New Password", Font=UIHelper.FontTitle, ForeColor=UIHelper.Navy, Location=new Point(28,112), AutoSize=true });
            pnlStep3.Controls.Add(new Label { Text="Create a strong password for your account.", Font=UIHelper.FontBody, ForeColor=UIHelper.Muted, Location=new Point(30,150), AutoSize=true });

            var ctrl3 = UIHelper.MakeCapsLabel("NEW PASSWORD");


            ctrl3.Location = new Point(30, 194);


            pnlStep3.Controls.Add(ctrl3);
            txtNewPassword = UIHelper.MakeInput("Min. 6 characters", password: true);
            txtNewPassword.Size = new Size(420, 36);

            txtNewPassword.Location = new Point(30, 212);
            pnlStep3.Controls.Add(txtNewPassword);

            var ctrl4 = UIHelper.MakeCapsLabel("CONFIRM PASSWORD");


            ctrl4.Location = new Point(30, 262);


            pnlStep3.Controls.Add(ctrl4);
            txtConfirmPassword = UIHelper.MakeInput("Re-enter password", password: true);
            txtConfirmPassword.Size = new Size(420, 36);

            txtConfirmPassword.Location = new Point(30, 280);
            pnlStep3.Controls.Add(txtConfirmPassword);

            var btnUpdate = UIHelper.MakeGoldBtn("Update Password  →", new Size(420, 44));
            btnUpdate.Location = new Point(30, 338);
            btnUpdate.Click   += BtnUpdatePassword_Click;
            pnlStep3.Controls.Add(btnUpdate);
        }

        private Panel MakeStepPanel(Panel parent)
        {
            var p = new Panel { Size = parent.Size, Location = new Point(0,0), BackColor = UIHelper.White, Visible = false };
            parent.Controls.Add(p);
            return p;
        }

        private void GoStep(int step)
        {
            pnlStep1.Visible = step == 1;
            pnlStep2.Visible = step == 2;
            pnlStep3.Visible = step == 3;

            // Update step indicator circles
            for (int i = 0; i < 3; i++)
            {
                bool done   = i + 1 < step;
                bool active = i + 1 == step;

                stepCircles[i].Text      = done ? "✓" : (i + 1).ToString();
                stepCircles[i].BackColor = done   ? UIHelper.Green :
                                           active ? UIHelper.Navy  :
                                           Color.FromArgb(220, 220, 220);
                stepCircles[i].ForeColor = (done || active) ? UIHelper.White : UIHelper.Muted;
            }

            for (int i = 0; i < 2; i++)
                stepLines[i].BackColor = i + 1 < step ? UIHelper.Green : Color.FromArgb(220,220,220);
        }

        // ── STEP 1: Send OTP ─────────────────────────────────────────
        private void BtnSendCode_Click(object? sender, EventArgs e)
        {
            string email = txtRecoveryEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            { MessageBox.Show("Please enter your email.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                object? result = DatabaseConnection.Instance.ExecuteScalar(
                    "SELECT member_id FROM members WHERE email = @email LIMIT 1",
                    new MySqlParameter("@email", email));

                if (result == null)
                { MessageBox.Show("Email not found in our system.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                _otpPin        = PasswordHelper.GeneratePin();
                _recoveryEmail = email;

                DatabaseConnection.Instance.ExecuteNonQuery(
                    "UPDATE members SET recovery_pin = @pin WHERE email = @email",
                    new MySqlParameter("@pin",   _otpPin),
                    new MySqlParameter("@email", _recoveryEmail));

                MessageBox.Show($"Your OTP code is: {_otpPin}\n(In production this would be sent via email.)",
                    "Verification Code", MessageBoxButtons.OK, MessageBoxIcon.Information);

                int at = email.IndexOf('@');
                lblOtpHint.Text = at > 1
                    ? $"We sent a code to {email.Substring(0, 2)}***{email.Substring(at)}."
                    : "We sent a code to your email.";

                GoStep(2);
            }
            catch (Exception ex)
            { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ── STEP 2: Verify OTP ───────────────────────────────────────
        private void BtnVerifyCode_Click(object? sender, EventArgs e)
        {
            string entered = "";
            foreach (var box in otpBoxes) entered += box.Text;

            if (entered.Length < 6)
            { MessageBox.Show("Please enter all 6 digits.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                object? dbPin = DatabaseConnection.Instance.ExecuteScalar(
                    "SELECT recovery_pin FROM members WHERE email = @email LIMIT 1",
                    new MySqlParameter("@email", _recoveryEmail));

                if (dbPin == null || dbPin.ToString() != entered)
                { MessageBox.Show("Incorrect code. Please try again.", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                GoStep(3);
            }
            catch (Exception ex)
            { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ── STEP 3: Reset password ────────────────────────────────────
        private void BtnUpdatePassword_Click(object? sender, EventArgs e)
        {
            string np = txtNewPassword.Text;
            string cp = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(np) || string.IsNullOrWhiteSpace(cp))
            { MessageBox.Show("Please fill in both fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (np != cp)
            { MessageBox.Show("Passwords do not match.", "Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (np.Length < 6)
            { MessageBox.Show("Password must be at least 6 characters.", "Too Short", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                DatabaseConnection.Instance.ExecuteNonQuery(
                    "UPDATE members SET password = @pw, recovery_pin = NULL WHERE email = @email",
                    new MySqlParameter("@pw",    PasswordHelper.Hash(np)),
                    new MySqlParameter("@email", _recoveryEmail));

                MessageBox.Show("Password updated! Please sign in with your new password.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
