using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MasterDetailsCRUDapp
{
    public partial class Form1 : Form
    {
        public int inEmpId;
        bool isDefaultImg = true;

        //Office
        //string strConnectionString = @"Data Source=DESKTOP-8NTSO8C\SQLEXPRESS; Initial Catalog=MasterDetailsDB; integrated Security= true;";

        //Home
        string strConnectionString = @"Data Source=DESKTOP-5FBSLNK; Initial Catalog=MasterDetailsDB; integrated Security= true;";

        string strPreviousImage = "";
        OpenFileDialog ofd = new OpenFileDialog();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PositionComboBoxFill();
            Clear();
        }

        void Clear()
        {
            txtEmpCode.Text = txtEmpName.Text = "";
            cmbGender.SelectedIndex = cmbPosition.SelectedIndex = 0;
            dtpDOB.Value = DateTime.Now;
            rbtRegular.Checked = true;

            if (dgvEmpCompany.DataSource == null)
            {
                dgvEmpCompany.Rows.Clear();
            }
            else
            {
                dgvEmpCompany.DataSource = (dgvEmpCompany.DataSource as DataTable).Clone();
            }

            inEmpId = 0;
            btnSave.Text = "Save";
            btnDelete.Enabled = false;

            pbxPhoto.Image = Image.FromFile(Application.StartupPath + "\\Images\\defaultImg.png");
            isDefaultImg = true;
        }

        void PositionComboBoxFill()
        {
            using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Position", sqlCon);
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                DataRow topItem = dtbl.NewRow();
                topItem[0] = 0; // PositionId column
                topItem[1] = "-Select-"; // Position Column
                dtbl.Rows.InsertAt(topItem, 0);
                cmbPosition.ValueMember = dgvcmbPosition.ValueMember = "PositionId";
                cmbPosition.DisplayMember = dgvcmbPosition.DisplayMember = "Position";
                cmbPosition.DataSource = dtbl;
                dgvcmbPosition.DataSource = dtbl.Copy();
            }
        }

        private void btnImageBrowse_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Images(.jpg,.png)|*.png;*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pbxPhoto.Image = new Bitmap(ofd.FileName);
                isDefaultImg = false;
                strPreviousImage = "";
            }
        }

        private void btnImageClear_Click(object sender, EventArgs e)
        {
            pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\defaultImg.png");
            isDefaultImg = true;
            strPreviousImage = "";

        }
    }
}
