using Bunifu.Framework.UI;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
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

    /// <summary>Colors, grid look and signed-in user shared by the Setting pages.</summary>
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

        // ---- highlight text (switched TO/CC, added address in the recipients grid) ----
        public static readonly Color TAB_ACTIVE_BACK = Color.FromArgb(60, 90, 229);
        public static readonly Color TAB_ACTIVE_TEXT = Color.White;

        // ---- buttons: the look of btnLoginn on frmLogin, used by every button of the Setting screen ----
        public static readonly Color BUTTON_BACK = Color.WhiteSmoke;
        public static readonly Color BUTTON_TEXT = Color.Black;
        public static readonly Color BUTTON_HOVER = Color.FromArgb(0, 192, 192);
        public static readonly Color BUTTON_ACTIVE = Color.DimGray;        // pressed, and the open tab
        public static readonly Color BUTTON_ACTIVE_TEXT = Color.White;
        public static readonly Color BUTTON_DISABLED = Color.Gray;
        public static readonly Color BUTTON_SHADOW = Color.FromArgb(35, 35, 35);
        public const float BUTTON_FONT_SIZE = 9.75F;

        // the button that writes (Save, the confirm button of frmConfirm): the old Save blue, so it stands apart from Cancel
        public static readonly Color BUTTON_PRIMARY = Color.FromArgb(60, 90, 229);
        public static readonly Color BUTTON_PRIMARY_TEXT = Color.White;

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

        /// <summary>btnLoginn's look (frmLogin): WhiteSmoke, black bold Century Gothic, teal hover, 1px dark shadow.</summary>
        public static void StyleButton(BunifuFlatButton button)
        {
            button.Normalcolor = BUTTON_BACK;
            button.BackColor = BUTTON_BACK;
            button.Activecolor = BUTTON_ACTIVE;
            button.OnHovercolor = BUTTON_HOVER;
            button.OnHoverTextColor = BUTTON_TEXT;
            button.Textcolor = BUTTON_TEXT;
            button.DisabledColor = BUTTON_DISABLED;
            button.BorderRadius = 0;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.TextFont = new Font(FONT_NAME, BUTTON_FONT_SIZE, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            AddShadow(button);
            ResetHoverOnReturn(button);
        }

        /// <summary>
        /// A button that opens a window over itself (Manage, Save -> confirm ...) never gets its MouseLeave - the mouse
        /// leaves over the new window - so Bunifu keeps the hover colour after that window closes. When the button's
        /// window is active again and the mouse is not on the button, the normal colour is put back.
        /// </summary>
        private static void ResetHoverOnReturn(BunifuFlatButton button)
        {
            Form hooked = null;

            EventHandler reset = delegate
            {
                if (button.IsDisposed || !button.IsHandleCreated) return;
                if (button.ClientRectangle.Contains(button.PointToClient(Cursor.Position))) return;
                if (button.BackColor != button.Normalcolor) button.BackColor = button.Normalcolor;
            };

            // the form is known only once the button sits in it
            EventHandler hook = delegate
            {
                Form form = button.FindForm();
                if (form == null || form == hooked) return;

                if (hooked != null) hooked.Activated -= reset;
                hooked = form;
                form.Activated += reset;
            };

            button.ParentChanged += hook;
            button.HandleCreated += hook;
            hook(button, EventArgs.Empty);
        }

        /// <summary>btnLoginn's look but blue with white text - for Save, so it is not mistaken for Cancel.</summary>
        public static void StylePrimaryButton(BunifuFlatButton button)
        {
            StyleButton(button);
            button.Normalcolor = BUTTON_PRIMARY;
            button.BackColor = BUTTON_PRIMARY;
            button.Textcolor = BUTTON_PRIMARY_TEXT;
            button.OnHoverTextColor = BUTTON_PRIMARY_TEXT;
        }

        /// <summary>The same look for a plain WinForms Button (the Close of the All Groups dialog).</summary>
        public static void StyleButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = BUTTON_HOVER;
            button.FlatAppearance.MouseDownBackColor = BUTTON_ACTIVE;
            button.BackColor = BUTTON_BACK;
            button.ForeColor = BUTTON_TEXT;
            button.Font = new Font(FONT_NAME, BUTTON_FONT_SIZE, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            AddShadow(button);
        }

        /// <summary>
        /// Grey a styled BunifuFlatButton. Its Enabled hides Control.Enabled and only repaints - the click still
        /// fires (disabling the base control draws the caption in the wrong place), so every click handler
        /// checks for itself that it may act.
        /// </summary>
        public static void SetButtonEnabled(BunifuFlatButton button, bool enabled)
        {
            button.Enabled = enabled;
            button.BackColor = enabled ? BUTTON_BACK : BUTTON_DISABLED;
            button.Normalcolor = button.BackColor;
        }

        /// <summary>Tab button: DimGray + white text when it is the open page, otherwise the normal button look.</summary>
        public static void PaintTab(BunifuFlatButton button, bool active)
        {
            button.selected = active;
            button.Normalcolor = active ? BUTTON_ACTIVE : BUTTON_BACK;
            button.BackColor = button.Normalcolor;
            button.Textcolor = active ? BUTTON_ACTIVE_TEXT : BUTTON_TEXT;
        }

        /// <summary>
        /// A dark panel 1px right / down, right behind the button (like frmLogin.AddButtonShadow); it follows the
        /// button's place, size and visibility, so buttons shown / hidden by the pages keep it right.
        /// </summary>
        private static void AddShadow(Control button)
        {
            Panel shadow = new Panel { BackColor = BUTTON_SHADOW, Enabled = false, Visible = false };

            EventHandler sync = delegate
            {
                Control parent = button.Parent;
                if (parent == null) return;

                if (shadow.Parent != parent) parent.Controls.Add(shadow);
                shadow.Anchor = button.Anchor;
                shadow.Bounds = new Rectangle(button.Left + 1, button.Top + 1, button.Width, button.Height);
                shadow.Visible = button.Visible;
                parent.Controls.SetChildIndex(shadow, parent.Controls.GetChildIndex(button) + 1);
            };

            button.LocationChanged += sync;
            button.SizeChanged += sync;
            button.VisibleChanged += sync;
            button.ParentChanged += sync;
            sync(button, EventArgs.Empty);
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

    /// <summary>
    /// "กำลังโหลด..." over a window while slow work runs, so the window never sits frozen: the Setting pages
    /// (database calls), Select on the main screen (file preparation) and Send (Outlook).
    ///   Run     the work runs on a pool thread; it must not touch a control (read the screen before, show the result after)
    ///   RunSta  the same on a thread of its own in a single-threaded apartment - for Outlook / Office COM
    ///   Progress  for work that has to stay on the UI thread (it shows message boxes, fills grids): call it between
    ///           steps; it updates the text and keeps the window painted
    ///   StartCount / NextCount  "กำลังส่งเมล 2/3": a counted job, NextCount moves it on
    ///   * nothing is shown for the first SHOW_AFTER_MS - a quick job does not flash
    ///   * the sheet covers the whole window and takes the keyboard, so nothing can be clicked twice
    ///   * Begin ... Dispose keeps one sheet up over several calls (a Save and the reload after it, every mail of a Send)
    ///   * frmError / frmConfirm call Pause first, so a message is never hidden under the sheet
    /// UI thread only.
    /// </summary>
    internal static class SettingBusy
    {
        public const string LOADING = "กำลังโหลด...";
        public const string SAVING = "กำลังบันทึก...";
        public const string PREPARING = "กำลังเตรียมไฟล์...";
        public const string SENDING = "กำลังส่งเมล...";

        private const int SHOW_AFTER_MS = 300;

        /// <summary>A RunSta job still running after this long gets its slow hint under the text.</summary>
        private const int SLOW_AFTER_MS = 30000;

        private static Scope _current;

        /// <summary>True while a Begin is open (a sheet is up or may come up).</summary>
        public static bool IsActive
        {
            get { return _current != null; }
        }

        /// <summary>
        /// One sheet for everything until Dispose. Inside another Begin nothing new opens: text replaces the outer text,
        /// null keeps it.
        /// </summary>
        public static IDisposable Begin(Control owner, string text)
        {
            if (_current != null)
            {
                if (text != null) _current.SetText(text);
                return new Nested();
            }

            _current = new Scope(owner, text ?? LOADING);
            return _current;
        }

        public static T Run<T>(Control owner, string text, Func<T> work)
        {
            using (Begin(owner, text))
            {
                Task<T> task = Task.Run(work);
                _current.WaitFor(task, null);
                return ResultOf(task);
            }
        }

        /// <summary>
        /// Run on a new single-threaded-apartment thread (Outlook / Office COM must be driven from one STA thread).
        /// slowHint: added under the text when the job is still running after SLOW_AFTER_MS (null = none).
        /// </summary>
        public static T RunSta<T>(Control owner, string text, Func<T> work, string slowHint)
        {
            using (Begin(owner, text))
            {
                TaskCompletionSource<T> done = new TaskCompletionSource<T>();
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        done.SetResult(work());
                    }
                    catch (Exception ex)
                    {
                        done.SetException(ex);
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();

                _current.WaitFor(done.Task, slowHint);
                return ResultOf(done.Task);
            }
        }

        /// <summary>The result of a finished job; the caller sees what the work threw, not the wrapper.</summary>
        private static T ResultOf<T>(Task<T> task)
        {
            try
            {
                return task.Result;
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
                throw;
            }
        }

        /// <summary>Change the text of the open sheet (no Begin open = nothing happens).</summary>
        public static void SetText(string text)
        {
            if (_current != null && text != null) _current.SetText(text);
        }

        /// <summary>
        /// Work on the UI thread, between two steps: show text (null = keep), put the sheet up once the job has taken
        /// SHOW_AFTER_MS, and let the window paint. No Begin open = nothing happens.
        /// </summary>
        public static void Progress(string text)
        {
            if (_current != null) _current.Progress(text);
        }

        /// <summary>A counted job, e.g. format "กำลังส่งเมล {0}/{1}"; NextCount shows 1/total, 2/total ...</summary>
        public static void StartCount(string format, int total)
        {
            if (_current != null) _current.StartCount(format, total);
        }

        public static void NextCount()
        {
            if (_current != null) _current.NextCount();
        }

        /// <summary>Take the sheet away for a message box; the next Run puts it back if it is still slow.</summary>
        public static void Pause()
        {
            if (_current != null) _current.Hide();
        }

        private sealed class Nested : IDisposable
        {
            public void Dispose() { }
        }

        private sealed class Scope : IDisposable
        {
            private readonly Control _owner;
            private readonly Stopwatch _clock = Stopwatch.StartNew();
            private string _text;
            private string _hint = "";
            private string _countFormat;
            private int _countTotal;
            private int _countDone;
            private Form _sheet;
            private Label _label;

            public Scope(Control owner, string text)
            {
                _owner = owner;
                _text = text;
            }

            public void SetText(string text)
            {
                _text = text;
                _hint = "";
                if (_label != null) _label.Text = text;
            }

            public void StartCount(string format, int total)
            {
                _countFormat = format;
                _countTotal = total;
                _countDone = 0;
            }

            public void NextCount()
            {
                if (_countFormat == null) return;

                _countDone++;
                SetText(string.Format(_countFormat, _countDone, _countTotal));
            }

            public void Progress(string text)
            {
                if (text != null) SetText(text);

                if (_sheet == null && _clock.ElapsedMilliseconds >= SHOW_AFTER_MS) Show();
                if (_sheet != null) Application.DoEvents();
            }

            public void WaitFor(Task task, string slowHint)
            {
                // not shown yet: wait without handling input, so nothing else can be clicked in the gap
                int left = SHOW_AFTER_MS - (int)_clock.ElapsedMilliseconds;
                if (_sheet == null && left > 0 && ((IAsyncResult)task).AsyncWaitHandle.WaitOne(left)) return;

                Show();
                Stopwatch waited = Stopwatch.StartNew();
                while (!task.IsCompleted)
                {
                    if (slowHint != null && _hint == "" && waited.ElapsedMilliseconds >= SLOW_AFTER_MS)
                    {
                        _hint = slowHint;
                        if (_label != null) _label.Text = _text + Environment.NewLine + _hint;
                    }

                    Application.DoEvents();
                    Thread.Sleep(15);
                }
            }

            private void Show()
            {
                if (_sheet != null) return;

                Form window = _owner == null ? null : _owner.FindForm();
                bool cover = window != null && window.Visible && window.WindowState != FormWindowState.Minimized;

                _sheet = new Form();
                _sheet.FormBorderStyle = FormBorderStyle.None;
                _sheet.ShowInTaskbar = false;
                _sheet.StartPosition = FormStartPosition.Manual;
                _sheet.BackColor = Color.White;
                _sheet.Opacity = cover ? 0.88 : 1;
                _sheet.Cursor = Cursors.WaitCursor;

                if (cover)
                {
                    _sheet.Bounds = window.Bounds;
                }
                else
                {
                    // the window is not on screen yet (first load): a box in the middle of the screen
                    Rectangle screen = Screen.FromPoint(Cursor.Position).WorkingArea;
                    _sheet.Bounds = new Rectangle(screen.Left + (screen.Width - 460) / 2, screen.Top + (screen.Height - 150) / 2, 460, 150);
                }

                // two lines: the text, and the slow hint or the second line of a long text
                _label = new Label();
                _label.AutoSize = false;
                _label.AutoEllipsis = true;
                _label.Text = _hint == "" ? _text : _text + Environment.NewLine + _hint;
                _label.Font = new Font(SettingUi.FONT_NAME, 13F, FontStyle.Bold);
                _label.ForeColor = SettingUi.GRID_TEXT;
                _label.TextAlign = ContentAlignment.BottomCenter;
                _label.SetBounds(16, _sheet.Height / 2 - 66, _sheet.Width - 32, 58);

                ProgressBar bar = new ProgressBar();
                bar.Style = ProgressBarStyle.Marquee;
                bar.MarqueeAnimationSpeed = 25;
                bar.SetBounds((_sheet.Width - 260) / 2, _sheet.Height / 2 + 2, 260, 10);

                _sheet.Controls.Add(_label);
                _sheet.Controls.Add(bar);

                if (cover) _sheet.Show(window);
                else _sheet.Show();
                _sheet.Update();
            }

            public void Hide()
            {
                if (_sheet == null) return;

                Form window = _sheet.Owner;
                _sheet.Dispose();
                _sheet = null;
                _label = null;
                _clock.Restart();

                if (window != null && window.Visible) window.Activate();
            }

            public void Dispose()
            {
                Hide();
                if (_current == this) _current = null;
            }
        }
    }
}