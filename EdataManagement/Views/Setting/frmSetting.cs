using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Bunifu.Framework.UI;
using EdataManagement.Utilities;

namespace EdataManagement.Views.Setting
{
    /// <summary>
    /// Shell of the Setting screen: header with the tab buttons, the Save / Cancel footer,
    /// and panelUCSetting in between where the page of the chosen tab is shown.
    ///
    ///   btnSettingSpec  -> ucSpecSetting        (send method / file / group / subject / body of each spec,
    ///                                            and the recipients of its mail)
    ///   btnSettingLogin -> ucLoginSetting       (who may open Setting, ADMIN only)
    ///
    /// A page is created the first time its tab is opened and then kept, so going back to a tab
    /// finds it as it was left.
    /// </summary>
    public partial class frmSetting : Form
    {
        private readonly Dictionary<BunifuFlatButton, UserControl> _pages = new Dictionary<BunifuFlatButton, UserControl>();
        private BunifuFlatButton _activeTab;
        private bool _loadingDatabase;

        public frmSetting()
        {
            InitializeComponent();
        }

        /// <summary>Every tab button, left to right.</summary>
        private BunifuFlatButton[] TabButtons
        {
            get { return new[] { btnSettingSpec, btnSettingLogin }; }
        }

        private ISettingPage ActivePage
        {
            get
            {
                UserControl page;
                if (_activeTab == null || !_pages.TryGetValue(_activeTab, out page)) return null;
                return page as ISettingPage;
            }
        }

        // =====================================================================
        // load
        // =====================================================================
        private void frmSetting_Load(object sender, EventArgs e)
        {
            ShowDatabaseInTitle();
            LoadDatabaseSelector();
            cbDatabase.SelectedIndexChanged += cbDatabase_SelectedIndexChanged;

            // every button looks like btnLoginn on frmLogin; Save keeps its blue so it stands apart from Cancel
            foreach (BunifuFlatButton button in new[] { btnSettingSpec, btnSettingLogin, btnCancel })
            {
                SettingUi.StyleButton(button);
            }
            SettingUi.StylePrimaryButton(btnSave);

            // a USER never sees the permission tab
            btnSettingLogin.Visible = SettingUi.IsAdmin();

            FormClosing += frmSetting_FormClosing;

            SelectTab(btnSettingSpec);
        }

        private void LoadDatabaseSelector()
        {
            _loadingDatabase = true;
            DataTable databases = new DataTable();
            databases.Columns.Add("Text", typeof(string));
            databases.Columns.Add("Value", typeof(string));
            databases.Rows.Add("TEST (qa_oc_test)", DatabaseSelection.TestDatabase);
            databases.Rows.Add("PRODUCTION (qa_oc)", DatabaseSelection.ProductionDatabase);

            cbDatabase.Font = new Font(cbDatabase.Font.FontFamily, 11f, FontStyle.Regular);
            cbDatabase.DisplayMember = "Text";
            cbDatabase.ValueMember = "Value";
            cbDatabase.DataSource = databases;
            cbDatabase.SelectedValue = DatabaseSelection.CurrentDatabase;
            _loadingDatabase = false;
        }

        private void cbDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingDatabase || cbDatabase.SelectedValue == null) return;

            string selected = cbDatabase.SelectedValue.ToString();
            if (string.Equals(selected, DatabaseSelection.CurrentDatabase, StringComparison.OrdinalIgnoreCase)) return;

            string warning = selected == DatabaseSelection.ProductionDatabase
                ? "เปลี่ยนเป็นฐานข้อมูลจริง qa_oc อีเมลผู้รับจริง\nต้องการเปลี่ยนหรือไม่?"
                : "เปลี่ยนเป็นฐานข้อมูลทดสอบ qa_oc_test อีเมลผู้รับอีเมลทดสอบ\nต้องการเปลี่ยนหรือไม่?";

            if (!frmConfirm.Ask(this, warning, "เปลี่ยนฐานข้อมูล", "ยกเลิก"))
            {
                _loadingDatabase = true;
                cbDatabase.SelectedValue = DatabaseSelection.CurrentDatabase;
                _loadingDatabase = false;
                return;
            }

            try
            {
                DatabaseSelection.SaveAndRestart(selected);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "ไม่สามารถบันทึกการเปลี่ยนฐานข้อมูลได้\n" + ex.Message,
                    "Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _loadingDatabase = true;
                cbDatabase.SelectedValue = DatabaseSelection.CurrentDatabase;
                _loadingDatabase = false;
            }
        }

        /// <summary>Warn on the caption when the app is pointed at the test database.</summary>
        private void ShowDatabaseInTitle()
        {
            try
            {
                string strConnection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder { ConnectionString = strConnection };
                string databaseName = builder["Database"].ToString();

                this.Text = "Setting - 1012 Send E-data   [DB: " + databaseName + "]";
            }
            catch
            {
                this.Text = "Setting - 1012 Send E-data";
            }
        }

        // =====================================================================
        // tab buttons
        // =====================================================================
        private void btnSettingSpec_Click(object sender, EventArgs e)
        {
            SelectTab(btnSettingSpec);
        }

        private void btnSettingLogin_Click(object sender, EventArgs e)
        {
            SelectTab(btnSettingLogin);
        }

        private void SelectTab(BunifuFlatButton tab)
        {
            if (tab == _activeTab) return;

            // only an ADMIN may open the permission page
            if (tab == btnSettingLogin && !SettingUi.IsAdmin())
            {
                frmError.ShowError(this, "Only an ADMIN can manage login permission.");
                return;
            }

            ISettingPage leaving = ActivePage;
            if (leaving != null && !leaving.ConfirmLeavePage()) return;

            UserControl page = GetPage(tab);

            foreach (UserControl other in _pages.Values)
            {
                if (other != page) other.Visible = false;
            }

            page.Visible = true;
            page.BringToFront();

            _activeTab = tab;

            foreach (BunifuFlatButton button in TabButtons)
            {
                SettingUi.PaintTab(button, button == tab);
            }

            ISettingPage settingPage = (ISettingPage)page;
            lbChangeForbtn.Text = settingPage.PageTitle;

            settingPage.PageShown();
            RefreshSaveButton();
        }

        /// <summary>Create the page of a tab the first time, then reuse it.</summary>
        private UserControl GetPage(BunifuFlatButton tab)
        {
            UserControl page;
            if (_pages.TryGetValue(tab, out page)) return page;

            if (tab == btnSettingLogin) page = new ucLoginSetting();
            else page = new ucSpecSetting();

            page.Dock = DockStyle.Fill;
            page.Visible = false;
            panelUCSetting.Controls.Add(page);

            ((ISettingPage)page).SaveButtonTextChanged += delegate (object sender, EventArgs e)
            {
                if (sender == page && tab == _activeTab) RefreshSaveButton();
            };

            _pages[tab] = page;
            return page;
        }

        private void RefreshSaveButton()
        {
            ISettingPage page = ActivePage;
            btnSave.ButtonText = page == null ? "Save" : page.SaveButtonText;
        }

        // =====================================================================
        // footer
        // =====================================================================
        private void btnSave_Click(object sender, EventArgs e)
        {
            ISettingPage page = ActivePage;
            if (page != null) page.SavePage();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ISettingPage page = ActivePage;

            // nothing to discard on this page -> Cancel closes the screen
            if (page == null || !page.CancelPage())
            {
                Close();
            }
        }

        /// <summary>Pages are kept alive, so edits left on another tab are checked here too.</summary>
        private void frmSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            int pending = _pages.Values.OfType<ISettingPage>().Count(p => p.HasPendingChanges);
            if (pending == 0) return;

            if (!frmConfirm.Ask(this,
                pending + " page(s) still have unsaved changes. Close and discard them?",
                "Close", "Keep editing"))
            {
                e.Cancel = true;
            }
        }
    }
}
