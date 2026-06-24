using EdataManagement.Controllers;
using EdataManagement.Property;
using EdataManagement.Utilities;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static EdataManagement.Property.EmailProperty;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace EdataManagement.Views
{
    public partial class emailUserControl : UserControl
    {
        EmailCls emailCls = new EmailCls();
        EmailProperty.replaceWord rpWord = new EmailProperty.replaceWord();
        EmailControllers _emailCon = new EmailControllers();

        private readonly frmMain mf;

        public DataGridView MainDataGridView()
        {
            DataGridView dgv = new DataGridView();
            if (this.ParentForm is frmMain mf)
            {
                dgv = mf.mainDataGridView;

            }
            return dgv;
        }

        public DataGridViewRow GetSelectedRow()
        {
            // ดึง DataGridView จาก MainForm
            DataGridView dgv = MainDataGridView();

            // ตรวจสอบว่ามี DataGridView และแถวถูกเลือก
            if (dgv != null && dgv.SelectedRows.Count > 0)
            {
                return dgv.SelectedRows[0]; // คืนค่าแถวที่ถูกเลือกแถวแรก
            }

            return null; // ไม่มีแถวถูกเลือก
        }

        public DataGridView GetDataGridViewDocs()
        {
            
            return dataGridViewDocs;
        }

        public BindingSource GetDataGridViewSL()
        {

            return bdSerialList;
        }


        public Label countOnLabel()
        {
 

            return lb_SerialShippingCount;


        }

        public BindingSource bdInfoEmail()
        {
            return bdEmail;
        }

        public BindingSource GetDataGridViewWF()
        {
            return bdWafer;
        }

        public emailUserControl(frmMain mf)
        {
            this.mf = mf;
            InitializeComponent();

            // ตั้งค่าเริ่มต้นให้ปุ่มส่งอีเมล์เป็น disabled
            bt_sendEmail.Enabled = false;

            // ผูก event ต่างๆ
            this.dataGridViewDocs.RowsAdded += DataGridViewDocs_RowsChanged;
            this.dataGridViewDocs.RowsRemoved += DataGridViewDocs_RowsChanged;
            this.dataGridViewDocs.DataBindingComplete += DataGridViewDocs_DataBindingComplete;

        }

        private void DataGridViewDocs_RowsChanged(object sender, EventArgs e)
        {
            RefreshSendEmailButtonState();
        }

        private void DataGridViewDocs_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            RefreshSendEmailButtonState();
        }

        private void RefreshSendEmailButtonState()
        {
            if (dataGridViewDocs == null || dataGridViewDocs.Rows == null || !dataGridViewDocs.Columns.Contains("attached"))
            {
                bt_sendEmail.Enabled = false;
                return;
            }

            bool hasData = dataGridViewDocs.Rows.Cast<DataGridViewRow>()
                .Any(row => !row.IsNewRow &&
                            row.Cells["attached"] != null &&
                            row.Cells["attached"].Value != null &&
                            !string.IsNullOrWhiteSpace(row.Cells["attached"].Value.ToString()));

            bt_sendEmail.Enabled = hasData;
        }
        private void emailUserControl_Load(object sender, EventArgs e)
        {
            RefreshSendEmailButtonState();
        }


        public void SetLabelNameText(EmailProperty prop)
        {
            rpWord.subject = _emailCon.getSubject(prop); ;
            rpWord.formatDate = _emailCon.allFormatDate();
            
            //lb_subject.Text = emailCls.strProcessSubject(rpWord);
        }

        private void label6_Click(object sender, EventArgs e)
        {
            
        }

        private void bt_sendEmail_Click(object sender, EventArgs e)
        {
            // Unsubscribe event ก่อน
            this.dataGridViewDocs.RowsAdded -= DataGridViewDocs_RowsChanged;
            this.dataGridViewDocs.RowsRemoved -= DataGridViewDocs_RowsChanged;
            this.dataGridViewDocs.DataBindingComplete -= DataGridViewDocs_DataBindingComplete;

            EmailProperty prop = new EmailProperty();
            EmailProperty.replaceWord rpWord = new EmailProperty.replaceWord();

            DataGridView dgView = ((DataGridView)bdEmail.DataSource);
            DataGridView dgslView = ((DataGridView)bdSerialList.DataSource);
            DataGridView dgwfVIew = ((DataGridView)bdWafer.DataSource);

            DataGridView dgvMain = MainDataGridView();

            DataTable dt = new DataTable();
            DataTable sldt = new DataTable();
            DataTable wfdt = new DataTable();

            dt = ((DataTable)dgView.DataSource);

            if (dgslView != null)
            {
                sldt = ((DataTable)dgslView.DataSource);
            }

            if (dgwfVIew != null)
            {
                wfdt = ((DataTable)dgwfVIew.DataSource);
            }

            if (dt != null)
            {
                prop.fileFormatsId = dt.Rows[0]["FILE_FORMATS_ID"].ToString();
            }

            if (prop.fileFormatsId == "4")
            {
                string successMessage = "";
                int Index = 0;
                int indexMain = 0;

                List<string> attachList = new List<string>();
                prop.dgvFile = dataGridViewDocs;

                // ตรวจสอบไฟล์แนบทั้งหมดก่อนส่ง
                List<string> missingFiles = new List<string>();
                foreach (DataGridViewRow dgvfilePath in prop.dgvFile.Rows)
                {
                    string filePath = dgvfilePath.Cells["attached"].Value?.ToString();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        missingFiles.Add("ไฟล์ว่าง (ไม่มี path)");
                    }
                    else if (!File.Exists(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                    else
                    {
                        attachList.Add(filePath);
                    }
                }

                if (missingFiles.Any())
                {
                    string errorMessage = "ไม่พบไฟล์แนบต่อไปนี้:\n" + string.Join("\n", missingFiles);
                    MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                foreach (DataGridViewRow dtRow in dgvMain.SelectedRows.Cast<DataGridViewRow>().OrderBy(r => r.Index))
                {
                    prop.attachmentFilePaths = new List<string>();

                    prop.toEmailAddresses = (List<string>)dt.Rows[Index]["toEmailAddresses"];
                    prop.ccEmailAddresses = (List<string>)dt.Rows[Index]["ccEmailAddresses"];

                    rpWord.subject = dt.Rows[Index]["subject"].ToString();
                    rpWord.body = dt.Rows[Index]["body"].ToString();

                    prop.productName = dt.Rows[Index]["productName"].ToString();
                    prop.key = dt.Rows[Index]["key_id"].ToString();

                    List<string> manuFactureFilter = attachList.Where(file =>
                        file.Contains(dtRow.Cells["manuFactureNo"].Value.ToString())).ToList();

                    if (manuFactureFilter.Any())
                    {
                        // ตรวจสอบไฟล์ที่ filter ได้อีกครั้ง
                        List<string> filteredMissingFiles = new List<string>();
                        foreach (string atf in manuFactureFilter)
                        {
                            if (File.Exists(atf))
                            {
                                prop.attachmentFilePaths.Add(atf);
                            }
                            else
                            {
                                filteredMissingFiles.Add(atf);
                            }
                        }

                        if (filteredMissingFiles.Any())
                        {
                            string errorMessage = $"ไม่พบไฟล์สำหรับ Order: {dtRow.Cells["manuFactureNo"].Value}\n" +
                                                 string.Join("\n", filteredMissingFiles);
                            MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            this.Parent.Controls.Remove(this);
                            mf.clearDataForm();
                            return;
                        }
                    }
                    else
                    {
                        string errorMessage = $"ไม่พบไฟล์ที่จะส่งของ Order: {dtRow.Cells["manuFactureNo"].Value}\n" +
                                             $"Product: {prop.productName}";
                        MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        this.Parent.Controls.Remove(this);
                        mf.clearDataForm();
                        return;
                    }

                    Index++;

                    if (emailCls.SendEmail(prop, rpWord) == true)
                    {
                        successMessage = "ส่งอีเมล์สำเร็จ";
                    }
                    else
                    {
                        this.Parent.Controls.Remove(this);
                        mf.clearDataForm();
                        return;
                    }
                }

                this.Parent.Controls.Remove(this);
                mf.clearDataForm();
                MessageBox.Show(successMessage, "FINISHED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dt.Rows.Count == 0 || dt == null)
            {
                MessageBox.Show("Not found data subject or body", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Parent.Controls.Remove(this);
                mf.clearDataForm();
                return;
            }
            else if (sldt.Rows.Count != 0 && dt.Rows.Count == 2)
            {
                string successMessage = "";
                List<string> attachList = new List<string>();
                prop.dgvFile = dataGridViewDocs;

                // ตรวจสอบไฟล์แนบหลัก
                List<string> missingFiles = new List<string>();
                foreach (DataGridViewRow dgvfilePath in prop.dgvFile.Rows)
                {
                    string filePath = dgvfilePath.Cells["attached"].Value?.ToString();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        missingFiles.Add("ไฟล์ว่าง (ไม่มี path)");
                    }
                    else if (!File.Exists(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                    else
                    {
                        attachList.Add(filePath);
                    }
                }

                if (missingFiles.Any())
                {
                    string errorMessage = "ไม่พบไฟล์แนบหลักต่อไปนี้:\n" + string.Join("\n", missingFiles);
                    MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                prop.toEmailAddresses = (List<string>)dt.Rows[0]["toEmailAddresses"];
                prop.ccEmailAddresses = (List<string>)dt.Rows[0]["ccEmailAddresses"];
                rpWord.subject = dt.Rows[0]["subject"].ToString();
                rpWord.body = dt.Rows[0]["body"].ToString();
                prop.productName = dt.Rows[0]["productName"].ToString();
                prop.key = dt.Rows[0]["key_id"].ToString();
                prop.attachmentFilePaths = attachList;

                if (emailCls.SendEmail(prop, rpWord) == true)
                {
                    successMessage = "ส่งอีเมล์สำเร็จ";
                }
                else
                {
                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                // ตรวจสอบไฟล์ Serial List
                attachList = new List<string>();
                missingFiles = new List<string>();

                foreach (DataRow row in sldt.Rows)
                {
                    string filePath = row["attached"].ToString();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        missingFiles.Add("ไฟล์ว่าง (ไม่มี path)");
                    }
                    else if (!File.Exists(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                    else
                    {
                        attachList.Add(filePath);
                    }
                }

                if (missingFiles.Any())
                {
                    string errorMessage = "ไม่พบไฟล์ Serial List ต่อไปนี้:\n" + string.Join("\n", missingFiles);
                    MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                prop.toEmailAddresses = (List<string>)dt.Rows[1]["toEmailAddresses"];
                prop.ccEmailAddresses = (List<string>)dt.Rows[1]["ccEmailAddresses"];
                rpWord.subject = dt.Rows[1]["subject"].ToString();
                rpWord.body = dt.Rows[1]["body"].ToString();
                prop.productName = dt.Rows[1]["productName"].ToString();
                prop.key = dt.Rows[1]["key_id"].ToString();
                prop.attachmentFilePaths = attachList;

                if (emailCls.SendEmail(prop, rpWord) == true)
                {
                    successMessage = "ส่งอีเมล์สำเร็จ";
                }
                else
                {
                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                this.Parent.Controls.Remove(this);
                mf.clearDataForm();
                MessageBox.Show(successMessage, "FINISHED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (wfdt.Rows.Count != 0 && dt.Rows.Count == 3)
            {
                string successMessage = "";
                List<string> attachList = new List<string>();
                prop.dgvFile = dataGridViewDocs;

                // ตรวจสอบไฟล์แนบหลัก
                List<string> missingFiles = new List<string>();
                foreach (DataGridViewRow dgvfilePath in prop.dgvFile.Rows)
                {
                    string filePath = dgvfilePath.Cells["attached"].Value?.ToString();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        missingFiles.Add("ไฟล์ว่าง (ไม่มี path)");
                    }
                    else if (!File.Exists(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                    else
                    {
                        attachList.Add(filePath);
                    }
                }

                if (missingFiles.Any())
                {
                    string errorMessage = "ไม่พบไฟล์แนบหลักต่อไปนี้:\n" + string.Join("\n", missingFiles);
                    MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                prop.toEmailAddresses = (List<string>)dt.Rows[0]["toEmailAddresses"];
                prop.ccEmailAddresses = (List<string>)dt.Rows[0]["ccEmailAddresses"];
                rpWord.subject = dt.Rows[0]["subject"].ToString();
                rpWord.body = dt.Rows[0]["body"].ToString();
                prop.productName = dt.Rows[0]["productName"].ToString();
                prop.key = dt.Rows[0]["key_id"].ToString();
                prop.attachmentFilePaths = attachList;

                if (emailCls.SendEmail(prop, rpWord) == true)
                {
                    successMessage = "ส่งอีเมล์สำเร็จ";
                }
                else
                {
                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                // ตรวจสอบไฟล์ Serial List
                attachList = new List<string>();
                missingFiles = new List<string>();

                foreach (DataRow row in sldt.Rows)
                {
                    string filePath = row["attached"].ToString();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        missingFiles.Add("ไฟล์ว่าง (ไม่มี path)");
                    }
                    else if (!File.Exists(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                    else
                    {
                        attachList.Add(filePath);
                    }
                }

                if (missingFiles.Any())
                {
                    string errorMessage = "ไม่พบไฟล์ Serial List ต่อไปนี้:\n" + string.Join("\n", missingFiles);
                    MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                prop.toEmailAddresses = (List<string>)dt.Rows[1]["toEmailAddresses"];
                prop.ccEmailAddresses = (List<string>)dt.Rows[1]["ccEmailAddresses"];
                rpWord.subject = dt.Rows[1]["subject"].ToString();
                rpWord.body = dt.Rows[1]["body"].ToString();
                prop.productName = dt.Rows[1]["productName"].ToString();
                prop.key = dt.Rows[1]["key_id"].ToString();
                prop.attachmentFilePaths = attachList;

                if (emailCls.SendEmail(prop, rpWord) == true)
                {
                    successMessage = "ส่งอีเมล์สำเร็จ";
                }
                else
                {
                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                // ตรวจสอบไฟล์ Wafer
                attachList = new List<string>();
                missingFiles = new List<string>();

                foreach (DataRow row in wfdt.Rows)
                {
                    string filePath = row["attached"].ToString();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        missingFiles.Add("ไฟล์ว่าง (ไม่มี path)");
                    }
                    else if (!File.Exists(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                    else
                    {
                        attachList.Add(filePath);
                    }
                }

                if (missingFiles.Any())
                {
                    string errorMessage = "ไม่พบไฟล์ Wafer ต่อไปนี้:\n" + string.Join("\n", missingFiles);
                    MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                prop.toEmailAddresses = (List<string>)dt.Rows[2]["toEmailAddresses"];
                prop.ccEmailAddresses = (List<string>)dt.Rows[2]["ccEmailAddresses"];
                rpWord.subject = dt.Rows[2]["subject"].ToString();
                rpWord.body = dt.Rows[2]["body"].ToString();
                prop.productName = dt.Rows[2]["productName"].ToString();
                prop.key = dt.Rows[2]["key_id"].ToString();
                prop.attachmentFilePaths = attachList;

                if (emailCls.SendEmail(prop, rpWord) == true)
                {
                    successMessage = "ส่งอีเมล์สำเร็จ";
                }
                else
                {
                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                this.Parent.Controls.Remove(this);
                mf.clearDataForm();
                MessageBox.Show(successMessage, "FINISHED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (dt.Rows.Count == 1)
            {
                List<string> attachList = new List<string>();
                prop.dgvFile = dataGridViewDocs;

                // ตรวจสอบไฟล์แนบทั้งหมดก่อนส่ง
                List<string> missingFiles = new List<string>();
                foreach (DataGridViewRow dgvfilePath in prop.dgvFile.Rows)
                {
                    string filePath = dgvfilePath.Cells["attached"].Value?.ToString();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        missingFiles.Add("ไฟล์ว่าง (ไม่มี path)");
                    }
                    else if (!File.Exists(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                    else
                    {
                        attachList.Add(filePath);
                    }
                }

                if (missingFiles.Any())
                {
                    string errorMessage = "ไม่พบไฟล์แนบต่อไปนี้:\n" + string.Join("\n", missingFiles);
                    MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                foreach (DataRow row in dt.Rows)
                {
                    prop.toEmailAddresses = (List<string>)row["toEmailAddresses"];
                    prop.ccEmailAddresses = (List<string>)row["ccEmailAddresses"];
                    rpWord.subject = row["subject"].ToString();
                    rpWord.body = row["body"].ToString();
                    prop.productName = row["productName"].ToString();
                    prop.key = row["key_id"].ToString();
                    prop.attachmentFilePaths = attachList;

                    if (emailCls.SendEmail(prop, rpWord) == true)
                    {
                        MessageBox.Show("ส่งอีเมล์สำเร็จ", "FINISHED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Parent.Controls.Remove(this);
                        mf.clearDataForm();
                        return;
                    }
                    else
                    {
                        this.Parent.Controls.Remove(this);
                        mf.clearDataForm();
                        return;
                    }
                }
            }
            else
            {
                string successMessage = "";
                int Index = 0;
                List<string> attachList = new List<string>();
                prop.dgvFile = dataGridViewDocs;

                // ตรวจสอบไฟล์แนบทั้งหมดก่อนส่ง
                List<string> missingFiles = new List<string>();
                foreach (DataGridViewRow dgvfilePath in prop.dgvFile.Rows)
                {
                    string filePath = dgvfilePath.Cells["attached"].Value?.ToString();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        missingFiles.Add("ไฟล์ว่าง (ไม่มี path)");
                    }
                    else if (!File.Exists(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                    else
                    {
                        attachList.Add(filePath);
                    }
                }

                if (missingFiles.Any())
                {
                    string errorMessage = "ไม่พบไฟล์แนบต่อไปนี้:\n" + string.Join("\n", missingFiles);
                    MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    this.Parent.Controls.Remove(this);
                    mf.clearDataForm();
                    return;
                }

                foreach (DataRow row in dt.Rows)
                {
                    prop.attachmentFilePaths = new List<string>();
                    prop.toEmailAddresses = (List<string>)row["toEmailAddresses"];
                    prop.ccEmailAddresses = (List<string>)row["ccEmailAddresses"];
                    rpWord.subject = row["subject"].ToString();
                    rpWord.body = row["body"].ToString();
                    prop.productName = row["productName"].ToString();
                    prop.key = row["key_id"].ToString();

                    // ตรวจสอบไฟล์ที่ Index ปัจจุบัน
                    if (Index < attachList.Count)
                    {
                        string filePath = attachList[Index].ToString();
                        if (File.Exists(filePath))
                        {
                            prop.attachmentFilePaths.Add(filePath);
                        }
                        else
                        {
                            string errorMessage = $"ไม่พบไฟล์ที่ index {Index + 1}:\n{filePath}";
                            MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            this.Parent.Controls.Remove(this);
                            mf.clearDataForm();
                            return;
                        }
                    }
                    else
                    {
                        string errorMessage = $"ไม่พบไฟล์สำหรับอีเมล์ที่ {Index + 1} (จำนวนไฟล์ไม่เพียงพอ)";
                        MessageBox.Show(errorMessage, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        this.Parent.Controls.Remove(this);
                        mf.clearDataForm();
                        return;
                    }

                    if (emailCls.SendEmail(prop, rpWord) == true)
                    {
                        successMessage = "ส่งอีเมล์สำเร็จ";
                    }
                    else
                    {
                        this.Parent.Controls.Remove(this);
                        mf.clearDataForm();
                        return;
                    }

                    Index++;
                }

                this.Parent.Controls.Remove(this);
                mf.clearDataForm();
                MessageBox.Show(successMessage, "FINISHED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        private void dataGridViewDocs_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void bt_ShowEmail_Click(object sender, EventArgs e)
        {
            //DataGridView dgView = ((DataGridView)bdEmail.DataSource);
            frmEmail fe = new frmEmail();
            fe.SetDataSource((DataGridView)bdEmail.DataSource);
            fe.ShowDialog();

        }
    }
}
    