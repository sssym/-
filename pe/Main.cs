using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Data.SqlClient;

using System.Data.OracleClient;
namespace pe
{
    public partial class Main : Form
    {

        ImportDV importDV;
        ArrayList alist;
        ArrayList alsql;
        DbConn db;
        OrclDbConn orclDbConn = null;
        String Strsql = "";
        String Strsql1 = "";
        DataSet dataSet = null;
        OracleDataReader oracleDataReader = null;
        DbConn dbConn = null;
        OracleDataAdapter oracleDataAdapter = null;
        SqlDataAdapter sqlDataAdapter = null;
        public Main()
        {
            orclDbConn = new OrclDbConn();
            db=new DbConn();
            dbConn = new DbConn();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           //// form1 = new Form1();
           // form1.ShowDialog();
        }

       
        private void button1_Click_1(object sender, EventArgs e)
        {
            importDV = new ImportDV();
            importDV.btnImpot_Click(dataGridView1);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                ToExcel toExcel = new ToExcel();
                toExcel.DataToExcel(dataGridView1, "N41_" );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

            alist = new ArrayList();
            alsql = new ArrayList();
            if (OrclDbConn.Str_JFFS == "1")
            {

                alist = OrclDbConn.arraylist;
                Strsql = "select 住院号 as zyh,patientid_chr,入院日期 as ryrq,出院日期 as cyrq,sum(总费用) as zfy,sum(自付金额) as zfje,";
                for (int ii = 0; ii < alist.Count - 1; ii++)
                {
                    Strsql = Strsql + "max(c" + ii.ToString() + ") as c" + ii.ToString() + ",";
                }
                Strsql = Strsql + "max(c" + (alist.Count - 1).ToString() + ") as c" + (alist.Count - 1).ToString() + " from ";
                Strsql = Strsql + "(select a.inpatientid_chr as 住院号,a.patientid_chr,a.inpatient_dat as 入院日期,d.outhospital_dat as 出院日期,case when b.totalsum_mny is null then sum(c.totalmoney_dec) else  b.totalsum_mny end  as 总费用, "
                       + " case when b.sbsum_mny is null then sum(c.totalmoney_dec) else  b.sbsum_mny  end as 自付金额,";

                for (int ii = 0; ii < alist.Count - 1; ii++)
                {
                    Strsql = Strsql + "sum(case when (c.invcateid_chr in (" + getTypeString(alist[ii].ToString()) + ") ) then c.totalmoney_dec else 0 end) as c" + ii.ToString() + ", ";
                }
                Strsql = Strsql + "sum(case when (c.invcateid_chr in (" + getTypeString(alist[alist.Count - 1].ToString()) + ") ) then c.totalmoney_dec else 0 end) as c" + (alist.Count - 1).ToString();

                Strsql = Strsql + " from t_opr_bih_charge b,t_opr_bih_register a "
                    + " left join t_opr_bih_patientcharge c on c.registerid_chr=a.registerid_chr "
                    + " left join t_opr_bih_leave d on d.registerid_chr=a.registerid_chr "
                    + " where  a.pstatus_int <> 0  and a.status_int = 1 and a.feestatus_int=3 and d.pstatus_int=1 and d.status_int=1 and a.registerid_chr=b.registerid_chr "
                    + " and d.outhospital_dat>=to_date('" + dateTimePicker1.Value.Date.ToShortDateString() + "','yyyy-MM-dd') "
                    + " and d.outhospital_dat<to_date('" + dateTimePicker2.Value.Date.AddDays(1).ToShortDateString() + "','yyyy-MM-dd') "
                    + " and a.inpatientid_chr like '%" + textBox1.Text.Trim() + "%'"
                    // + " group by a.inpatientid_chr ,b.totalsum_mny,b.sbsum_mny,a.inpatient_dat,d.outhospital_dat order by a.inpatientid_chr";
                    + " group by b.totalsum_mny, b.sbsum_mny,a.inpatientid_chr ,a.inpatient_dat,d.outhospital_dat,a.patientid_chr order by a.inpatientid_chr) group by 住院号,入院日期,出院日期,patientid_chr";
                //alsql.Add(Strsql);
                oracleDataAdapter = orclDbConn.GetDataAdapter(Strsql);
                dataSet = new DataSet();
                oracleDataAdapter.Fill(dataSet, "table1");
                Strsql = "delete from table_bafy";
                alsql.Add(Strsql);
                for (int i = 0; i < dataSet.Tables["table1"].Rows.Count; i++)
                {
                    Strsql = "insert into table_bafy values('";
                    for (int j = 0; j < dataSet.Tables["table1"].Columns.Count - 1; j++)
                    {
                        Strsql = Strsql + dataSet.Tables["table1"].Rows[i].ItemArray[j].ToString() + "','";
                    }


                    Strsql = Strsql + dataSet.Tables["table1"].Rows[i].ItemArray[dataSet.Tables["table1"].Columns.Count - 1].ToString() + "')";
                    alsql.Add(Strsql);
                }

                dbConn.GetTransaction(alsql);

                Strsql = "select '' AS USERNAME,(select bavalue from T_SET where A.response_type=hisvalue and type=1)  as YLFKFS ,"
                 + "sxl as JKKH, B.admiss_times AS ZYCS,"
                 + "B.case_no AS BAH,A.name as XM, CASE WHEN B.sex='男' then '1'  when B.sex='女' then '2' end as XB,"
                 + "CONVERT(varchar(12) , B.birth_date, 112 ) AS CSRQ,A.age_unit AS NL,"
                 + "CASE WHEN B.country='中国' then 'CHN' ELSE '' END AS GJ,'0' AS BZYZSNL,"
                 + "(CASE when B.other1 LIKE '%-%' THEN '' when B.other1 like '%－%' then '' ELSE B.other1 end) AS XSECSTZ,"
                 + "(CASE when B.other2 LIKE '%-%' THEN '' when B.other2 like '%－%' then '' ELSE B.other2 end) AS XSERYTZ,"
                    //+ " C.birth_place_3+'@'+birth_place_2 AS CSD,"
                 + "(select birth_place_3+CONVERT(VARCHAR(100),fmc COLLATE Chinese_PRC_CI_AS) from  SJZD_XZQH_NEW where fbh COLLATE   Chinese_PRC_CS_AS=birth_place_3)+birth_place_2 AS CSD,"
                 + "(select case when XZQHNAME='' then CONVERT(VARCHAR(100),  B.other3 COLLATE Chinese_PRC_CI_AS) else XZQHNAME end   from SJZD_XZQH where right(XZQHID,6) = '000000' and XZQHID  COLLATE   Chinese_PRC_CS_AS=B.other3)+"
                 + "(select case when XZQHNAME='' then CONVERT(VARCHAR(100),  B.other4 COLLATE Chinese_PRC_CI_AS) else XZQHNAME end   from SJZD_XZQH where right(XZQHID,3) = '000'   and left(right(XZQHID,5),2) <> '00' and XZQHID  COLLATE   Chinese_PRC_CS_AS=B.other4) AS GG,"
                 + "(SELECT bavalue FROM T_SET WHERE B.nation_code=hisvalue and type=4 ) AS MZ,B.social_no AS SFZH,"
                 + "(SELECT bavalue FROM T_SET WHERE C.occupation_code=hisvalue and type=2 ) AS ZY,"
                 + "(SELECT bavalue FROM T_SET WHERE C.marry_code=hisvalue and type=3 ) AS HY,"
                    //+ "home_name_3+(select CONVERT(VARCHAR(100),XZQHNAME COLLATE Chinese_PRC_CI_AS) from SJZD_XZQH where right(XZQHID,3) = '000'   and left(right(XZQHID,5),2) <> '00' and XZQHID  COLLATE   Chinese_PRC_CS_AS=home_name_3)+home_street_new+'@' AS XZZ,"
                 + "(select home_name_3+CONVERT(VARCHAR(100),fmc COLLATE Chinese_PRC_CI_AS) from  SJZD_XZQH_NEW where fbh COLLATE   Chinese_PRC_CS_AS=home_name_3)+home_street_new  AS XZZ,"
                 + "home_tel_new AS DH,home_zipcode_new AS YB1,"
                 + "(select C.employer_name_3+CONVERT(VARCHAR(100),fmc COLLATE Chinese_PRC_CI_AS) from  SJZD_XZQH_NEW where fbh COLLATE   Chinese_PRC_CS_AS=C.employer_name_3)+C.home_street AS HKDZ,"
                 + "C.home_zipcode AS YB2,"
                 + "(CASE WHEN employer_street LIKE '%-%' THEN '' when employer_street like '%－%' then '' ELSE employer_street END) AS GZDWJDZ,"
                 + "(CASE WHEN employer_tel LIKE '%-%' THEN '' when employer_tel like '%－%' then '' ELSE employer_tel END) as DWDH,"
                 + "(CASE WHEN employer_zipcode LIKE '%-%' THEN '' when employer_zipcode like '%－%' then '' ELSE employer_zipcode END) as YB3,relation_name as LXRXM,"
                 + "(SELECT bavalue FROM T_SET WHERE relation_code=hisvalue and type=6 ) AS GX,"
                 + "relation_name_3 AS DZ,relation_tel AS DH2,admiss_code as RYTJ,CONVERT(varchar(12) , A.admiss_date, 112 ) as RYSJ,"
                 + "DateName(HOUR,A.admiss_date) as RYSJS,(SELECT bavalue FROM T_SET WHERE admiss_dept_code=hisvalue and type=5 ) as RYKB,A.bed_no AS RYBF,"
                 + "(SELECT bavalue FROM T_SET WHERE zkkb=hisvalue and type=5 ) AS ZKKB,CONVERT(varchar(12) , dis_date, 112 ) AS CYSJ,"
                 + "DateName(HOUR,dis_date) as CYSJS,(SELECT bavalue FROM T_SET WHERE dis_dept_code=hisvalue and type=5 ) AS CYKB,D.bed_no AS CYBF,indays AS SJZYTS,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='入院诊断' and patient_id=A.patient_id AND order_no=1) as MZZD,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='入院诊断' and patient_id=A.patient_id AND order_no=1) as JBBM,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=1) as ZYZD,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=1) as JBDM,"
                 + "(case WHEN diag1_1=1 THEN 1 WHEN diag1_2=1 THEN 2 WHEN diag1_3=1 THEN 3 WHEN diag1_4=1 THEN 4 end ) AS RYBQ,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=9) as QTZD8,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=9) as JBDM8,"
                 + "(case WHEN diag9_1=1 THEN 1 WHEN diag9_2=1 THEN 2 WHEN diag9_3=1 THEN 3 WHEN diag9_4=1 THEN 4 end ) AS RYBQ8,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=2) as QTZD1,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=2) as JBDM1,"
                 + "(case WHEN diag2_1=1 THEN 1 WHEN diag2_2=1 THEN 2 WHEN diag2_3=1 THEN 3 WHEN diag2_4=1 THEN 4 end ) AS RYBQ1,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=10) as QTZD9,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=10) as JBDM9,"
                 + "(case WHEN diag10_1=1 THEN 1 WHEN diag10_2=1 THEN 2 WHEN diag10_3=1 THEN 3 WHEN diag10_4=1 THEN 4 end ) AS RYBQ9,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=3) as QTZD2,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=3) as JBDM2,"
                 + "(case WHEN diag3_1=1 THEN 1 WHEN diag3_2=1 THEN 2 WHEN diag3_3=1 THEN 3 WHEN diag3_4=1 THEN 4 end ) AS RYBQ2,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=11) as QTZD10,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=11) as JBDM10,"
                 + "(case WHEN diag11_1=1 THEN 1 WHEN diag11_2=1 THEN 2 WHEN diag11_3=1 THEN 3 WHEN diag11_4=1 THEN 4 end ) AS RYBQ10,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=4) as QTZD3,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=4) as JBDM3,"
                 + "(case WHEN diag4_1=1 THEN 1 WHEN diag4_2=1 THEN 2 WHEN diag4_3=1 THEN 3 WHEN diag4_4=1 THEN 4 end ) AS RYBQ3,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=12) as QTZD11,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=12) as JBDM11,"
                 + "(case WHEN diag12_1=1 THEN 1 WHEN diag12_2=1 THEN 2 WHEN diag12_3=1 THEN 3 WHEN diag12_4=1 THEN 4 end ) AS RYBQ11,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=5) as QTZD4,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=5) as JBDM4,"
                 + "(case WHEN diag5_1=1 THEN 1 WHEN diag5_2=1 THEN 2 WHEN diag5_3=1 THEN 3 WHEN diag5_4=1 THEN 4 end ) AS RYBQ4,"
                 + "'' as QTZD12,'' as JBDM12,'' AS RYBQ12,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=6) as QTZD5,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=6) as JBDM5,"
                 + "(case WHEN diag6_1=1 THEN 1 WHEN diag6_2=1 THEN 2 WHEN diag6_3=1 THEN 3 WHEN diag6_4=1 THEN 4 end ) AS RYBQ5,"
                 + "'' as QTZD13,'' as JBDM13,'' AS RYBQ13,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=7) as QTZD6,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=7) as JBDM6,"
                 + "(case WHEN diag7_1=1 THEN 1 WHEN diag7_2=1 THEN 2 WHEN diag7_3=1 THEN 3 WHEN diag7_4=1 THEN 4 end ) AS RYBQ6,"
                 + "'' as QTZD14,'' as JBDM14,'' AS RYBQ14,"
                 + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=8) as QTZD7,"
                 + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=8) as JBDM7,"
                 + "(case WHEN diag8_1=1 THEN 1 WHEN diag8_2=1 THEN 2 WHEN diag8_3=1 THEN 3 WHEN diag8_4=1 THEN 4 end ) AS RYBQ7,"
                 + "'' as QTZD15,'' as JBDM15,'' AS RYBQ15,"
                 + "(SELECT [name] FROM tblzd_icd_code WHERE D.waibuyinshu=tblzd_icd_code.code AND code > 'V01' and code <='Y98'  ) AS WBYY,"
                 + "(CASE WHEN D.waibuyinshu LIKE '%-%' THEN '' when D.waibuyinshu like '%－%' then '' ELSE D.waibuyinshu END) AS H23,"
                 + "(SELECT [name] FROM tblzd_icd_code WHERE bingli_diag=tblzd_icd_code.code AND  code > 'C00' and code <='D48'   ) AS BLZD,"
                 + "(CASE WHEN bingli_diag LIKE '%-%' THEN '' when bingli_diag like '%－%' then '' ELSE bingli_diag END) AS JBBM,"
                 + "(CASE WHEN blh LIKE '%-%' THEN '' when blh like '%－%' then '' ELSE blh END) AS BLH,"
                 + "(CASE WHEN hbsag=1 THEN '无' when hbsag=2 then '有' END) AS YWGM,ywgm AS GMYW,"
                 + "(CASE WHEN hcv_ab LIKE '%-%' THEN '' when hcv_ab like '%－%' then '' ELSE hcv_ab END) AS SWHZSJ,btype AS XX,(CASE WHEN Rh IN('1','2') THEN Rh ELSE 4 END) AS RH,"
                 + "(select [user_name] from tblxt_user2 where dept_director=user_code) AS KZR,"
                 + "(select [user_name] from tblxt_user2 where director_physician=user_code) AS ZRYS,"
                 + "(select [user_name] from tblxt_user2 where consult_physician=user_code) AS ZZYS,"
                 + "(select [user_name] from tblxt_user2 where refer_physician=user_code)  AS ZYYS,"
                 + "(select [user_name] from tblxt_user2 where dr_study=user_code) AS ZRHS,"
                 + "(select [user_name] from tblxt_user2 where dr_jingxiu=user_code) AS JXYS,"
                 + "(select [user_name] from tblxt_user2 where dr_shixi=user_code)  AS SXYS,"
                 + "(select [user_name] from tblxt_user2 where dr_code=user_code)  AS BMY,grade AS BAZL,"
                 + "(select [user_name] from tblxt_user2 where dr_control=user_code)  AS ZKYS,"
                 + "(select [user_name] from tblxt_user2 where nr_control=user_code)   AS ZKHS,"
                 + "CONVERT(varchar(12) , write_time, 112 ) AS ZKRQ,"
                 + "(CASE WHEN opera_no1 LIKE '%-%' THEN '' ELSE opera_no1 END) AS SSJCZBM1,CONVERT(varchar(12) , opera_date1, 112 ) AS SSJCZRQ1,"
                 + "(CASE WHEN opera_class1='一级' then 1 WHEN opera_class1='二级' then 2 WHEN opera_class1='三级' then 3 WHEN opera_class1='四级' then 4 WHEN opera_class1='%-%' THEN '' end) as SSJB1,"
                 + "(SELECT case when fopname like '%-%' then '' else fopname end FROM tblzd_icd9_cm WHERE fopcode=opera_name1) AS SSJCZMC1,"
                 + "CASE WHEN operator1 LIKE '%-%' THEN '' ELSE operator1 END AS SZ1,"
                 + "CASE WHEN I1 LIKE '%-%' THEN '' ELSE I1 END  AS YZ1,"
                 + "CASE WHEN II1 LIKE '%-%' THEN '' ELSE II1 END AS EZ1,"
                 + "(CASE  SUBSTRING(class1,0,charindex('/',class1)) WHEN '0' THEN 1 WHEN 'Ⅰ' THEN 2 WHEN 'Ⅱ' THEN 3 WHEN 'Ⅲ' THEN 4 ELSE SUBSTRING(class1,NULL,charindex('/',class1)) END) AS QKDJ1,"
                 + "(CASE  SUBSTRING(class1,charindex('/',class1)+1,LEN(class1)-charindex('/',class1)) WHEN '甲' THEN 1 WHEN '乙' THEN 2 WHEN '丙' THEN 3 ELSE   SUBSTRING(class1,NULL,LEN(class1)-charindex('/',class1)) END)  AS QKYHLB1,"
                 + "(CASE WHEN mz1 LIKE '%-%' THEN '' when mz1 like '%－%' then '' ELSE mz1 END) AS MZFS1,"
                 + "(CASE WHEN mzs1 LIKE '%-%' THEN '' when mzs1 like '%－%' then '' ELSE mzs1 END) AS MZYS1,opera_zqss1 AS ZQSS1,opera_no2 AS SSJCZBM2,CONVERT(varchar(12) , opera_date2, 112 ) AS SSJCZRQ2,"
                 + " (CASE WHEN opera_class2='一级' then 1 WHEN opera_class2='二级' then 2 WHEN opera_class2='三级' then 3 WHEN opera_class2='四级' then 4 end) as SSJB2,"
                 + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name2) AS SSJCZMC2,"
                 + "(CASE WHEN operator2 LIKE '%-%' THEN '' when operator2 like '%－%' then '' ELSE operator2 END) AS SZ2,"
                 + "(CASE WHEN I2 LIKE '%-%' THEN '' when I2 like '%－%' then '' ELSE I2 END) AS YZ2,"
                 + "(CASE WHEN II2 LIKE '%-%' THEN '' when II2 like '%－%' then '' ELSE II2 END) AS EZ2,"
                 + "SUBSTRING(class2,0,charindex('/',class2))  AS QKDJ2,SUBSTRING(class2,charindex('/',class2)+1,LEN(class2)-charindex('/',class2)) AS QKYHLB2,"
                 + "mz2 AS MZFS2,mzs2 AS MZYS2,opera_zqss2 AS ZQSS2,opera_no3 AS SSJCZBM3,CONVERT(varchar(12) , opera_date3, 112 ) AS SSJCZRQ3,opera_class3 as SSJB3,"
                 + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name3) AS SSJCZMC3,operator3 AS SZ3,I3 AS YZ3,II3 AS EZ3,"
                 + "SUBSTRING(class3,0,charindex('/',class3))  AS QKDJ3,SUBSTRING(class3,charindex('/',class3)+1,LEN(class3)-charindex('/',class3)) AS QKYHLB3,"
                 + "mz3 AS MZFS3,mzs3 AS MZYS3,opera_zqss3 AS ZQSS3,opera_no4 AS SSJCZBM4,CONVERT(varchar(12) , opera_date4, 112 ) AS SSJCZRQ4,opera_class4 as SSJB4,"
                 + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name4) AS SSJCZMC4,operator4 AS SZ4,I4 AS YZ4,II4 AS EZ4,"
                 + "SUBSTRING(class4,0,charindex('/',class4))  AS QKDJ4,SUBSTRING(class4,charindex('/',class4)+1,LEN(class4)-charindex('/',class4)) AS QKYHLB4,"
                 + "mz4 AS MZFS4,mzs4 AS MZYS4,opera_zqss4 AS ZQSS4,opera_no5 AS SSJCZBM5,CONVERT(varchar(12) , opera_date5, 112 ) AS SSJCZRQ5,opera_class5 as SSJB5,"
                 + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name5) AS SSJCZMC5,operator5 AS SZ5,I5 AS YZ5,II5 AS EZ5,"
                 + "SUBSTRING(class5,0,charindex('/',class5))  AS QKDJ5,SUBSTRING(class5,charindex('/',class5)+1,LEN(class5)-charindex('/',class5)) AS QKYHLB5,"
                 + "mz5 AS MZFS5,mzs5 AS MZYS5,opera_zqss5 AS ZQSS5,opera_no6 AS SSJCZBM6,CONVERT(varchar(12) , opera_date6, 112 ) AS SSJCZRQ6,opera_class6 as SSJB6,"
                 + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name6) AS SSJCZMC6,operator6 AS SZ6,I6 AS YZ6,II6 AS EZ6,"
                 + "SUBSTRING(class6,0,charindex('/',class6))  AS QKDJ6,SUBSTRING(class6,charindex('/',class6)+1,LEN(class6)-charindex('/',class6)) AS QKYHLB6,"
                 + "mz6 AS MZFS6,mzs6 AS MZYS6,opera_zqss6 AS ZQSS6,'' AS SSJCZBM7,'' AS SSJCZRQ7,'' as SSJB7,'' AS SSJCZMC7,"
                 + " '' AS SZ7,'' AS YZ7,'' AS EZ7,''  AS QKDJ7,'' AS QKYHLB7,'' AS MZFS7,'' AS MZYS7,'' AS ZQSS7,"
                 + "D.other17 AS LYFS,(CASE WHEN D.other18 LIKE '%-%' THEN '' when D.other18 like '%－%' then '' ELSE D.other18 END) AS YZZY_YLJG,"
                 + "(CASE WHEN D.other31 LIKE '%-%' THEN '' when D.other31 like '%－%' then '' ELSE D.other31 END) AS WSY_YLJG,(CASE D.shijian WHEN 2 THEN 2 ELSE 1 END)  AS SFZZYJH,"
                 + "(CASE WHEN D.other30 LIKE '%-%' THEN '' when D.other30 like '%－%' then '' ELSE D.other30 END) as MD,"
                 + "(case when D.other19 like '%-%' then '' else  D.other19 end) as RYQ_T,(case when D.other20 like '%-%' then '' else  D.other20 end) as RYQ_XS,(case when D.other21 like '%-%' then '' else  D.other21 end) as RYQ_F,"
                 + "(case when D.other22 like '%-%' then '' else  D.other22 end) as RYH_T,(case when D.other23 like '%-%' then '' else  D.other23 end) as RYH_XS,(case when D.other24 like '%-%' then '' else  D.other24 end) as RYH_F,"
                 + " I.zfy as ZFY,I.zfje AS ZFJE,I.c0 AS YLFUF,I.c1 AS ZLCZF,I.c2 AS HLF,I.c3 AS QTFY,I.c4 AS BLZDF,I.c5 AS SYSZDF,I.c6 AS YXXZDF,I.c7 AS LCZDXMF,"
                    + " I.c8 AS FSSZLXMF,I.c9 AS WLZLF,I.c10 AS SSZLF,I.c11 AS MAF,I.c12 AS SSF,I.c13 AS KFF,I.c14 AS ZYZLF,I.c15 AS XYF,I.c16 AS KJYWF,I.c17 AS ZCYF,"
                    + " I.c18 AS ZCYF1,I.c19 AS XF,I.c20 AS BDBLZPF,I.c21 AS QDBLZPF,I.c22 AS NXYZLZPF,I.c23 AS XBYZLZPF,I.c24 AS HCYYCLF,I.c25 AS YYCLF,I.c26 AS YCXYYCLF,"
                    + " I.c27 AS QTF,"
                    + "(case when source='本市' then 1 when source='外市' then 2 when source='不详' then 4 else 5 end) as BRLY,"
                    + "(CASE WHEN slcase LIKE '%-%' THEN '' when slcase like '%－%' then '' ELSE slcase END) AS LCLJ,"
                    + "qiangjiu AS QJCS,qiangjiu_success AS QJCGCS,(CASE WHEN diag_f5='A' THEN 1 WHEN diag_f5='B' THEN 2 WHEN diag_f5='C' THEN 3 WHEN diag_f5='D' THEN 4 END)  AS BLFX "
                    + " from tblzy_actpatient1 A left join tblpatient_base B on A.case_no=B.case_no"
                    + " LEFT JOIN tblzy_actpatient2 C ON A.patient_id=C.patient_id"
                    + " LEFT JOIN dr_emr_headpage D ON A.patient_id=D.patient_id "
                    + " left join table_bafy I on I.registerid_chr=A.patient_id "
                    + " WHERE dis_date>='" + dateTimePicker1.Value.ToString("yyyy-MM-dd")
                    + "' and dis_date<'" + dateTimePicker2.Value.AddDays(1).ToString("yyyy-MM-dd")
                    + "' and B.case_no like '%" + textBox1.Text + "%'";
                sqlDataAdapter = dbConn.GetDataAdapter(Strsql);
                dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet, "table1");
                dataGridView1.DataSource = dataSet.Tables["table1"].DefaultView;
                button2.Enabled = true;
            }
            else if (OrclDbConn.Str_JFFS == "2")
            {
                Strsql = "select '' AS USERNAME,(select bavalue from T_SET where A.response_type=hisvalue and type=1)  as YLFKFS ,"
                + "sxl as JKKH, B.admiss_times AS ZYCS,"
                + "B.case_no AS BAH,A.name as XM, CASE WHEN B.sex='男' then '1'  when B.sex='女' then '2' end as XB,"
                + "CONVERT(varchar(12) , B.birth_date, 112 ) AS CSRQ,A.age_unit AS NL,"
                + "CASE WHEN B.country='中国' then 'CHN' ELSE '' END AS GJ,'0' AS BZYZSNL,"
                + "(CASE when B.other1 LIKE '%-%' THEN '' when B.other1 like '%－%' then '' ELSE B.other1 end) AS XSECSTZ,"
                + "(CASE when B.other2 LIKE '%-%' THEN '' when B.other2 like '%－%' then '' ELSE B.other2 end) AS XSERYTZ,"
                    //+ " C.birth_place_3+'@'+birth_place_2 AS CSD,"
                + "(select birth_place_3+CONVERT(VARCHAR(100),fmc COLLATE Chinese_PRC_CI_AS) from  SJZD_XZQH_NEW where fbh COLLATE   Chinese_PRC_CS_AS=birth_place_3)+birth_place_2 AS CSD,"
                + "(select case when XZQHNAME='' then CONVERT(VARCHAR(100),  B.other3 COLLATE Chinese_PRC_CI_AS) else XZQHNAME end   from SJZD_XZQH where right(XZQHID,6) = '000000' and XZQHID  COLLATE   Chinese_PRC_CS_AS=B.other3)+"
                + "(select case when XZQHNAME='' then CONVERT(VARCHAR(100),  B.other4 COLLATE Chinese_PRC_CI_AS) else XZQHNAME end   from SJZD_XZQH where right(XZQHID,3) = '000'   and left(right(XZQHID,5),2) <> '00' and XZQHID  COLLATE   Chinese_PRC_CS_AS=B.other4) AS GG,"
                + "(SELECT bavalue FROM T_SET WHERE B.nation_code=hisvalue and type=4 ) AS MZ,B.social_no AS SFZH,"
                + "(SELECT bavalue FROM T_SET WHERE C.occupation_code=hisvalue and type=2 ) AS ZY,"
                + "(SELECT bavalue FROM T_SET WHERE C.marry_code=hisvalue and type=3 ) AS HY,"
                    //+ "home_name_3+(select CONVERT(VARCHAR(100),XZQHNAME COLLATE Chinese_PRC_CI_AS) from SJZD_XZQH where right(XZQHID,3) = '000'   and left(right(XZQHID,5),2) <> '00' and XZQHID  COLLATE   Chinese_PRC_CS_AS=home_name_3)+home_street_new+'@' AS XZZ,"
                + "(select home_name_3+CONVERT(VARCHAR(100),fmc COLLATE Chinese_PRC_CI_AS) from  SJZD_XZQH_NEW where fbh COLLATE   Chinese_PRC_CS_AS=home_name_3)+home_street_new  AS XZZ,"
                + "home_tel_new AS DH,home_zipcode_new AS YB1,"
                + "(select C.employer_name_3+CONVERT(VARCHAR(100),fmc COLLATE Chinese_PRC_CI_AS) from  SJZD_XZQH_NEW where fbh COLLATE   Chinese_PRC_CS_AS=C.employer_name_3)+C.home_street AS HKDZ,"
                + "C.home_zipcode AS YB2,"
                + "(CASE WHEN employer_street LIKE '%-%' THEN '' when employer_street like '%－%' then '' ELSE employer_street END) AS GZDWJDZ,"
                + "(CASE WHEN employer_tel LIKE '%-%' THEN '' when employer_tel like '%－%' then '' ELSE employer_tel END) as DWDH,"
                + "(CASE WHEN employer_zipcode LIKE '%-%' THEN '' when employer_zipcode like '%－%' then '' ELSE employer_zipcode END) as YB3,relation_name as LXRXM,"
                + "(SELECT bavalue FROM T_SET WHERE relation_code=hisvalue and type=6 ) AS GX,"
                + "relation_name_3 AS DZ,relation_tel AS DH2,admiss_code as RYTJ,CONVERT(varchar(12) , A.admiss_date, 112 ) as RYSJ,"
                + "DateName(HOUR,A.admiss_date) as RYSJS,(SELECT bavalue FROM T_SET WHERE admiss_dept_code=hisvalue and type=5 ) as RYKB,A.bed_no AS RYBF,"
                + "(SELECT bavalue FROM T_SET WHERE zkkb=hisvalue and type=5 ) AS ZKKB,CONVERT(varchar(12) , dis_date, 112 ) AS CYSJ,"
                + "DateName(HOUR,dis_date) as CYSJS,(SELECT bavalue FROM T_SET WHERE dis_dept_code=hisvalue and type=5 ) AS CYKB,D.bed_no AS CYBF,indays AS SJZYTS,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='入院诊断' and patient_id=A.patient_id AND order_no=1) as MZZD,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='入院诊断' and patient_id=A.patient_id AND order_no=1) as JBBM,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=1) as ZYZD,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=1) as JBDM,"
                + "(case WHEN diag1_1=1 THEN 1 WHEN diag1_2=1 THEN 2 WHEN diag1_3=1 THEN 3 WHEN diag1_4=1 THEN 4 end ) AS RYBQ,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=9) as QTZD8,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=9) as JBDM8,"
                + "(case WHEN diag9_1=1 THEN 1 WHEN diag9_2=1 THEN 2 WHEN diag9_3=1 THEN 3 WHEN diag9_4=1 THEN 4 end ) AS RYBQ8,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=2) as QTZD1,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=2) as JBDM1,"
                + "(case WHEN diag2_1=1 THEN 1 WHEN diag2_2=1 THEN 2 WHEN diag2_3=1 THEN 3 WHEN diag2_4=1 THEN 4 end ) AS RYBQ1,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=10) as QTZD9,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=10) as JBDM9,"
                + "(case WHEN diag10_1=1 THEN 1 WHEN diag10_2=1 THEN 2 WHEN diag10_3=1 THEN 3 WHEN diag10_4=1 THEN 4 end ) AS RYBQ9,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=3) as QTZD2,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=3) as JBDM2,"
                + "(case WHEN diag3_1=1 THEN 1 WHEN diag3_2=1 THEN 2 WHEN diag3_3=1 THEN 3 WHEN diag3_4=1 THEN 4 end ) AS RYBQ2,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=11) as QTZD10,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=11) as JBDM10,"
                + "(case WHEN diag11_1=1 THEN 1 WHEN diag11_2=1 THEN 2 WHEN diag11_3=1 THEN 3 WHEN diag11_4=1 THEN 4 end ) AS RYBQ10,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=4) as QTZD3,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=4) as JBDM3,"
                + "(case WHEN diag4_1=1 THEN 1 WHEN diag4_2=1 THEN 2 WHEN diag4_3=1 THEN 3 WHEN diag4_4=1 THEN 4 end ) AS RYBQ3,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=12) as QTZD11,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=12) as JBDM11,"
                + "(case WHEN diag12_1=1 THEN 1 WHEN diag12_2=1 THEN 2 WHEN diag12_3=1 THEN 3 WHEN diag12_4=1 THEN 4 end ) AS RYBQ11,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=5) as QTZD4,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=5) as JBDM4,"
                + "(case WHEN diag5_1=1 THEN 1 WHEN diag5_2=1 THEN 2 WHEN diag5_3=1 THEN 3 WHEN diag5_4=1 THEN 4 end ) AS RYBQ4,"
                + "'' as QTZD12,'' as JBDM12,'' AS RYBQ12,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=6) as QTZD5,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=6) as JBDM5,"
                + "(case WHEN diag6_1=1 THEN 1 WHEN diag6_2=1 THEN 2 WHEN diag6_3=1 THEN 3 WHEN diag6_4=1 THEN 4 end ) AS RYBQ5,"
                + "'' as QTZD13,'' as JBDM13,'' AS RYBQ13,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=7) as QTZD6,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=7) as JBDM6,"
                + "(case WHEN diag7_1=1 THEN 1 WHEN diag7_2=1 THEN 2 WHEN diag7_3=1 THEN 3 WHEN diag7_4=1 THEN 4 end ) AS RYBQ6,"
                + "'' as QTZD14,'' as JBDM14,'' AS RYBQ14,"
                + "(SELECT diag_name FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=8) as QTZD7,"
                + "(SELECT icd_code FROM dr_emr_diagnose WHERE diag_type='出院诊断' and patient_id=A.patient_id AND order_no=8) as JBDM7,"
                + "(case WHEN diag8_1=1 THEN 1 WHEN diag8_2=1 THEN 2 WHEN diag8_3=1 THEN 3 WHEN diag8_4=1 THEN 4 end ) AS RYBQ7,"
                + "'' as QTZD15,'' as JBDM15,'' AS RYBQ15,"
                + "(SELECT [name] FROM tblzd_icd_code WHERE D.waibuyinshu=tblzd_icd_code.code AND code > 'V01' and code <='Y98'  ) AS WBYY,"
                + "(CASE WHEN D.waibuyinshu LIKE '%-%' THEN '' when D.waibuyinshu like '%－%' then '' ELSE D.waibuyinshu END) AS H23,"
                + "(SELECT [name] FROM tblzd_icd_code WHERE bingli_diag=tblzd_icd_code.code AND  code > 'C00' and code <='D48'   ) AS BLZD,"
                + "(CASE WHEN bingli_diag LIKE '%-%' THEN '' when bingli_diag like '%－%' then '' ELSE bingli_diag END) AS JBBM,"
                + "(CASE WHEN blh LIKE '%-%' THEN '' when blh like '%－%' then '' ELSE blh END) AS BLH,"
                + "(CASE WHEN hbsag=1 THEN '无' when hbsag=2 then '有' END) AS YWGM,ywgm AS GMYW,"
                + "(CASE WHEN hcv_ab LIKE '%-%' THEN '' when hcv_ab like '%－%' then '' ELSE hcv_ab END) AS SWHZSJ,btype AS XX,(CASE WHEN Rh IN('1','2') THEN Rh ELSE 4 END) AS RH,"
                + "(select [user_name] from tblxt_user2 where dept_director=user_code) AS KZR,"
                + "(select [user_name] from tblxt_user2 where director_physician=user_code) AS ZRYS,"
                + "(select [user_name] from tblxt_user2 where consult_physician=user_code) AS ZZYS,"
                + "(select [user_name] from tblxt_user2 where refer_physician=user_code)  AS ZYYS,"
                + "(select [user_name] from tblxt_user2 where dr_study=user_code) AS ZRHS,"
                + "(select [user_name] from tblxt_user2 where dr_jingxiu=user_code) AS JXYS,"
                + "(select [user_name] from tblxt_user2 where dr_shixi=user_code)  AS SXYS,"
                + "(select [user_name] from tblxt_user2 where dr_code=user_code)  AS BMY,grade AS BAZL,"
                + "(select [user_name] from tblxt_user2 where dr_control=user_code)  AS ZKYS,"
                + "(select [user_name] from tblxt_user2 where nr_control=user_code)   AS ZKHS,"
                + "CONVERT(varchar(12) , write_time, 112 ) AS ZKRQ,"
                + "(CASE WHEN opera_no1 LIKE '%-%' THEN '' ELSE opera_no1 END) AS SSJCZBM1,CONVERT(varchar(12) , opera_date1, 112 ) AS SSJCZRQ1,"
                + "(CASE WHEN opera_class1='一级' then 1 WHEN opera_class1='二级' then 2 WHEN opera_class1='三级' then 3 WHEN opera_class1='四级' then 4 WHEN opera_class1='%-%' THEN '' end) as SSJB1,"
                + "(SELECT case when fopname like '%-%' then '' else fopname end FROM tblzd_icd9_cm WHERE fopcode=opera_name1) AS SSJCZMC1,"
                + "CASE WHEN operator1 LIKE '%-%' THEN '' ELSE operator1 END AS SZ1,"
                + "CASE WHEN I1 LIKE '%-%' THEN '' ELSE I1 END  AS YZ1,"
                + "CASE WHEN II1 LIKE '%-%' THEN '' ELSE II1 END AS EZ1,"
                + "(CASE  SUBSTRING(class1,0,charindex('/',class1)) WHEN '0' THEN 1 WHEN 'Ⅰ' THEN 2 WHEN 'Ⅱ' THEN 3 WHEN 'Ⅲ' THEN 4 ELSE SUBSTRING(class1,NULL,charindex('/',class1)) END) AS QKDJ1,"
                + "(CASE  SUBSTRING(class1,charindex('/',class1)+1,LEN(class1)-charindex('/',class1)) WHEN '甲' THEN 1 WHEN '乙' THEN 2 WHEN '丙' THEN 3 ELSE   SUBSTRING(class1,NULL,LEN(class1)-charindex('/',class1)) END)  AS QKYHLB1,"
                + "(CASE WHEN mz1 LIKE '%-%' THEN '' when mz1 like '%－%' then '' ELSE mz1 END) AS MZFS1,"
                + "(CASE WHEN mzs1 LIKE '%-%' THEN '' when mzs1 like '%－%' then '' ELSE mzs1 END) AS MZYS1,opera_zqss1 AS ZQSS1,opera_no2 AS SSJCZBM2,CONVERT(varchar(12) , opera_date2, 112 ) AS SSJCZRQ2,"
                + " (CASE WHEN opera_class2='一级' then 1 WHEN opera_class2='二级' then 2 WHEN opera_class2='三级' then 3 WHEN opera_class2='四级' then 4 end) as SSJB2,"
                + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name2) AS SSJCZMC2,"
                + "(CASE WHEN operator2 LIKE '%-%' THEN '' when operator2 like '%－%' then '' ELSE operator2 END) AS SZ2,"
                + "(CASE WHEN I2 LIKE '%-%' THEN '' when I2 like '%－%' then '' ELSE I2 END) AS YZ2,"
                + "(CASE WHEN II2 LIKE '%-%' THEN '' when II2 like '%－%' then '' ELSE II2 END) AS EZ2,"
                + "SUBSTRING(class2,0,charindex('/',class2))  AS QKDJ2,SUBSTRING(class2,charindex('/',class2)+1,LEN(class2)-charindex('/',class2)) AS QKYHLB2,"
                + "mz2 AS MZFS2,mzs2 AS MZYS2,opera_zqss2 AS ZQSS2,opera_no3 AS SSJCZBM3,CONVERT(varchar(12) , opera_date3, 112 ) AS SSJCZRQ3,opera_class3 as SSJB3,"
                + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name3) AS SSJCZMC3,operator3 AS SZ3,I3 AS YZ3,II3 AS EZ3,"
                + "SUBSTRING(class3,0,charindex('/',class3))  AS QKDJ3,SUBSTRING(class3,charindex('/',class3)+1,LEN(class3)-charindex('/',class3)) AS QKYHLB3,"
                + "mz3 AS MZFS3,mzs3 AS MZYS3,opera_zqss3 AS ZQSS3,opera_no4 AS SSJCZBM4,CONVERT(varchar(12) , opera_date4, 112 ) AS SSJCZRQ4,opera_class4 as SSJB4,"
                + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name4) AS SSJCZMC4,operator4 AS SZ4,I4 AS YZ4,II4 AS EZ4,"
                + "SUBSTRING(class4,0,charindex('/',class4))  AS QKDJ4,SUBSTRING(class4,charindex('/',class4)+1,LEN(class4)-charindex('/',class4)) AS QKYHLB4,"
                + "mz4 AS MZFS4,mzs4 AS MZYS4,opera_zqss4 AS ZQSS4,opera_no5 AS SSJCZBM5,CONVERT(varchar(12) , opera_date5, 112 ) AS SSJCZRQ5,opera_class5 as SSJB5,"
                + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name5) AS SSJCZMC5,operator5 AS SZ5,I5 AS YZ5,II5 AS EZ5,"
                + "SUBSTRING(class5,0,charindex('/',class5))  AS QKDJ5,SUBSTRING(class5,charindex('/',class5)+1,LEN(class5)-charindex('/',class5)) AS QKYHLB5,"
                + "mz5 AS MZFS5,mzs5 AS MZYS5,opera_zqss5 AS ZQSS5,opera_no6 AS SSJCZBM6,CONVERT(varchar(12) , opera_date6, 112 ) AS SSJCZRQ6,opera_class6 as SSJB6,"
                + "(SELECT fopname FROM tblzd_icd9_cm WHERE fopcode=opera_name6) AS SSJCZMC6,operator6 AS SZ6,I6 AS YZ6,II6 AS EZ6,"
                + "SUBSTRING(class6,0,charindex('/',class6))  AS QKDJ6,SUBSTRING(class6,charindex('/',class6)+1,LEN(class6)-charindex('/',class6)) AS QKYHLB6,"
                + "mz6 AS MZFS6,mzs6 AS MZYS6,opera_zqss6 AS ZQSS6,'' AS SSJCZBM7,'' AS SSJCZRQ7,'' as SSJB7,'' AS SSJCZMC7,"
                + " '' AS SZ7,'' AS YZ7,'' AS EZ7,''  AS QKDJ7,'' AS QKYHLB7,'' AS MZFS7,'' AS MZYS7,'' AS ZQSS7,"
                + "D.other17 AS LYFS,(CASE WHEN D.other18 LIKE '%-%' THEN '' when D.other18 like '%－%' then '' ELSE D.other18 END) AS YZZY_YLJG,"
                + "(CASE WHEN D.other31 LIKE '%-%' THEN '' when D.other31 like '%－%' then '' ELSE D.other31 END) AS WSY_YLJG,(CASE D.shijian WHEN 2 THEN 2 ELSE 1 END)  AS SFZZYJH,"
                + "(CASE WHEN D.other30 LIKE '%-%' THEN '' when D.other30 like '%－%' then '' ELSE D.other30 END) as MD,"
                + "(case when D.other19 like '%-%' then '' else  D.other19 end) as RYQ_T,(case when D.other20 like '%-%' then '' else  D.other20 end) as RYQ_XS,(case when D.other21 like '%-%' then '' else  D.other21 end) as RYQ_F,"
                + "(case when D.other22 like '%-%' then '' else  D.other22 end) as RYH_T,(case when D.other23 like '%-%' then '' else  D.other23 end) as RYH_XS,(case when D.other24 like '%-%' then '' else  D.other24 end) as RYH_F,"
                + "D.fy_sum AS ZFY,D.other16 AS ZFJE,D.m_chuang AS YLFUF,D.zl as ZLCZF,D.m_huli as HLF,D.other3 as QTFY,D.m_check as BLZDF,D.hy as SYSZDF,D.fs as YXXZDF,D.qt as LCZDXMF,"
                    + "D.other4 as FSSZLXMF,D.other5 as WLZLF,D.other6 as SSZLF,D.mazui as MAF,D.operation as SSF,D.other7 as KFF,"
                    + "D.other8 as ZYZLF,D.m_xiyao as XYF,D.other2 as KJYWF,D.m_zy as ZCYF,D.m_cy as ZCYF1,D.sx as XF,D.other9 as BDBLZPF,D.other10 as QDBLZPF,D.other11 as NXYZLZPF,"
                    + "D.other12 as XBYZLZPF,D.other13 as HCYYCLF,D.other14 as YYCLF,D.other15 as YCXYYCLF,D.other1 as QTF,"
                   + "(case when source='本市' then 1 when source='外市' then 2 when source='不详' then 4 else 5 end) as BRLY,"
                   + "(CASE WHEN slcase LIKE '%-%' THEN '' when slcase like '%－%' then '' ELSE slcase END) AS LCLJ,"
                   + "qiangjiu AS QJCS,qiangjiu_success AS QJCGCS,(CASE WHEN diag_f5='A' THEN 1 WHEN diag_f5='B' THEN 2 WHEN diag_f5='C' THEN 3 WHEN diag_f5='D' THEN 4 END)  AS BLFX "
                   + " from tblzy_actpatient1 A left join tblpatient_base B on A.case_no=B.case_no AND A.admiss_times=B.admiss_times"
                   + " LEFT JOIN tblzy_actpatient2 C ON A.patient_id=C.patient_id"
                   + " LEFT JOIN dr_emr_headpage D ON A.patient_id=D.patient_id "
                   + " WHERE dis_date>='" + dateTimePicker1.Value.ToString("yyyy-MM-dd")
                   + "' and dis_date<'" + dateTimePicker2.Value.AddDays(1).ToString("yyyy-MM-dd")
                   + "' and B.case_no like '%" + textBox1.Text + "%'";
               
                sqlDataAdapter = dbConn.GetDataAdapter(Strsql);
                dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet, "table1");
                dataGridView1.DataSource = dataSet.Tables["table1"].DefaultView;
                button2.Enabled = true;
            }
          
            
        }
        public String getTypeString(String str)
        {
            int length = 0;
            String Str1 = "";
            int i = 0;
            do
            {
                length = str.IndexOf(',');
                if (length > 0)
                {
                    if (i == 0)
                    {
                        Str1 = Str1 + "'" + str.Substring(0, length) + "'";
                        i++;
                    }
                    else
                    {
                        Str1 = Str1 + ",'" + str.Substring(0, length) + "'";
                        i++;
                    }
                    str = str.Substring(length + 1);
                }
                else if (length == -1)
                {
                    if (i == 0)
                    {
                        Str1 = "'" + str + "'";
                    }
                    else
                    {
                        Str1 = Str1 + ",'" + str + "'";
                    }
                }
            }
            while (length > 0);

            return Str1;
        }

        private void Main_Load(object sender, EventArgs e)
        {
           
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            FrmSetup fromSetup = new FrmSetup();
            fromSetup.ShowDialog();
        }
    }
}
