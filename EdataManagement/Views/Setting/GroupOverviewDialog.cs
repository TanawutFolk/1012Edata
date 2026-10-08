using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// "All Groups" window: every product + customer that sends by Email, its group and who is on it.
    /// Opened by the All Groups button of ucSpecSetting.
    /// </summary>
    internal static class GroupOverviewDialog
    {
        /// <summary>
        /// One table of every product + customer: the group it is bound to and who receives the mail.
        /// </summary>
        public static void Show(IWin32Window owner, DataTable matrix)
        {
            DataView view = new DataView(matrix);

            using (Form dialog = new Form())
            {
                dialog.Text = "Group Overview - every product / customer";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ClientSize = new Size(1180, 620);
                dialog.MinimumSize = new Size(820, 420);
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.White;
                dialog.Font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Regular);

                // ---- top bar: search ----
                Panel top = new Panel();
                top.Dock = DockStyle.Top;
                top.Height = 58;
                top.BackColor = Color.White;
                top.Padding = new Padding(16, 12, 16, 8);

                Label lbFind = new Label();
                lbFind.Text = "Search";
                lbFind.AutoSize = true;
                lbFind.Location = new Point(16, 20);
                lbFind.ForeColor = SettingUi.GRID_TEXT;

                TextBox txtFind = new TextBox();
                txtFind.Location = new Point(76, 16);
                txtFind.Size = new Size(320, 26);
                txtFind.Font = new Font(SettingUi.FONT_NAME, 10F, FontStyle.Regular);

                Label lbCount = new Label();
                lbCount.AutoSize = true;
                lbCount.Location = new Point(412, 20);
                lbCount.ForeColor = SettingUi.TYPE_CC_TEXT;
                lbCount.Text = matrix.Rows.Count + " row(s)";

                top.Controls.Add(lbCount);
                top.Controls.Add(txtFind);
                top.Controls.Add(lbFind);

                // ---- grid ----
                DataGridView grid = new DataGridView();
                grid.Dock = DockStyle.Fill;
                SettingUi.StyleGrid(grid);
                grid.DataSource = view;

                grid.DataBindingComplete += delegate
                {
                    FormatMatrixGrid(grid);
                };

                // TO in blue, CC in grey, same language as the member grid
                grid.CellFormatting += delegate (object s2, DataGridViewCellFormattingEventArgs e2)
                {
                    if (e2.RowIndex < 0) return;
                    string column = grid.Columns[e2.ColumnIndex].Name;

                    if (column == "TO_LIST" || column == "TO_COUNT")
                    {
                        e2.CellStyle.ForeColor = SettingUi.TYPE_TO_TEXT;
                        e2.CellStyle.SelectionForeColor = SettingUi.TYPE_TO_TEXT;
                    }
                    else if (column == "CC_LIST" || column == "CC_COUNT")
                    {
                        e2.CellStyle.ForeColor = SettingUi.TYPE_CC_TEXT;
                        e2.CellStyle.SelectionForeColor = SettingUi.TYPE_CC_TEXT;
                    }
                    else if (column == "GROUP_ID")
                    {
                        e2.Value = "Group #" + Convert.ToString(e2.Value);
                        e2.CellStyle.Font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Bold);
                        e2.FormattingApplied = true;
                    }
                };

                // full address list on hover - the column is too narrow for 10 addresses
                grid.CellMouseEnter += delegate (object s3, DataGridViewCellEventArgs e3)
                {
                    if (e3.RowIndex < 0 || e3.ColumnIndex < 0) return;
                    DataGridViewCell cell = grid.Rows[e3.RowIndex].Cells[e3.ColumnIndex];
                    cell.ToolTipText = Convert.ToString(cell.Value);
                };

                // ---- search filter ----
                txtFind.TextChanged += delegate
                {
                    string keyword = txtFind.Text.Trim().Replace("'", "''");

                    view.RowFilter = keyword.Length == 0
                        ? ""
                        : "PRODUCT LIKE '%" + keyword + "%' OR CUSTOMER_NAME LIKE '%" + keyword + "%'"
                          + " OR TO_LIST LIKE '%" + keyword + "%' OR CC_LIST LIKE '%" + keyword + "%'"
                          + " OR CONVERT(GROUP_ID, 'System.String') LIKE '%" + keyword + "%'";

                    lbCount.Text = view.Count + " row(s)"
                        + (keyword.Length == 0 ? "" : " of " + matrix.Rows.Count);
                };

                // ---- bottom bar ----
                Panel bottom = new Panel();
                bottom.Dock = DockStyle.Bottom;
                bottom.Height = 56;
                bottom.BackColor = Color.White;

                Label lbHint = new Label();
                lbHint.AutoSize = true;
                lbHint.Location = new Point(16, 20);
                lbHint.ForeColor = SettingUi.TYPE_CC_TEXT;
                lbHint.Text = "Only customers whose Send Method is Email are listed. Hover a cell to read the full address list.";

                Button close = new Button();
                close.Text = "Close";
                close.Size = new Size(110, 32);
                close.Location = new Point(dialog.ClientSize.Width - 130, 12);
                close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                close.FlatStyle = FlatStyle.Flat;
                close.BackColor = Color.White;
                close.Click += delegate { dialog.Close(); };

                bottom.Controls.Add(close);
                bottom.Controls.Add(lbHint);

                dialog.Controls.Add(grid);
                dialog.Controls.Add(top);
                dialog.Controls.Add(bottom);
                dialog.AcceptButton = close;

                dialog.ShowDialog(owner);
            }
        }

        private static void FormatMatrixGrid(DataGridView grid)
        {
            if (grid.Columns.Count == 0) return;

            SettingUi.SetColumn(grid, "PRODUCT", "PRODUCT", 70);
            SettingUi.SetColumn(grid, "CUSTOMER_NAME", "CUSTOMER", 150);
            SettingUi.SetColumn(grid, "GROUP_ID", "GROUP", 80);
            SettingUi.SetColumn(grid, "SPEC_COUNT", "SPECS", 60);
            SettingUi.SetColumn(grid, "TO_COUNT", "TO", 45);
            SettingUi.SetColumn(grid, "CC_COUNT", "CC", 45);
            SettingUi.SetColumn(grid, "TO_LIST", "TO (recipients)", 250);
            SettingUi.SetColumn(grid, "CC_LIST", "CC (recipients)", 250);

            if (grid.Columns.Contains("SPEC_COUNT"))
            {
                grid.Columns["SPEC_COUNT"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }
}
