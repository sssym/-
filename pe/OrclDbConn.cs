using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Data.OracleClient;
using System.Collections;

namespace pe
{
    class OrclDbConn
    {

        private string StrConn;

        public OracleConnection OrcDrConn = null;
        public OracleConnection OrcDaConn = null;
        public OracleConnection OrcCmdConn = null;
        public OracleConnection SqlTrConn = null;
        public static string SerUser = "";
        public static string SerPassword = "";
        public static string Database = "";
        public static bool SerLogin = true;  //登录模式
        public static string Str_JFFS = "";
        private XmlDocument XmlDoc = new XmlDocument();
        private string FilePath = @".\icare.Xml";
        public static ArrayList arraylist = null;
        public static string Str_HosName = "";
        public OrclDbConn()
        {
            //StrConn = "server=localhost;integrated security=sspi;database=housing";
            ReadXml();
            //integrated security=sspi;
        }

        public OracleDataReader GetDataReader(string StrSql)
        {

            if (OrcDrConn != null && OrcDrConn.State == ConnectionState.Open)
            {
                OrcDrConn.Close();
            }
            else
            {
                OrcDrConn = new OracleConnection(StrConn);

            }
            try
            {


                OrcDrConn.Open();
                OracleCommand OrcCmd = new OracleCommand(StrSql, OrcDrConn);
                OracleDataReader OrcDr = OrcCmd.ExecuteReader();
                return OrcDr;

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                OrcDrConn.Close();
                return null;
            }
            finally
            {

            }
        }
        public OracleDataAdapter GetDataAdapter(string StrSql)
        {
            if (OrcDaConn != null && OrcDaConn.State == ConnectionState.Open)
            {
                OrcDaConn.Close();
                // OrcDaConn.Close();
            }
            else
            {
                OrcDaConn = new OracleConnection();
                OrcDaConn = new OracleConnection(StrConn);
            }
            try
            {
                OrcDaConn.Open();
                OracleDataAdapter SqlDa = new OracleDataAdapter(StrSql, OrcDaConn);
                OracleCommandBuilder SqlCb = new OracleCommandBuilder(SqlDa);
                return SqlDa;
            }
            catch (Exception Msg)
            {
                Console.WriteLine(Msg.ToString());
                OrcDaConn.Close();
                return null;
            }
            finally
            {

            }
        }

        public bool GetTransaction(System.Collections.ArrayList StrSqlList)
        {
            if (SqlTrConn != null && SqlTrConn.State == ConnectionState.Open)
            {
                SqlTrConn.Close();
            }
            else
            {
                SqlTrConn = new OracleConnection(StrConn);
            }

            OracleTransaction SqlTr = null;
            //----------------------------------------------------------
            try
            {
                SqlTrConn.Open();
                SqlTr = SqlTrConn.BeginTransaction();
                OracleCommand SqlCmd = new OracleCommand();
                SqlCmd.Connection = SqlTrConn;
                SqlCmd.Transaction = SqlTr;

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
                System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
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

                if (OrcCmdConn != null && OrcCmdConn.State == ConnectionState.Open)
                {
                    OrcCmdConn.Close();
                }
                else
                {
                    OrcCmdConn = new OracleConnection(StrConn);
                }
                OrcCmdConn = new OracleConnection(StrConn);
                OracleCommand SqlCmd = new OracleCommand(StrSql, OrcCmdConn);
                OrcCmdConn.Open();
                k = SqlCmd.ExecuteNonQuery();
                OrcCmdConn.Close();
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
                arraylist = new ArrayList();
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
                            if (StrNode.Equals("SerUser"))
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
                            else if (StrNode.Equals("FZHFWLYLF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZHFWLCZF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZHFWLHLF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZHFWLQTF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZDLBLF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZDLSSSF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZDLYXF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZDLLCF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZLLFFSSF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZLLFWLZWLF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZLLFSSF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZLLFMZF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZLLFSSZLF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FKFLKFF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZYLZF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FXYF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FXYLGJF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZCHYF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FZCYF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FXYLXF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FXYLBQBF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FXYLQDBF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FXYLYXYZF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FXYLXBYZF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FHCLCJF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FHCLZLF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FHCLSSF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("FQTF"))
                            { arraylist.Add(reader.Value); }
                            else if (StrNode.Equals("YYMC"))
                            {
                                Str_HosName = Decodebase64(reader.Value);

                            }
                            else if (StrNode.Equals("JFFS"))
                            {
                                Str_JFFS = reader.Value;

                            }


                            break;
                    }



                }
                if (SerLogin.Equals(""))
                {
                    System.Windows.Forms.MessageBox.Show("配置文件错误，无法连接！", "系统提示"
                        , System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                else
                {
                    StrConn = ToStrConn(SerLogin, SerUser, SerPassword, Database);
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("数据库配置文件错误，请重新配置文件！", "错误提示"
                    , System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                System.Windows.Forms.Application.Exit();
            }
            finally
            {
                //清除打开的数据流
                if (reader != null)
                    reader.Close();
            }
        }

        public static string ToStrConn(bool SerLogin, string SerUser, string SerPassword, string dataBase)
        {
            return "data source=" + Database + ";User Id=" + SerUser + ";Password=" + SerPassword + ";";
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
