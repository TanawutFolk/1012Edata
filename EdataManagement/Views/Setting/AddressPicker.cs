using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// A box to pick one text of a list - an e-mail address, a customer: type any part of it and one list under the box narrows as you type
    /// (a ComboBox with autocomplete opens two lists on top of each other and only matches the start).
    ///   Up / Down move, Enter / click pick, Esc closes, the arrow shows every address
    ///   each row can carry a note on the right (InfoOf) and be greyed (DimOf)
    ///   AllowNew: what is typed and looks like an address that is not in the list is offered as a new one (last row)
    ///   FocusLeft: the box was left - the owner may put back the text of what is really selected
    /// Chosen(address, fromList): fromList = picked in the list; false = Enter on what is typed, the list closed.
    /// The list is a child of the form, not of this control, so it can lie over the neighbours; it opens upwards
    /// when there is no room below.
    /// </summary>
    internal class AddressPicker : UserControl
    {
        private const int ROW_HEIGHT = 28;
        private const int MAX_ROWS = 10;
        private const int ARROW_WIDTH = 24;
        private const int EM_SETCUEBANNER = 0x1501;
        private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        /// <summary>Last row: what is typed, offered as an address that is not in the list yet.</summary>
        private sealed class NewAddress
        {
            public string Address;
        }

        /// <summary>The only row when nothing matches; cannot be picked.</summary>
        private static readonly object NOTHING = new object();

        private readonly TextBox _box = new TextBox();
        private readonly Label _arrow = new Label();
        private readonly ListBox _list = new ListBox();
        private readonly Font _font = new Font(SettingUi.FONT_NAME, 9.75F, FontStyle.Regular);
        private readonly Font _small = new Font(SettingUi.FONT_NAME, 8.25F, FontStyle.Regular);

        private List<string> _items = new List<string>();
        private string _placeholder = "";
        private bool _setting;   // the text is being put in by code, not typed

        /// <summary>Note on the right of a row (e.g. "5 group"); null = none.</summary>
        public Func<string, string> InfoOf;

        /// <summary>True = the row is greyed.</summary>
        public Func<string, bool> DimOf;

        /// <summary>False = only what is in the list can be picked (no "new address" row).</summary>
        public bool AllowNew = true;

        /// <summary>The row shown when nothing matches what is typed.</summary>
        public string NothingText = "ไม่พบอีเมลที่ตรงกับที่พิมพ์";

        public event Action<string, bool> Chosen;
        public event Action FocusLeft;

        public AddressPicker()
        {
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Height = 28;

            _box.BorderStyle = BorderStyle.None;
            _box.Font = _font;
            _box.ForeColor = SettingUi.GRID_TEXT;
            _box.TextChanged += delegate
            {
                if (_setting) return;
                Fill(false);
                OpenList();
            };
            _box.LostFocus += delegate { CloseWhenFocusLeft(); };
            _box.HandleCreated += delegate { ApplyPlaceholder(); };
            Controls.Add(_box);

            _arrow.Text = "▾";
            _arrow.Dock = DockStyle.Right;
            _arrow.Width = ARROW_WIDTH;
            _arrow.TextAlign = ContentAlignment.MiddleCenter;
            _arrow.ForeColor = SettingUi.TYPE_CC_TEXT;
            _arrow.Cursor = Cursors.Hand;
            _arrow.Click += delegate
            {
                if (_list.Visible)
                {
                    CloseList();
                }
                else
                {
                    Fill(true);
                    OpenList();
                }
                _box.Focus();
            };
            Controls.Add(_arrow);

            _list.Visible = false;
            _list.TabStop = false;
            _list.IntegralHeight = false;
            _list.BorderStyle = BorderStyle.FixedSingle;
            _list.DrawMode = DrawMode.OwnerDrawFixed;
            _list.ItemHeight = ROW_HEIGHT;
            _list.Font = _font;
            _list.Cursor = Cursors.Hand;
            _list.DrawItem += list_DrawItem;
            _list.MouseMove += delegate (object s, MouseEventArgs e)
            {
                int index = _list.IndexFromPoint(e.Location);
                if (index >= 0 && index != _list.SelectedIndex) _list.SelectedIndex = index;
            };
            _list.MouseDown += delegate (object s, MouseEventArgs e)
            {
                Pick(_list.IndexFromPoint(e.Location));
                _box.Focus();
            };
            _list.LostFocus += delegate { CloseWhenFocusLeft(); };

            PlaceBox();
        }

        /// <summary>The text in the box. Setting it does not open the list.</summary>
        public string Value
        {
            get { return _box.Text; }
            set
            {
                _setting = true;
                _box.Text = value ?? "";
                _box.SelectionStart = _box.Text.Length;
                _setting = false;
            }
        }

        /// <summary>Grey text shown while the box is empty.</summary>
        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value ?? "";
                ApplyPlaceholder();
            }
        }

        public void SetItems(IEnumerable<string> addresses)
        {
            _items = addresses.ToList();
            if (_list.Visible) Fill(false);
        }

        /// <summary>Size of the text in the box (the list keeps its own).</summary>
        public void SetBoxFontSize(float size)
        {
            _box.Font = new Font(SettingUi.FONT_NAME, size, FontStyle.Regular);
            PlaceBox();
        }

        public void FocusBox()
        {
            _box.Focus();
        }

        // ---- layout ----

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PlaceBox();
            CloseList();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            BackColor = Enabled ? Color.White : SettingUi.ALERT_NONE_BACK;
            _box.BackColor = BackColor;
            if (!Enabled) CloseList();
        }

        private void PlaceBox()
        {
            _box.SetBounds(6, Math.Max(1, (ClientSize.Height - _box.PreferredHeight) / 2), Math.Max(20, ClientSize.Width - ARROW_WIDTH - 8), _box.PreferredHeight);
        }

        private void ApplyPlaceholder()
        {
            if (_box.IsHandleCreated) SendMessage(_box.Handle, EM_SETCUEBANNER, IntPtr.Zero, _placeholder);
        }

        // ---- keys: handled here, before the form takes Enter / Esc for its own buttons ----

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Down:
                case Keys.Up:
                    if (!_list.Visible)
                    {
                        Fill(_box.Text.Trim() == "");
                        OpenList();
                    }
                    else if (_list.Items.Count > 0)
                    {
                        int next = _list.SelectedIndex + (keyData == Keys.Down ? 1 : -1);
                        _list.SelectedIndex = Math.Max(0, Math.Min(_list.Items.Count - 1, next));
                    }
                    return true;

                case Keys.Enter:
                    if (_list.Visible && _list.SelectedIndex >= 0 && _list.Items[_list.SelectedIndex] != NOTHING)
                    {
                        Pick(_list.SelectedIndex);
                    }
                    else
                    {
                        CloseList();
                        Raise(_box.Text.Trim(), false);
                    }
                    return true;

                case Keys.Escape:
                    if (!_list.Visible) break;   // closed: Esc belongs to the form
                    CloseList();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ---- the list ----

        /// <summary>Rows for what is typed: addresses starting with it, then the ones containing it; all = every address.</summary>
        private void Fill(bool all)
        {
            string typed = _box.Text.Trim();

            List<string> rows = all || typed == ""
                ? _items
                : _items.Where(a => a.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
                    .Concat(_items.Where(a => !a.StartsWith(typed, StringComparison.OrdinalIgnoreCase) &&
                                              a.IndexOf(typed, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();

            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (string row in rows) _list.Items.Add(row);

            if (AllowNew && Regex.IsMatch(typed, EMAIL_PATTERN) &&
                !_items.Any(a => string.Equals(a, typed, StringComparison.OrdinalIgnoreCase)))
            {
                _list.Items.Add(new NewAddress { Address = typed });
            }

            if (_list.Items.Count == 0) _list.Items.Add(NOTHING);
            _list.EndUpdate();

            // the address in the box, else the first row
            int index = rows.FindIndex(a => string.Equals(a, typed, StringComparison.OrdinalIgnoreCase));
            _list.SelectedIndex = _list.Items[0] == NOTHING ? -1 : Math.Max(0, index);

            if (_list.Visible) PlaceList();
        }

        private void OpenList()
        {
            Form form = FindForm();
            if (form == null || !Enabled) return;

            if (_list.Parent != form) form.Controls.Add(_list);

            PlaceList();
            _list.Visible = true;
            _list.BringToFront();
        }

        /// <summary>Under the box, as wide as it; above it when the form has no room below.</summary>
        private void PlaceList()
        {
            Form form = FindForm();
            if (form == null) return;

            int height = Math.Min(_list.Items.Count, MAX_ROWS) * ROW_HEIGHT + 2;
            Point below = form.PointToClient(PointToScreen(new Point(-1, Height - 1)));
            Point above = form.PointToClient(PointToScreen(new Point(-1, -height - 1)));

            bool fitsBelow = below.Y + height <= form.ClientSize.Height;
            _list.SetBounds(below.X, fitsBelow || above.Y < 0 ? below.Y : above.Y, Width, height);
        }

        private void CloseList()
        {
            _list.Visible = false;
        }

        /// <summary>Focus went somewhere: close, unless it only moved between the box and the list.</summary>
        private void CloseWhenFocusLeft()
        {
            if (!IsHandleCreated || IsDisposed) return;

            BeginInvoke((Action)delegate
            {
                if (_box.Focused || _list.Focused) return;

                CloseList();
                Action handler = FocusLeft;
                if (handler != null) handler();
            });
        }

        private void Pick(int index)
        {
            if (index < 0 || index >= _list.Items.Count) return;

            object item = _list.Items[index];
            if (item == NOTHING) return;

            NewAddress added = item as NewAddress;
            string address = added != null ? added.Address : (string)item;

            Value = address;
            CloseList();
            Raise(address, true);
        }

        private void Raise(string address, bool fromList)
        {
            Action<string, bool> handler = Chosen;
            if (handler != null) handler(address, fromList);
        }

        private void list_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            object item = _list.Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) != 0 && item != NOTHING;

            using (SolidBrush back = new SolidBrush(selected ? SettingUi.GRID_SELECT_BACK : Color.White))
            {
                e.Graphics.FillRectangle(back, e.Bounds);
            }

            Rectangle text = new Rectangle(e.Bounds.Left + 8, e.Bounds.Top, e.Bounds.Width - 16, e.Bounds.Height);
            const TextFormatFlags LEFT = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            const TextFormatFlags RIGHT = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;

            if (item == NOTHING)
            {
                TextRenderer.DrawText(e.Graphics, NothingText, _font, text, SettingUi.TYPE_CC_TEXT, LEFT);
                return;
            }

            NewAddress added = item as NewAddress;
            if (added != null)
            {
                TextRenderer.DrawText(e.Graphics, "+ ใช้ \"" + added.Address + "\" เป็นอีเมลใหม่", _font, text, SettingUi.TAB_ACTIVE_BACK, LEFT);
                return;
            }

            string address = (string)item;
            string info = InfoOf == null ? null : InfoOf(address);
            bool dim = DimOf != null && DimOf(address);

            if (!string.IsNullOrEmpty(info))
            {
                int width = TextRenderer.MeasureText(e.Graphics, info, _small).Width + 4;
                TextRenderer.DrawText(e.Graphics, info, _small, text, SettingUi.TYPE_CC_TEXT, RIGHT);
                text.Width -= width;
            }

            TextRenderer.DrawText(e.Graphics, address, _font, text, dim ? SettingUi.TYPE_CC_TEXT : SettingUi.GRID_TEXT, LEFT);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _list.Dispose();
                _font.Dispose();
                _small.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
