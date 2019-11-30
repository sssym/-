using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
namespace pe
{
    public class ToExcel
    {
        public void DataToExcel(DataGridView m_DataView, String Str_Title)
        {
            try
            {
                //建立Excel对象   
                Excel.Application excel = new Excel.Application();
                excel.Application.Workbooks.Add(true);
                excel.Visible = true;
                //生成字段名称   
                for (int i = 0; i < m_DataView.ColumnCount; i++)
                {
                    excel.Cells[1, i + 1] = m_DataView.Columns[i].HeaderText;
                }
                //填充数据   
                for (int i = 0; i < m_DataView.RowCount ; i++)
                {
                    for (int j = 0; j < m_DataView.ColumnCount; j++)
                    {
                        if (m_DataView[j, i].ValueType == typeof(string))
                        {
                            excel.Cells[i + 2, j + 1] = "'" + m_DataView[j, i].Value.ToString();
                        }
                        else
                        {
                            excel.Cells[i + 2, j + 1] = "'" + m_DataView[j, i].Value.ToString();
                        }
                    }
                }


                MessageBox.Show("导出完毕!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "表格内容未导完，请勿操作。");

            }
        }

        public void DataToExcel2(DataGridView m_DataView, String Str_Title)
        {
            SaveFileDialog kk = new SaveFileDialog();
            kk.Title = "保存EXECL文件";
            kk.Filter = "EXECL文件(*.xls) |*.xls |所有文件(*.*) |*.*";
            kk.FilterIndex = 1;
            kk.FileName = Str_Title;
            if (kk.ShowDialog() == DialogResult.OK)
            {
                string FileName = kk.FileName;
                if (File.Exists(FileName))
                    File.Delete(FileName);
                FileStream objFileStream;
                StreamWriter objStreamWriter;
                string strLine = "";
       
                objFileStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write);
                
                objStreamWriter = new StreamWriter(objFileStream, System.Text.Encoding.Unicode);
                // strLine = Str_Title;
               // objStreamWriter.WriteLine(strLine);
                strLine = "";
                for (int i = 0; i < m_DataView.Columns.Count; i++)
                {
                    if (m_DataView.Columns[i].Visible == true)
                    {
                        strLine = strLine + m_DataView.Columns[i].HeaderText.ToString() + Convert.ToChar(9);
                    }

                }
                objStreamWriter.WriteLine(strLine);
                strLine = "";

                for (int i = 0; i < m_DataView.Rows.Count; i++)
                {
                    if (m_DataView.Columns[0].Visible == true)
                    {
                        if (m_DataView.Rows[i].Cells[0].Value == null)
                            strLine = strLine + " " + Convert.ToChar(9);
                        else
                            strLine = strLine + "'"+m_DataView.Rows[i].Cells[0].Value.ToString() + Convert.ToChar(9);
                    }
                    for (int j = 1; j < m_DataView.Columns.Count; j++)
                    {
                        if (m_DataView.Columns[j].Visible == true)
                        {
                            if (m_DataView.Rows[i].Cells[j].Value == null)
                                strLine = strLine + " " + Convert.ToChar(9);
                            else
                            {
                                string rowstr = "";
                                rowstr =  "'"+m_DataView.Rows[i].Cells[j].Value.ToString();
                                if (rowstr.IndexOf("\r\n") > 0)
                                    rowstr = rowstr.Replace("\r\n", " ");
                                if (rowstr.IndexOf("\t") > 0)
                                    rowstr = rowstr.Replace("\t", " ");
                                strLine = strLine + rowstr + Convert.ToChar(9);
                            }
                        }
                    }
                    objStreamWriter.WriteLine(strLine);
                    strLine = "";
                }
                objStreamWriter.Close();

                objFileStream.Close();

                MessageBox.Show("保存EXCEL成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        
    }
}
