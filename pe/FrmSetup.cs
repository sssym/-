using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace pe
{
    public partial class FrmSetup : Form
    {

        ImportDV importDV;
        ArrayList alist;
        ArrayList alsql;
        DbConn db;
        String Strsql = "";
        String Strsql1 = "";
        int i_flag = -1;
        DataSet dataSet = null;
        OracleDataReader oracleDataReader = null;
        DbConn sqlDbConn = null;
        SqlDataAdapter sqlDataAdapter = null;
        public FrmSetup()
        {
            InitializeComponent();
            sqlDbConn = new DbConn();
        }

        private void FrmSetup_Load(object sender, EventArgs e)
        {
            LoadGv();
            comboBox1.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            comboBox1.SelectedIndex = 0;

        }
        private void LoadGv()
        {
            Strsql = "select type as id,name as 参数类型,bavalue as 病案参数,hisvalue as HIS参数 from T_SET,T_SETdetail where T_SET.type=T_SETdetail.id order by type";
            sqlDataAdapter = sqlDbConn.GetDataAdapter(Strsql);
            dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet, "table1");
            dataGridView1.DataSource = dataSet.Tables["table1"].DefaultView;
            dataGridView1.Columns["id"].Visible = false;

        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                comboBox1.SelectedIndex = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                textBox1.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                textBox2.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                comboBox1.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                button3.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = true;
            }
            else
            {
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            i_flag = 0;
            textBox1.Text = "";
            textBox2.Text = "";
            comboBox1.Enabled = true;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dir = MessageBox.Show("确定删除该项目？", "警告", MessageBoxButtons.YesNo);
            if (dir == DialogResult.Yes)
            {
                Strsql = "delete from T_SET where bavalue='" + textBox1.Text + "' and hisvalue='" + textBox2.Text + "' and type=" + comboBox1.SelectedIndex.ToString();
                if (sqlDbConn.GetSqlCmd(Strsql) != 0)
                {
                    MessageBox.Show("删除成功！");
                    LoadGv();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (comboBox1.SelectedIndex > 0)
            {
                Strsql = "insert into T_SET values('" + textBox2.Text + "','" + textBox1.Text + "','" + comboBox1.SelectedIndex.ToString() + "')";
                if (sqlDbConn.GetSqlCmd(Strsql) != 0)
                {
                    MessageBox.Show("新增成功！");
                }

                textBox1.Enabled = false;
                textBox2.Enabled = false;
                button3.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = true;
                LoadGv();
            }
            else
            {
                MessageBox.Show("请选择类型！");
            }




        }
    }
}
