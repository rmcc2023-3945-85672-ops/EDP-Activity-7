// ============================================================
// FormReports.cs — REPORT GENERATION MODULE
// Three reports with DataGridView + Excel export:
//   1. Borrowings Report   (Sheet1 data, Sheet2 bar chart)
//   2. Reservations Report (Sheet1 data, Sheet2 pie chart)
//   3. Fine Payments Report(Sheet1 data, Sheet2 column chart)
// Each exported Excel file includes:
//   • Header with company name + logo placeholder
//   • Signature placeholder
//   • Sheet 2 with a chart of the data
// ============================================================

using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using MySql.Data.MySqlClient;
using LibrarySystem.Database;
using LibrarySystem.UI;

// NOTE: Add EPPlus to .csproj:
//   <PackageReference Include="EPPlus" Version="6.2.10" />
// EPPlus 6+ is licensed under PolyForm Noncommercial; set the context:
//   ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

namespace LibrarySystem.Forms
{
    public class FormReports : Form
    {
        private TabControl      tabReports   = null!;
        private DataGridView    dgvBorrow    = null!, dgvReserve = null!, dgvFines = null!;
        private Label           lblBorrowCount = null!, lblReserveCount = null!, lblFinesCount = null!;
        private ComboBox        cboStatus    = null!;
        private DateTimePicker  dtpFrom      = null!, dtpTo = null!;

        public FormReports()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Text          = "Library IS — Reports";
            Size          = new Size(1100, 760);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = UIHelper.Cream;
            BuildUI();
            LoadAllReports();
        }

        // ── UI ────────────────────────────────────────────────
        private void BuildUI()
        {
            // Header bar
            var hdr = new Panel { Size = new Size(1100, 68), BackColor = UIHelper.White };
            hdr.Paint += (s, e) => e.Graphics.DrawLine(new Pen(UIHelper.Border), 0, 67, 1100, 67);
            Controls.Add(hdr);
            hdr.Controls.Add(UIHelper.MakeLabel("Report Generation", UIHelper.FontTitle, UIHelper.Navy)
                             .Tap(l => l.Location = new Point(28, 14)));
            hdr.Controls.Add(UIHelper.MakeLabel("Home › Reports", UIHelper.FontSmall, UIHelper.Muted)
                             .Tap(l => l.Location = new Point(30, 46)));

            // Filter bar
            var filterBar = new Panel { Size = new Size(1100, 54), Location = new Point(0, 70),
                                         BackColor = UIHelper.White };
            filterBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(UIHelper.Border), 0, 53, 1100, 53);
            Controls.Add(filterBar);

            filterBar.Controls.Add(UIHelper.MakeCapsLabel("FROM").Tap(l => l.Location = new Point(28, 14)));
            dtpFrom = new DateTimePicker { Size = new Size(130, 26), Location = new Point(28, 28),
                                            Format = DateTimePickerFormat.Short,
                                            Value = DateTime.Today.AddMonths(-3), Font = UIHelper.FontBody };
            filterBar.Controls.Add(dtpFrom);

            filterBar.Controls.Add(UIHelper.MakeCapsLabel("TO").Tap(l => l.Location = new Point(172, 14)));
            dtpTo = new DateTimePicker { Size = new Size(130, 26), Location = new Point(172, 28),
                                          Format = DateTimePickerFormat.Short,
                                          Value = DateTime.Today, Font = UIHelper.FontBody };
            filterBar.Controls.Add(dtpTo);

            var btnRefresh = UIHelper.MakePrimaryBtn("🔄  Refresh", new Size(110, 34));
            btnRefresh.Location = new Point(316, 20);
            btnRefresh.Click += (s, e) => LoadAllReports();
            filterBar.Controls.Add(btnRefresh);

            // Tabs
            tabReports = new TabControl
            {
                Location = new Point(0, 124),
                Size     = new Size(1100, 636),
                Font     = UIHelper.FontBold,
            };
            Controls.Add(tabReports);

            // ── Tab 1 : Borrowings ─────────────────────────
            var tabB = new TabPage("📖  Borrowings Report") { BackColor = UIHelper.Cream };
            tabReports.TabPages.Add(tabB);
            lblBorrowCount = BuildReportTab(tabB, out dgvBorrow,
                "Borrow Date, Member, Book, Status, Due Date",
                "Export Borrowings to Excel", BtnExportBorrow_Click);

            // ── Tab 2 : Reservations ───────────────────────
            var tabR = new TabPage("🔖  Reservations Report") { BackColor = UIHelper.Cream };
            tabReports.TabPages.Add(tabR);
            lblReserveCount = BuildReportTab(tabR, out dgvReserve,
                "Reserved Date, Member, Book, Status, Expiry",
                "Export Reservations to Excel", BtnExportReserve_Click);

            // ── Tab 3 : Fine Payments ──────────────────────
            var tabF = new TabPage("💰  Fine Payments Report") { BackColor = UIHelper.Cream };
            tabReports.TabPages.Add(tabF);
            lblFinesCount = BuildReportTab(tabF, out dgvFines,
                "Payment Date, Member, Book, Amount Paid, Method",
                "Export Fine Payments to Excel", BtnExportFines_Click);
        }

        /// Creates a uniform tab layout; returns the record-count label.
        private Label BuildReportTab(TabPage page, out DataGridView dgv,
                                     string description, string exportLabel,
                                     EventHandler exportHandler)
        {
            var desc = UIHelper.MakeLabel(description, UIHelper.FontSmall, UIHelper.Muted);
            desc.Location = new Point(20, 12);
            page.Controls.Add(desc);

            var countLbl = UIHelper.MakeLabel("— records", UIHelper.FontBody, UIHelper.Muted);
            countLbl.Location = new Point(20, 30);
            page.Controls.Add(countLbl);

            var btnExport = UIHelper.MakeGoldBtn("📊  " + exportLabel, new Size(260, 36));
            btnExport.Location = new Point(800, 14);
            btnExport.Click += exportHandler;
            page.Controls.Add(btnExport);

            var gridCard = UIHelper.MakeCard(1060, 540);
            gridCard.Location = new Point(16, 60);
            page.Controls.Add(gridCard);

            dgv = UIHelper.MakeGrid();
            dgv.Location = new Point(0, 0);
            dgv.Size     = new Size(1060, 540);
            gridCard.Controls.Add(dgv);

            return countLbl;
        }

        // ── Data loading ──────────────────────────────────────
        private void LoadAllReports()
        {
            LoadBorrowings();
            LoadReservations();
            LoadFinePayments();
        }

        private void LoadBorrowings()
        {
            var dt = DatabaseConnection.Instance.FillDataTable(
                @"SELECT br.borrow_id         AS ID,
                         br.borrow_date        AS 'Borrow Date',
                         m.full_name           AS Member,
                         b.title               AS Book,
                         a.author_name         AS Author,
                         b.genre               AS Genre,
                         br.return_date        AS 'Due Date',
                         br.status             AS Status
                  FROM borrowings br
                  JOIN members m ON m.member_id = br.member_id
                  JOIN books   b ON b.book_id   = br.book_id
                  JOIN authors a ON a.author_id  = b.author_id
                  WHERE br.borrow_date BETWEEN @f AND @t
                  ORDER BY br.borrow_date DESC",
                DateParam("@f", dtpFrom.Value.Date),
                DateParam("@t", dtpTo.Value.Date));
            dgvBorrow.DataSource = dt;
            lblBorrowCount.Text  = $"{dt.Rows.Count} records";
        }

        private void LoadReservations()
        {
            var dt = DatabaseConnection.Instance.FillDataTable(
                @"SELECT r.reservation_id  AS ID,
                         r.reserved_date   AS 'Reserved Date',
                         m.full_name       AS Member,
                         b.title           AS Book,
                         r.expiry_date     AS 'Expiry Date',
                         r.status          AS Status
                  FROM reservations r
                  JOIN members m ON m.member_id = r.member_id
                  JOIN books   b ON b.book_id   = r.book_id
                  WHERE r.reserved_date BETWEEN @f AND @t
                  ORDER BY r.reserved_date DESC",
                DateParam("@f", dtpFrom.Value.Date),
                DateParam("@t", dtpTo.Value.Date));
            dgvReserve.DataSource = dt;
            lblReserveCount.Text  = $"{dt.Rows.Count} records";
        }

        private void LoadFinePayments()
        {
            var dt = DatabaseConnection.Instance.FillDataTable(
                @"SELECT fp.payment_id     AS ID,
                         fp.payment_date   AS 'Payment Date',
                         m.full_name       AS Member,
                         b.title           AS Book,
                         fp.amount_paid    AS 'Amount (₱)',
                         fp.payment_method AS Method,
                         fp.remarks        AS Remarks,
                         r.full_name       AS 'Received By'
                  FROM fine_payments fp
                  JOIN fines      f  ON f.fine_id    = fp.fine_id
                  JOIN borrowings br ON br.borrow_id = f.borrow_id
                  JOIN members    m  ON m.member_id  = br.member_id
                  JOIN books      b  ON b.book_id    = br.book_id
                  JOIN members    r  ON r.member_id  = fp.received_by
                  WHERE fp.payment_date BETWEEN @f AND @t
                  ORDER BY fp.payment_date DESC",
                DateParam("@f", dtpFrom.Value.Date),
                DateParam("@t", dtpTo.Value.Date));
            dgvFines.DataSource = dt;
            lblFinesCount.Text  = $"{dt.Rows.Count} records";
        }

        // ── Excel Exports ─────────────────────────────────────
        private void BtnExportBorrow_Click(object? sender, EventArgs e)
        {
            var dt = (DataTable?)dgvBorrow.DataSource;
            if (dt == null || dt.Rows.Count == 0) { NoData(); return; }

            using var dlg = new SaveFileDialog
            { Filter = "Excel Files|*.xlsx", FileName = $"Borrowings_Report_{DateTime.Today:yyyyMMdd}.xlsx" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using var pkg = BuildExcelPackage(dt, "Borrowings Report",
                "Borrowings by Status", eChartType.BarClustered,
                BuildBorrowChartData(dt));
            pkg.SaveAs(new FileInfo(dlg.FileName));
            MessageBox.Show("Report exported successfully!\n" + dlg.FileName,
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnExportReserve_Click(object? sender, EventArgs e)
        {
            var dt = (DataTable?)dgvReserve.DataSource;
            if (dt == null || dt.Rows.Count == 0) { NoData(); return; }

            using var dlg = new SaveFileDialog
            { Filter = "Excel Files|*.xlsx", FileName = $"Reservations_Report_{DateTime.Today:yyyyMMdd}.xlsx" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using var pkg = BuildExcelPackage(dt, "Reservations Report",
                "Reservations by Status", eChartType.Pie,
                BuildStatusChartData(dt, "Status"));
            pkg.SaveAs(new FileInfo(dlg.FileName));
            MessageBox.Show("Report exported successfully!\n" + dlg.FileName,
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnExportFines_Click(object? sender, EventArgs e)
        {
            var dt = (DataTable?)dgvFines.DataSource;
            if (dt == null || dt.Rows.Count == 0) { NoData(); return; }

            using var dlg = new SaveFileDialog
            { Filter = "Excel Files|*.xlsx", FileName = $"FinePayments_Report_{DateTime.Today:yyyyMMdd}.xlsx" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using var pkg = BuildExcelPackage(dt, "Fine Payments Report",
                "Payments by Method", eChartType.ColumnClustered,
                BuildStatusChartData(dt, "Method"));
            pkg.SaveAs(new FileInfo(dlg.FileName));
            MessageBox.Show("Report exported successfully!\n" + dlg.FileName,
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Core Excel builder ────────────────────────────────
        /// <summary>
        /// Builds an ExcelPackage with:
        ///   Sheet1 — header (logo + company name) + data table + signature block
        ///   Sheet2 — chart built from chartData (label → value)
        /// </summary>
        private ExcelPackage BuildExcelPackage(DataTable dt, string reportTitle,
                                               string chartTitle, eChartType chartType,
                                               (string[] Labels, double[] Values) chartData)
        {
            var pkg  = new ExcelPackage();
            var ws1  = pkg.Workbook.Worksheets.Add("Report Data");
            var ws2  = pkg.Workbook.Worksheets.Add("Chart");

            // ── SHEET 1 ───────────────────────────────────────
            // Row 1-2 : Logo placeholder + Company Name
            var logoCell = ws1.Cells["A1:B3"];
            logoCell.Merge = true;
            ws1.Cells["A1"].Value = "[ LOGO ]";
            ws1.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws1.Cells["A1"].Style.VerticalAlignment   = ExcelVerticalAlignment.Center;
            ws1.Cells["A1"].Style.Font.Bold = true;
            ws1.Cells["A1"].Style.Font.Size = 14;
            ws1.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws1.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(15, 27, 45));
            ws1.Cells["A1"].Style.Font.Color.SetColor(Color.White);
            ws1.Row(1).Height = 20;
            ws1.Row(2).Height = 20;
            ws1.Row(3).Height = 20;
            ws1.Column(1).Width = 14;
            ws1.Column(2).Width = 14;

            // Company name
            int cols = dt.Columns.Count;
            var titleRange = ws1.Cells[1, 3, 2, Math.Max(cols, 6)];
            titleRange.Merge = true;
            titleRange.Value = "LIBRARY INFORMATION SYSTEM";
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.Size = 16;
            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            titleRange.Style.VerticalAlignment   = ExcelVerticalAlignment.Center;
            titleRange.Style.Font.Color.SetColor(Color.FromArgb(15, 27, 45));

            // Sub-title (report name)
            var subRange = ws1.Cells[3, 3, 3, Math.Max(cols, 6)];
            subRange.Merge = true;
            subRange.Value = reportTitle + $"  |  Period: {dtpFrom.Value:MMMM d, yyyy} – {dtpTo.Value:MMMM d, yyyy}";
            subRange.Style.Font.Italic = true;
            subRange.Style.Font.Size   = 10;
            subRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            subRange.Style.Font.Color.SetColor(Color.FromArgb(122, 143, 166));

            // Row 4: Generated line
            var genRange = ws1.Cells[4, 1, 4, Math.Max(cols, 6)];
            genRange.Merge = true;
            genRange.Value = $"Generated: {DateTime.Now:MMMM d, yyyy  hh:mm tt}   |   Total Records: {dt.Rows.Count}";
            genRange.Style.Font.Size = 9;
            genRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            genRange.Style.Font.Color.SetColor(Color.FromArgb(122, 143, 166));

            // Divider row 5
            ws1.Row(5).Height = 4;
            for (int c = 1; c <= Math.Max(cols, 6); c++)
            {
                ws1.Cells[5, c].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws1.Cells[5, c].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(232, 169, 35)); // gold
            }

            // Row 6: Column headers
            int headerRow = 6;
            var navyColor = Color.FromArgb(15, 27, 45);
            for (int c = 0; c < dt.Columns.Count; c++)
            {
                var cell = ws1.Cells[headerRow, c + 1];
                cell.Value = dt.Columns[c].ColumnName.ToUpper();
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 10;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(navyColor);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(232, 169, 35));
            }

            // Data rows
            var cream = Color.FromArgb(247, 243, 236);
            for (int r = 0; r < dt.Rows.Count; r++)
            {
                bool isEven = r % 2 == 0;
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    var cell = ws1.Cells[headerRow + 1 + r, c + 1];
                    cell.Value = dt.Rows[r][c];
                    cell.Style.Font.Size = 10;
                    if (isEven)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(cream);
                    }
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(224, 217, 208));
                }
            }

            // Auto-fit columns
            ws1.Cells[ws1.Dimension.Address].AutoFitColumns(12, 40);

            // Freeze header
            ws1.View.FreezePanes(headerRow + 1, 1);

            // ── Signature block ───────────────────────────────
            int sigRow = headerRow + dt.Rows.Count + 4;
            ws1.Cells[sigRow, 1].Value     = "Prepared by:";
            ws1.Cells[sigRow, 1].Style.Font.Bold = true;

            ws1.Cells[sigRow + 3, 1, sigRow + 3, 3].Merge = true;
            ws1.Cells[sigRow + 3, 1].Value = "________________________________";
            ws1.Cells[sigRow + 4, 1].Value = "Librarian / Staff Name";
            ws1.Cells[sigRow + 4, 1].Style.Font.Italic = true;
            ws1.Cells[sigRow + 4, 1].Style.Font.Color.SetColor(Color.FromArgb(122, 143, 166));

            ws1.Cells[sigRow + 3, cols - 2, sigRow + 3, cols].Merge = true;
            ws1.Cells[sigRow + 3, cols - 2].Value = "________________________________";
            ws1.Cells[sigRow + 4, cols - 2, sigRow + 4, cols].Merge = true;
            ws1.Cells[sigRow + 4, cols - 2].Value = "Approved by / Head Librarian";
            ws1.Cells[sigRow + 4, cols - 2].Style.Font.Italic = true;
            ws1.Cells[sigRow + 4, cols - 2].Style.Font.Color.SetColor(Color.FromArgb(122, 143, 166));

            ws1.Cells[sigRow + 6, 1].Value = "Date Signed: ____________________";
            ws1.Cells[sigRow + 6, 1].Style.Font.Size = 9;

            // ── SHEET 2: Chart ────────────────────────────────
            // Write labels + values for chart data
            for (int i = 0; i < chartData.Labels.Length; i++)
            {
                ws2.Cells[i + 2, 1].Value = chartData.Labels[i];
                ws2.Cells[i + 2, 2].Value = chartData.Values[i];
            }
            ws2.Cells[1, 1].Value = "Category";
            ws2.Cells[1, 2].Value = "Count";
            ws2.Cells[1, 1].Style.Font.Bold = true;
            ws2.Cells[1, 2].Style.Font.Bold = true;

            // Create chart
            var chart = (ExcelChart)ws2.Drawings.AddChart("rptChart", chartType);
            chart.Title.Text = chartTitle;
            chart.Title.Font.Bold = true;
            chart.SetPosition(1, 0, 3, 0);
            chart.SetSize(700, 400);

            int dataRows = chartData.Labels.Length;
            var series = chart.Series.Add(
                ws2.Cells[2, 2, dataRows + 1, 2],
                ws2.Cells[2, 1, dataRows + 1, 1]);
            series.Header = chartTitle;

            chart.Legend.Position = eLegendPosition.Bottom;
            chart.Style = eChartStyle.Style26;

            // Sheet 2 header
            ws2.Cells["A1"].Style.Font.Bold = true;

            return pkg;
        }

        // ── Chart data helpers ────────────────────────────────
        private (string[] Labels, double[] Values) BuildBorrowChartData(DataTable dt)
        {
            var dict = new System.Collections.Generic.Dictionary<string, int>
            { {"borrowed",0},{"returned",0},{"overdue",0} };
            foreach (DataRow row in dt.Rows)
            {
                var s = row["Status"]?.ToString() ?? "";
                if (dict.ContainsKey(s)) dict[s]++;
                else dict[s] = 1;
            }
            var keys = new string[dict.Count]; var vals = new double[dict.Count]; int i = 0;
            foreach (var kv in dict) { keys[i] = kv.Key; vals[i] = kv.Value; i++; }
            return (keys, vals);
        }

        private (string[] Labels, double[] Values) BuildStatusChartData(DataTable dt, string colName)
        {
            var dict = new System.Collections.Generic.Dictionary<string, int>();
            foreach (DataRow row in dt.Rows)
            {
                var s = row[colName]?.ToString() ?? "unknown";
                if (dict.ContainsKey(s)) dict[s]++;
                else dict[s] = 1;
            }
            var keys = new string[dict.Count]; var vals = new double[dict.Count]; int i = 0;
            foreach (var kv in dict) { keys[i] = kv.Key; vals[i] = kv.Value; i++; }
            return (keys, vals);
        }

        // ── Helpers ───────────────────────────────────────────
        private static MySqlParameter DateParam(string name, DateTime value)
            => new MySqlParameter(name, MySqlDbType.Date) { Value = value };

        private static void NoData() =>
            MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
