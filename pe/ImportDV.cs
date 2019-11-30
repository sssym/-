using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace pe
{
    class ImportDV
    {
        public void btnImpot_Click(DataGridView dgv)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Excel文件";
            ofd.FileName = "";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ofd.Filter = "Excel文件(*.xls)|*.xls|所有文件(*.*) |*.*";
            ofd.ValidateNames = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            string strName = string.Empty;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                strName = ofd.FileName;
            }

            if (strName == "")
            {
                MessageBox.Show("没有选择Excel文件，无法导入");
                return;
            }

            ExcelToDataGridView(strName, dgv);
            //ExcelToSqlServer(strName);
            // GetCount();
            //  DisplayExcel();

        }


        private void ExcelToDataGridView(String str, DataGridView gv)
        {
            OleDbConnection ole = null;
            OleDbDataAdapter da = null;
            DataTable dt = null;
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;"
                             + "Data Source=" + str + ";"
                             + "Extended Properties=Excel 5.0";
            //string sTableName = comboBox1.Text.Trim();
            string strExcel = "select * from [sheet1$]";
            try
            {
                ole = new OleDbConnection(strConn);
                ole.Open();
                da = new OleDbDataAdapter(strExcel, ole);
                dt = new DataTable();
                da.Fill(dt);
                gv.DataSource = dt;
                ole.Close();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
            finally
            {
                if (ole != null)
                    ole.Close();
            }
        }
    }
}
