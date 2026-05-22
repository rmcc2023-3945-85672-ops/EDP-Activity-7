using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LibrarySystem.UI
{
    /// <summary>
    /// Shared design tokens and UI factory helpers — mirrors the HTML design system.
    /// Colors: Navy, Gold, Cream, Muted, Green, Red, Blue
    /// </summary>
    public static class UIHelper
    {
        // ── Design Tokens ──────────────────────────────
        public static readonly Color Navy    = Color.FromArgb(15,  27,  45);
        public static readonly Color Navy2   = Color.FromArgb(26,  43,  66);
        public static readonly Color Gold    = Color.FromArgb(232, 169, 35);
        public static readonly Color Gold2   = Color.FromArgb(245, 200, 66);
        public static readonly Color Cream   = Color.FromArgb(247, 243, 236);
        public static readonly Color White   = Color.White;
        public static readonly Color Text    = Color.FromArgb(26,  43,  66);
        public static readonly Color Muted   = Color.FromArgb(122, 143, 166);
        public static readonly Color Green   = Color.FromArgb(46,  204, 113);
        public static readonly Color Red     = Color.FromArgb(231, 76,  60);
        public static readonly Color Blue    = Color.FromArgb(52,  152, 219);
        public static readonly Color Border  = Color.FromArgb(224, 217, 208);

        public static readonly Font FontBig    = new Font("Segoe UI", 22, FontStyle.Bold);
        public static readonly Font FontTitle  = new Font("Segoe UI", 16, FontStyle.Bold);
        public static readonly Font FontH2     = new Font("Segoe UI", 13, FontStyle.Bold);
        public static readonly Font FontBold   = new Font("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font FontBody   = new Font("Segoe UI", 10);
        public static readonly Font FontSmall  = new Font("Segoe UI",  8);
        public static readonly Font FontCaps   = new Font("Segoe UI",  8, FontStyle.Bold);
        public static readonly Font FontInput  = new Font("Segoe UI", 11);
        public static readonly Font FontNav    = new Font("Segoe UI", 10);

        // ── Primary button (navy) ───────────────────────
        public static Button MakePrimaryBtn(string text, Size size)
        {
            var b = new Button
            {
                Text      = text,
                Font      = FontBold,
                Size      = size,
                BackColor = Navy,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            b.FlatAppearance.BorderSize = 0;
            b.MouseEnter += (s,e) => b.BackColor = Navy2;
            b.MouseLeave += (s,e) => b.BackColor = Navy;
            return b;
        }

        // ── Gold / accent button ───────────────────────
        public static Button MakeGoldBtn(string text, Size size)
        {
            var b = new Button
            {
                Text      = text,
                Font      = FontBold,
                Size      = size,
                BackColor = Gold,
                ForeColor = Navy,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            b.FlatAppearance.BorderSize = 0;
            b.MouseEnter += (s,e) => b.BackColor = Gold2;
            b.MouseLeave += (s,e) => b.BackColor = Gold;
            return b;
        }

        // ── Outline / secondary button ──────────────────
        public static Button MakeOutlineBtn(string text, Size size)
        {
            var b = new Button
            {
                Text      = text,
                Font      = FontBold,
                Size      = size,
                BackColor = White,
                ForeColor = Navy,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            b.FlatAppearance.BorderColor = Border;
            b.FlatAppearance.BorderSize  = 1;
            b.MouseEnter += (s,e) => b.FlatAppearance.BorderColor = Navy;
            b.MouseLeave += (s,e) => b.FlatAppearance.BorderColor = Border;
            return b;
        }

        // ── Tinted action button (e.g. blue-tint Edit) ─
        public static Button MakeTintBtn(string text, Size size, Color back, Color fore)
        {
            var b = new Button
            {
                Text      = text,
                Font      = FontBold,
                Size      = size,
                BackColor = back,
                ForeColor = fore,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // ── Styled text input ───────────────────────────
        public static TextBox MakeInput(string placeholder, bool password = false)
        {
            var t = new TextBox
            {
                Font               = FontInput,
                BorderStyle        = BorderStyle.FixedSingle,
                PlaceholderText    = placeholder,
                UseSystemPasswordChar = password,
                BackColor          = White,
            };
            return t;
        }

        // ── Label factory ───────────────────────────────
        public static Label MakeLabel(string text, Font font, Color color,
                                      bool autoSize = true)
        {
            return new Label
            {
                Text      = text,
                Font      = font,
                ForeColor = color,
                AutoSize  = autoSize,
                BackColor = Color.Transparent,
            };
        }

        // ── Caps label (like HTML .form-label) ──────────
        public static Label MakeCapsLabel(string text)
            => MakeLabel(text, FontCaps, Navy);

        // ── White card panel with border ─────────────────
        public static Panel MakeCard(int w, int h)
        {
            var p = new Panel
            {
                Size      = new Size(w, h),
                BackColor = White,
            };
            p.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Border), 0, 0, p.Width - 1, p.Height - 1);
            return p;
        }

        // ── Rounded card (like HTML border-radius: 14px) ─
        public static Panel MakeRoundCard(int w, int h, int radius = 14)
        {
            var p = new Panel
            {
                Size      = new Size(w, h),
                BackColor = White,
            };
            p.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path  = RoundRect(new Rectangle(0, 0, p.Width - 1, p.Height - 1), radius);
                g.FillPath(Brushes.White, path);
                g.DrawPath(new Pen(Border), path);
            };
            return p;
        }

        // ── DataGridView — library style ─────────────────
        public static DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                BackgroundColor   = White,
                BorderStyle       = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                ReadOnly          = true,
                SelectionMode     = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font              = FontBody,
                GridColor         = Color.FromArgb(240, 237, 232),
                CellBorderStyle   = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Cream,
                    ForeColor = Muted,
                    Font      = FontCaps,
                    Padding   = new Padding(6, 0, 0, 0),
                    SelectionBackColor = Cream,
                    SelectionForeColor = Muted,
                },
                RowTemplate = { Height = 38 },
            };
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
            g.DefaultCellStyle.SelectionForeColor = Navy;
            g.DefaultCellStyle.Padding           = new Padding(6, 0, 0, 0);
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight         = 38;
            g.EnableHeadersVisualStyles   = false;
            return g;
        }

        // ── Horizontal divider ────────────────────────────
        public static Label MakeDivider(int w)
            => new Label { Size = new Size(w, 1), BackColor = Border };

        // ── Badge / pill label ────────────────────────────
        public static Label MakeBadge(string text, Color back, Color fore)
        {
            var l = new Label
            {
                Text      = text,
                Font      = FontSmall,
                ForeColor = fore,
                BackColor = back,
                AutoSize  = false,
                Size      = new Size(72, 20),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            return l;
        }

        // ── Rounded-rect path helper ──────────────────────
        public static GraphicsPath RoundRect(Rectangle r, int d)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
