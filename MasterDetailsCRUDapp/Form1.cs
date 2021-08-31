using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        public Form1()
        {
            InitializeComponent();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
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

        private void Form1_Load(object sender, EventArgs e)
        {
            Clear();
        }
    }
}
