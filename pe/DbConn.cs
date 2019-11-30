using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
namespace pe
{
    class DbConn
    {
        private string StrConn;
        public SqlConnection SqlDrConn = null;
        public SqlConnection SqlDaConn = null;
        public SqlConnection SqlCmdConn = null;
        public  string ServerName = "";
        public  string SerUser = "";
        public  string SerPassword = "";
        public  string Database = "";
        public  bool SerLogin = true;  //登录模式
        private XmlDocument XmlDoc = new XmlDocument();
        private string FilePath = @".\SetupDataBase.Xml";

        public DbConn()
        {
            //StrConn = "server=localhost;integrated security=sspi;database=housing";
            ReadXml();
            //integrated security=sspi;
        }

        public SqlDataReader GetDataReader(string StrSql)
        {
            try
            {
                SqlDrConn = new SqlConnection(StrConn);
                SqlDrConn.Open();
                SqlCommand SqlCmd = new SqlCommand(StrSql, SqlDrConn);
                SqlDataReader SqlDr = SqlCmd.ExecuteReader();
                return SqlDr;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                SqlDrConn.Close();
                return null;
            }
            finally
            {

            }
        }

        public SqlDataAdapter GetDataAdapter(string StrSql)
        {
            if (SqlDaConn != null && SqlDaConn.State == ConnectionState.Open)
            {
                SqlDaConn.Close();
            }
            else
            {
                SqlDaConn = new SqlConnection(StrConn);
            }
            try
            {
                SqlDaConn.Open();
                SqlDataAdapter SqlDa = new SqlDataAdapter(StrSql, SqlDaConn);
                SqlCommandBuilder SqlCb = new SqlCommandBuilder(SqlDa);
                return SqlDa;
            }
            catch (Exception Msg)
            {
                Console.WriteLine(Msg.ToString());
                SqlDaConn.Close();
                return null;
            }
            finally
            {

            }
        }

        public bool GetTransaction(System.Collections.ArrayList StrSqlList)
        {
            SqlConnection SqlTrConn = new SqlConnection(StrConn);
            SqlTransaction SqlTr = null;
            //----------------------------------------------------------
            SqlTrConn.Open();
            SqlTr = SqlTrConn.BeginTransaction();
            SqlCommand SqlCmd = new SqlCommand();
            SqlCmd.Connection = SqlTrConn;
            SqlCmd.Transaction = SqlTr;
            try
            {
                for (int i = 0; i < StrSqlList.Count; i++)
                {
                    SqlCmd.CommandText = StrSqlList[i].ToString();
                    SqlCmd.ExecuteNonQuery();
                }
                SqlTr.Commit();
            }
            catch (Exception ex)
            {
                SqlTr.Rollback();
                SqlTrConn.Close();
                System.Windows.Forms.MessageBox.Show(ex.Message.ToString() + SqlCmd.CommandText);
                return false;
            }
            SqlTrConn.Close();
            return true;
        }

        public int GetSqlCmd(string StrSql)
        {
            int k = 0;
            try
            {
               
                if (SqlCmdConn != null && SqlCmdConn.State == ConnectionState.Open)
                {
                    SqlCmdConn.Close();
                }
                else
                {
                    SqlCmdConn = new SqlConnection(StrConn);
                }
                SqlCmdConn = new SqlConnection(StrConn);
                SqlCommand SqlCmd = new SqlCommand(StrSql, SqlCmdConn);
                SqlCmdConn.Open();
                k = SqlCmd.ExecuteNonQuery();
                SqlCmdConn.Close();
            }
            catch (Exception Msg)
            {
                System.Windows.Forms.MessageBox.Show(Msg.Message);
            }
            finally
            {
            }
            return k;
        }

        private void ReadXml()
        {
            //读取Xml文档
            string StrNode = "";
            XmlNodeReader reader = null;
            try
            {
                // 装入指定的XML文档
                XmlDoc.Load(FilePath);
                // 设定XmlNodeReader对象来打开XML文件
                reader = new XmlNodeReader(XmlDoc);
                // 读取XML文件中的数据，并显示出来
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            StrNode = reader.Name;
                            break;
                        case XmlNodeType.Text:
                            if (StrNode.Equals("ServerName"))
                            {
                                ServerName = reader.Value;
                            }
                            else if (StrNode.Equals("SerUser"))
                            {
                                SerUser = reader.Value;
                            }
                            else if (StrNode.Equals("SerPassword"))
                            {
                                SerPassword = reader.Value;

                            }
                            else if (StrNode.Equals("SerLogin"))
                            {
                                SerLogin = Convert.ToBoolean(reader.Value);
                            }
                            else if (StrNode.Equals("Database"))
                            { Database = reader.Value; }

                            break;
                    }
                }
                if (ServerName.Equals(""))
                {
                    System.Windows.Forms.MessageBox.Show("配置文件错误，无法连接！", "系统提示"
                        , System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                else if (SerLogin.Equals(""))
                {
                    System.Windows.Forms.MessageBox.Show("配置文件错误，无法连接！", "系统提示"
                        , System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                else
                {
                    StrConn = ToStrConn(ServerName, SerLogin, SerUser, SerPassword, Database);
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("数据库配置文件错误，请重新配置文件！", "错误提示"
                    , System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                // System.Windows.Forms.Application.Exit();
            }
            finally
            {
                //清除打开的数据流
                if (reader != null)
                    reader.Close();
            }
        }

        public static string ToStrConn(string ServerName, bool SerLogin, string SerUser, string SerPassword, string dataBase)
        {
            if (ServerName.Equals(".") || ServerName.Equals(""))
            {
                ServerName = "(local)";
            }
            if (SerLogin == true)
            {
                return "server=" + ServerName + ";integrated security=sspi;database=" + dataBase;
            }
            else
            {
                return "server=" + ServerName + ";database=" + dataBase + ";uid=" + SerUser + ";pwd=" + SerPassword;
            }
        }

        private string Encodebase64(string code)
        {

            string encode = "";
            byte[] bytes = Encoding.Default.GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }

        private string Decodebase64(string code)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.Default.GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        }
    }
}
