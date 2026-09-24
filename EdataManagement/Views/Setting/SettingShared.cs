using System;
using System.Drawing;
using System.Windows.Forms;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// A page of frmSetting (a UserControl shown inside panelUCSetting).
    /// frmSetting owns the header, the tab buttons and the Save / Cancel footer;
    /// the page owns everything in between and tells the footer what to do.
    /// </summary>
    internal interface ISettingPage
    {
        /// <summary>Text for lbChangeForbtn in the header.</summary>
        string PageTitle { get; }

        /// <summary>Caption of the footer Save button, e.g. "Save (3)" or "Save *".</summary>
        string SaveButtonText { get; }

        /// <summary>Raised when SaveButtonText changes, so the footer can repaint.</summary>
        event EventHandler SaveButtonTextChanged;

        /// <summary>True while the page holds edits that are not written yet.</summary>
        bool HasPendingChanges { get; }

        /// <summary>Called every time the page is put on screen (first call loads the data).</summary>
        void PageShown();

        void SavePage();

        /// <summary>Discard the pending edits. False when there was nothing to discard (the form then closes).</summary>
        bool CancelPage();

        /// <summary>Called before another tab opens; false keeps the user on this page.</summary>
        bool ConfirmLeavePage();
    }

    /// <summary>Colors, grid look and signed-in user shared by the three Setting pages.</summary>
    internal static class SettingUi
    {
        // ---- alert bar ----
        public static readonly Color ALERT_WARN_BACK = Color.FromArgb(254, 243, 199);
        public static readonly Color ALERT_WARN_TEXT = Color.FromArgb(146, 64, 14);
        public static readonly Color ALERT_OK_BACK = Color.FromArgb(220, 252, 231);
        public static readonly Color ALERT_OK_TEXT = Color.FromArgb(22, 101, 52);
        public static readonly Color ALERT_INFO_BACK = Color.FromArgb(239, 246, 255);
        public static readonly Color ALERT_INFO_TEXT = Color.FromArgb(30, 64, 175);
        public static readonly Color ALERT_NONE_BACK = Color.FromArgb(243, 244, 246);
        public static readonly Color ALERT_NONE_TEXT = Color.FromArgb(107, 114, 128);

        // ---- tabs / buttons ----
        public static readonly Color TAB_ACTIVE_BACK = Color.FromArgb(60, 90, 229);
        public static readonly Color TAB_ACTIVE_TEXT = Color.White;
        public static readonly Color BUTTON_BACK = Color.FromArgb(60, 90, 229);

        // ---- grid ----
        public static readonly Color GRID_HEADER_BACK = Color.FromArgb(46, 139, 87);
        public static readonly Color GRID_HEADER_TEXT = Color.White;
        public static readonly Color GRID_TEXT = Color.FromArgb(17, 24, 39);
        public static readonly Color GRID_LINE = Color.FromArgb(235, 237, 240);
        public static readonly Color GRID_ALT_ROW = Color.FromArgb(249, 250, 251);
        public static readonly Color GRID_SELECT_BACK = Color.FromArgb(232, 240, 254);
        public static readonly Color TYPE_TO_TEXT = Color.FromArgb(29, 78, 216);
        public static readonly Color TYPE_CC_TEXT = Color.FromArgb(107, 114, 128);
        public static readonly Color REMOVE_TEXT = Color.FromArgb(220, 38, 38);
        public static readonly Color NG_TEXT = Color.FromArgb(185, 28, 28);

        public const string FONT_NAME = "Century Gothic";

        /// <summary>White paper, green header, thin horizontal lines - the look of every grid on this screen.</summary>
        public static void StyleGrid(DataGridView grid, int rowHeight = 34, float fontSize = 9.75F, int headerHeight = 38)
        {
            grid.AutoGenerateColumns = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToOrderColumns = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = GRID_LINE;

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = headerHeight;
            grid.ColumnHeadersDefaultCellStyle.BackColor = GRID_HEADER_BACK;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = GRID_HEADER_TEXT;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = GRID_HEADER_BACK;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = GRID_HEADER_TEXT;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(FONT_NAME, fontSize, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = GRID_TEXT;
            grid.DefaultCellStyle.SelectionBackColor = GRID_SELECT_BACK;
            grid.DefaultCellStyle.SelectionForeColor = GRID_TEXT;
            grid.DefaultCellStyle.Font = new Font(FONT_NAME, fontSize, FontStyle.Regular);
            grid.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = GRID_ALT_ROW;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = GRID_SELECT_BACK;

            grid.RowTemplate.Height = rowHeight;
            grid.RowTemplate.DividerHeight = 0;
        }

        public static void SetColumn(DataGridView grid, string name, string header, int weight)
        {
            if (!grid.Columns.Contains(name)) return;

            grid.Columns[name].HeaderText = header;
            grid.Columns[name].FillWeight = weight;
        }

        /// <summary>A flat text button column (Remove / Revoke ...) inside a grid.</summary>
        public static DataGridViewButtonColumn MakeButtonColumn(string name, string text, int weight, Color fore)
        {
            DataGridViewButtonColumn column = new DataGridViewButtonColumn();
            column.Name = name;
            column.HeaderText = "";
            column.Text = text;
            column.UseColumnTextForButtonValue = !string.IsNullOrEmpty(text);
            column.FillWeight = weight;
            column.FlatStyle = FlatStyle.Flat;
            column.DefaultCellStyle.ForeColor = fore;
            column.DefaultCellStyle.SelectionForeColor = fore;
            column.DefaultCellStyle.BackColor = Color.White;
            column.DefaultCellStyle.SelectionBackColor = GRID_SELECT_BACK;
            column.DefaultCellStyle.Font = new Font(FONT_NAME, 9F, FontStyle.Regular);
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.DefaultCellStyle.Padding = new Padding(4, 4, 4, 4);
            return column;
        }

        public static bool IsAdmin()
        {
            return frmLogin.CurrentUser != null && frmLogin.CurrentUser.isAdmin;
        }

        public static string CurrentUserCode()
        {
            return frmLogin.CurrentUser == null ? Environment.UserName : frmLogin.CurrentUser.empCode;
        }
    }
}
