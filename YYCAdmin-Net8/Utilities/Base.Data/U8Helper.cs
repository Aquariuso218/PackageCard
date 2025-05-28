using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Base.Data
{
    public class U8Helper
    {
        /// <summary>
        /// 验证U8档案在系统中是否使用
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="cCode">档案编码</param>
        /// <returns></returns>
        //public static DataTable FilesUsed(string TableName, string cCode)
        //{
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        string guid = Guid.NewGuid().ToString().Replace("-", "");
        //        string sql = "EXECUTE ArchIsUsedProxy '" + TableName + "', '" + cCode + "', 'tempU8501_" + guid + "', 'cRetVal' select * from tempdb..tempU8501_" + guid + "";
        //        dt = Bdbase.Query(sql).Tables[0];
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        dt.Columns.Add("iStatus");
        //        dt.Columns.Add("cRetVal");
        //        dt.Rows.Add("0", ex.Message);
        //        return dt;
        //    }
        //}
    }
}
