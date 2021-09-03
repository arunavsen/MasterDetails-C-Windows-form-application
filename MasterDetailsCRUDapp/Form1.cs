using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
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
            FillEmployeeDataGridView();
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidMasterDetailForm())
            {
                int _EmpID = 0;

                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    //Master
                    SqlCommand sqlCmd = new SqlCommand("EmployeeAddOrEdit",sqlCon);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("@EmpId", inEmpId);
                    sqlCmd.Parameters.AddWithValue("@EmpCode", txtEmpCode.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@EmpName",txtEmpName.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@PositionID",Convert.ToInt32( cmbPosition.SelectedValue));
                    sqlCmd.Parameters.AddWithValue("@DOB", dtpDOB.Value);
                    sqlCmd.Parameters.AddWithValue("@Gender",cmbGender.Text);
                    sqlCmd.Parameters.AddWithValue("@State",rbtRegular.Checked? "Regular":"Contractual");

                    //If user doesn't insert any image during Add action
                    if (isDefaultImg)
                    {
                        sqlCmd.Parameters.AddWithValue("@ImagePath",DBNull.Value);
                    }
                    // If user doesn't update any image during Update action
                    else if (inEmpId > 0 && strPreviousImage != "")
                    {
                        sqlCmd.Parameters.AddWithValue("@ImagePath", strPreviousImage);
                    }
                    else
                    {
                        sqlCmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    }

                    _EmpID = Convert.ToInt32(sqlCmd.ExecuteScalar());
                }

                //Details
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    foreach (DataGridViewRow item in dgvEmpCompany.Rows)
                    {
                        if (item.IsNewRow)
                        {
                            break;
                        }
                        else
                        {
                            SqlCommand sqlCmd = new SqlCommand("EmpCompanyAddOrEdit", sqlCon);
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            sqlCmd.Parameters.AddWithValue("@EmpCmpId", Convert.ToInt32(item.Cells["dgvtxtEmpCompId"].Value == DBNull.Value? "0" : item.Cells["dgvtxtEmpCompId"].Value));
                            sqlCmd.Parameters.AddWithValue("@EmpId", _EmpID);
                            sqlCmd.Parameters.AddWithValue("@CompanyName", item.Cells["dgvtxtCompanyName"].Value == DBNull.Value ? "0" : item.Cells["dgvtxtCompanyName"].Value);
                            sqlCmd.Parameters.AddWithValue("@PositionID", Convert.ToInt32(item.Cells["dgvcmbPosition"].Value == DBNull.Value ? "0" : item.Cells["dgvcmbPosition"].Value));
                            sqlCmd.Parameters.AddWithValue("@ExpYear", Convert.ToInt32(item.Cells["dgvtxtYear"].Value == DBNull.Value ? "0" : item.Cells["dgvtxtYear"].Value));
                            sqlCmd.ExecuteNonQuery();
                        }
                    }
                }
                FillEmployeeDataGridView();
                Clear();
                MessageBox.Show("Submitted Successfully");
            }
        }

        bool ValidMasterDetailForm()
        {
            bool _isValid = true;
            if (txtEmpName.Text.Trim() == "")
            {
                MessageBox.Show("Employee name is required");
                _isValid = false;
            }

            return _isValid;
        }

        string SaveImage(string _imagePath)
        {
            string _fileName = Path.GetFileNameWithoutExtension(_imagePath);
            string _extension = Path.GetExtension(_imagePath);
            //Shorten Image Name
            _fileName = _fileName.Length <= 15 ? _fileName : _fileName.Substring(0, 15);
            _fileName = _fileName + DateTime.Now.ToString("yymmssfff") + _extension;
            pbxPhoto.Image.Save(Application.StartupPath + "\\Images\\" + _fileName);
            return _fileName;
        }

        void FillEmployeeDataGridView()
        {
            using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
            {
                sqlcon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("EmployeeViewAll",sqlcon);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                dgvEmployee.DataSource = dtbl;
                dgvEmployee.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvEmployee.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvEmployee.Columns[0].Visible = false;
            }
        }
    }
}
