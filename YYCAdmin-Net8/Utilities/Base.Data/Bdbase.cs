using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;

namespace Utilities.Base.Data
{
    public class Bdbase
    {

        //public static string connectionStringU8;

        public Bdbase()
        {

        }

        //日志记录
        public static void WriteLogSql(string Message)
        {
            StreamWriter streamWriter = new StreamWriter(new FileStream("c:\\U8Log\\" + DateTime.Today.ToString("yyyyMMdd") + "_UAP.txt", FileMode.Append, FileAccess.Write));
            streamWriter.WriteLine("-----------------------------------------");
            streamWriter.WriteLine(DateTime.Now);
            streamWriter.WriteLine(Message);
            streamWriter.Flush();
            streamWriter.Close();
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string connectionStringU8, string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionStringU8))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();
                        LogManager.WriteLog(e.Message);
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        //执行语句返回值
        public static string GetValue(string connectionStringU8,string strSQL)
        {
            object val;
            using (SqlConnection conn = new SqlConnection(connectionStringU8))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(strSQL, conn);
                    val = cmd.ExecuteScalar();
                    conn.Close();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new System.ArgumentOutOfRangeException("index parameter is out of range.", ex);
                }
            }
            if (val == null)
            {
                return "";
            }
            else
            {
                return val.ToString();
            }
        }

        public static DataSet Query(string connectionStringU8, string SQLString)
        {
            //JSON装换方法 也使用此工具，获取PLM数据，后续需要优化
            using (SqlConnection connection = new SqlConnection(connectionStringU8))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter command = new SqlDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                    command.Dispose();
                    connection.Dispose();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    connection.Dispose();
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }

        public static void ExecuteSql(string connectionStringU8, string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionStringU8))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (System.Data.SqlClient.SqlException E)
                    {
                        connection.Close();
                        connection.Dispose();
                        throw new Exception(E.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 获取U8单据号
        /// </summary>
        /// <param name="CardNum">单据ID</param>
        /// <param name="FirstCode">流水之前的内容</param>
        /// <param name="cSeed">流水依据</param>
        /// <returns>单据编号</returns>
        //public static string GetCode(string CardNum, string FirstCode, string cSeed)
        //{
        //    try
        //    {
        //        string sql = "select * from VoucherNumber  where CardNumber='" + CardNum + "'";
        //        DataSet ds = Query(sql);
        //        string ccode = "1".PadLeft(int.Parse(ds.Tables[0].Rows[0]["GlideLen"].ToString()), '0');
        //        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //        {
        //            string sqlcode = "select (cNumber+1) as MaxNumber From VoucherHistory  with (ROWLOCK)  Where  CardNumber='" + CardNum + "'and  isnull(cContent,'')='" + ds.Tables[0].Rows[0]["Glide"].ToString() + "' and isnull(cContentRule,'')='" + ds.Tables[0].Rows[0]["GlideRule"].ToString() + "' and isnull(cSeed,'')='" + cSeed + "'";
        //            DataSet dscode = Query(sqlcode);
        //            if (dscode != null && dscode.Tables.Count > 0 && dscode.Tables[0].Rows.Count > 0)
        //            {
        //                ccode = dscode.Tables[0].Rows[0]["MaxNumber"].ToString().PadLeft(int.Parse(ds.Tables[0].Rows[0]["GlideLen"].ToString()), '0');
        //                ExecuteSql("update VoucherHistory set cNumber=cNumber+1 Where  CardNumber='" + CardNum + "' and isnull(cContent,'')='" + ds.Tables[0].Rows[0]["Glide"].ToString() + "' and isnull(cContentRule,'')='" + ds.Tables[0].Rows[0]["GlideRule"].ToString() + "' and isnull(cSeed,'')='" + cSeed + "'");
        //            }
        //            else
        //            {
        //                ExecuteSql(string.Format("insert into VoucherHistory(CardNumber,cContent,cContentRule,cSeed,cNumber,bEmpty) values('" + CardNum + "',{0},{1},{2},'1','0')", ds.Tables[0].Rows[0]["Glide"].ToString() == "" ? @"NULL" : "'" + ds.Tables[0].Rows[0]["Glide"].ToString() + "'", ds.Tables[0].Rows[0]["GlideRule"].ToString() == "" ? @"NULL" : "'" + ds.Tables[0].Rows[0]["GlideRule"].ToString() + "'", cSeed == "" ? @"NULL" : "'" + cSeed + "'"));
        //            }
        //            ccode = FirstCode + ccode;
        //            return ccode;
        //        }
        //        else
        //        {
        //            return "";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.WriteLog(ex.Message);
        //        return "";
        //    }
        //}

        /// <summary>
        /// 递归算法，根据存货分类以及级次取批次规则
        /// </summary>
        /// <param name="cinvcode"></param>
        /// <param name="iinvcgrade"></param>
        /// <returns></returns>
        //public static DataTable LotNumberGz(string cinvcode, int iinvcgrade)
        //{
        //    string sql = "select * from lotnumberrulemain where cinvcode=N'" + cinvcode + "' and cinvtype=N'0'";
        //    DataTable dt = Query(sql).Tables[0];
        //    if (DataHelper.IsExistRows(dt))
        //    {
        //        iinvcgrade--;
        //        if (iinvcgrade <= 0)
        //        {
        //            return null;
        //        }
        //        //获取当前存货分类级次
        //        int iinvcgradeArr = int.Parse(Query("select CODINGRULE from GradeDef where KEYWORD=N'inventoryclass'").Tables[0].Rows[0][0].ToString().Substring(iinvcgrade, 1));
        //        cinvcode = cinvcode.Substring(0, cinvcode.Length - iinvcgradeArr);
        //        //递归获取批号
        //        return LotNumberGz(cinvcode, iinvcgrade);
        //    }
        //    return dt;
        //}

        /// <summary>
        /// 根据Key取Value值
        /// </summary>
        /// <param name="key"></param>
        public static string AppSettings(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 效率最高，最灵活分页查询
        /// </summary>
        /// <param name="tablename">查询表</param>
        /// <param name="columname">查询列</param>
        /// <param name="where">条件</param>
        /// <param name="pagesize">分页条数</param>
        /// <param name="pagecount">当前页</param>
        /// <param name="sortname">排序字段</param>
        /// <param name="sortorder">排序方向</param>
        /// <param name="primarykey">主键</param>
        /// <returns></returns>
        //public static DataSet tra_twotableselect(string tablename, string columname, string where, string pagesize, string pagecount, string sortname, string sortorder, string primarykey)
        //{
        //    string tempwhere = "";
        //    if (where != "")
        //        tempwhere = " where " + where + " ";
        //    string sortstr = "";
        //    if (sortname != "")
        //    {
        //        if (sortorder != "" && primarykey != "")
        //            sortstr += " order by " + sortname + " " + sortorder + "," + primarykey + " " + sortorder + " ";
        //        else if (primarykey != "")
        //            sortstr += " order by " + sortname + "," + primarykey + " ";
        //        else
        //            sortstr += " order by " + sortname + " " + sortorder + " ";
        //    }
        //    else
        //    {
        //        if (primarykey != "")
        //        {
        //            sortstr += " order by " + primarykey + " ";
        //            if (sortorder != "")
        //                sortstr += " " + sortorder + " ";
        //        }
        //    }
        //    string datacount = " select count(*) from " + tablename + tempwhere;//获取页数
        //    int tempcount = int.Parse(pagecount) - 1;
        //    pagecount = tempcount.ToString();
        //    string datasql = "";
        //    if (pagecount == "0")
        //    {//第一页，直接top取
        //        datasql = "select top " + pagesize + " " + columname + "  from " + tablename + tempwhere + sortstr;
        //    }
        //    else
        //    {
        //        datasql = "select * from (select row_number()over(order by tempcolumn)temprownumber,* from (select top (" + pagecount + " * " + pagesize + " + " + pagesize + ") tempcolumn=0," + columname + " from " + tablename + tempwhere + sortstr + ")t01t)t02t where temprownumber>" + pagecount + " * " + pagesize + " " + sortstr;
        //    }
        //    DataSet ds = Query(datasql + "    " + datacount);
        //    return ds;
        //}

        //查询后返回
        public static List<T> ExecuteSql<T>(string connectionStringU8 ,string sqlQuery, Func<DataRow, T> convertRow)
        {
            using (SqlConnection connection = new SqlConnection(connectionStringU8))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(sqlQuery, connection);
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);

                List<T> resultList = new List<T>();
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    T item = convertRow(row);
                    resultList.Add(item);
                }
                return resultList;
            }
        }


    }
}
