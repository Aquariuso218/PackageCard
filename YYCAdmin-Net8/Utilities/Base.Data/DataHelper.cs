using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Utilities.Base.Data
{
    /// <summary>
    /// 数据源转换
    /// </summary>
    public class DataHelper
    {
        #region IList如何转成List<T>
        /// <summary>
        /// IList如何转成List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> IListToList<T>(IList list)
        {
            T[] array = new T[list.Count];
            list.CopyTo(array, 0);
            return new List<T>(array);
        }
        #endregion

        #region DataTable根据条件过滤表的内容
        /// <summary>
        /// 根据条件过滤表的内容
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static DataTable GetNewDataTable(DataTable dt, string condition)
        {
            if (!DataHelper.IsExistRows(dt))
            {
                if (condition.Trim() == "")
                {
                    return dt;
                }
                else
                {
                    DataTable newdt = new DataTable();
                    newdt = dt.Clone();
                    DataRow[] dr = dt.Select(condition);
                    for (int i = 0; i < dr.Length; i++)
                    {
                        newdt.ImportRow((DataRow)dr[i]);
                    }
                    return newdt;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 检查DataTable 是否有数据行
        /// <summary>
        /// 检查DataTable 是否有数据行
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns></returns>
        public static bool IsExistRows(DataTable dt)
        {
            if (dt != null && dt.Rows.Count > 0)
                return false;

            return true;
        }
        #endregion

        #region DataTable 转 DataTableToHashtable
        /// <summary>
        /// DataTable 转 DataTableToHashtable
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Hashtable DataTableToHashtable(DataTable dt)
        {
            Hashtable ht = new Hashtable();
            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string key = dt.Columns[i].ColumnName;
                    ht[key] = dr[key];
                }
            }
            return ht;
        }
        #endregion

        #region List转换DataTable
        /// <summary>
        /// 将泛类型集合List类转换成DataTable
        /// </summary>
        /// <param name="list">泛类型集合</param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(List<T> entitys)
        {
            //检查实体集合不能为空
            if (entitys == null || entitys.Count < 1)
            {
                throw new Exception("需转换的集合为空");
            }
            //取出第一个实体的所有Propertie
            Type entityType = entitys[0].GetType();
            PropertyInfo[] entityProperties = entityType.GetProperties();

            //生成DataTable的structure
            //生产代码中，应将生成的DataTable结构Cache起来，此处略
            DataTable dt = new DataTable();
            for (int i = 0; i < entityProperties.Length; i++)
            {
                //dt.Columns.Add(entityProperties[i].Name, entityProperties[i].PropertyType);
                dt.Columns.Add(entityProperties[i].Name);
            }
            //将所有entity添加到DataTable中
            foreach (object entity in entitys)
            {
                //检查所有的的实体都为同一类型
                if (entity.GetType() != entityType)
                {
                    throw new Exception("要转换的集合元素类型不一致");
                }
                object[] entityValues = new object[entityProperties.Length];
                for (int i = 0; i < entityProperties.Length; i++)
                {
                    entityValues[i] = entityProperties[i].GetValue(entity, null);
                }
                dt.Rows.Add(entityValues);
            }
            return dt;
        }
        #endregion

        #region DataTable/DataSet 转 XML
        /// <summary>
        /// DataTable 转 XML
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DataTableToXML(DataTable dt)
        {
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    System.IO.StringWriter writer = new System.IO.StringWriter();
                    dt.WriteXml(writer);
                    return writer.ToString();
                }
            }
            return String.Empty;
        }
        /// <summary>
        /// DataSet 转 XML
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string DataSetToXML(DataSet ds)
        {
            if (ds != null)
            {
                System.IO.StringWriter writer = new System.IO.StringWriter();
                ds.WriteXml(writer);
                return writer.ToString();
            }
            return String.Empty;
        }
        #endregion


        //判断是否日期格式
        public static bool IsDate(string strDate)
        {
            try
            {
                DateTime.Parse(strDate);  //不是字符串时会出现异常
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 判断字符串是否是纯数字
        /// </summary>
        public static bool IsNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            const string pattern = "^[0-9]*$";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(s);
        }


        #region Unix时间戳与DataTime相互转换
        /// <summary>
        /// Unix时间戳转DateTime
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(string timestamp)
        {
            DateTime time = DateTime.MinValue;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            if (timestamp.Length == 10)        //精确到秒
            {
                time = startTime.AddSeconds(double.Parse(timestamp));
            }
            else if (timestamp.Length == 13)   //精确到毫秒
            {
                time = startTime.AddMilliseconds(double.Parse(timestamp));
            }
            return time;
        }

        /// <summary>
        /// DateTime转时间戳
        /// </summary>
        /// <param name="time">DateTime时间</param>
        /// <param name="type">0为毫秒,1为秒</param>
        /// <returns></returns>
        public static string ConvertTimestamp(DateTime time, int type = 0)
        {
            double intResult = 0;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            if (type == 0)
            {
                intResult = (time - startTime).TotalMilliseconds;
            }
            else if (type == 1)
            {
                intResult = (time - startTime).TotalSeconds;
            }
            else
            {
                Console.WriteLine("参数错误!");
            }
            return Math.Round(intResult, 0).ToString();
        }
        #endregion

    }
}
