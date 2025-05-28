using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Base.U8Util
{
    public class U8Util
    {
        //此方法为Tplus使用，需根据U8系统进行调整

        /// <summary>
        /// 创建单据流水号方法，用于生成唯一的单据编号。
        /// 此方法会根据传入的单据类型（`idvouchertype`）和当前月份生成一个带有固定前缀的流水号，
        /// 如果是当月第一次生成单据号，则会插入新的流水号记录；
        /// 如果当月已有流水号，则会在现有最大流水号基础上递增并返回更新后的流水号。
        /// </summary>
        /// <param name="Connection">数据库连接字符串名称（来自app.config配置文件）</param>
        /// <param name="idvouchertype">单据类型的固定编码，用于区分不同类型的单据（如101表示请购单）</param>
        /// <returns>返回生成的单据编号，格式为“前缀-年月-流水号”（如：PQ-2024-10-0001）</returns>
        /// <remarks>
        /// 流程说明：
        /// 1. 根据前缀（例如 "PQ-2024-10"）查询数据库中是否已有该月的单据流水号。
        /// 2. 如果已有记录，则将最大流水号加1，更新数据库并返回新的单据号。
        /// 3. 如果没有记录，则插入一条新的流水号记录，序列号从0001开始。
        /// </remarks>
        public static string CreatCode(string Connection, string idvouchertype)
        {
            string aa = "PQ";
            string ddate = DateTime.Now.ToString("yyyy-MM");
            string cSeed = aa + '-' + ddate + '-';
            string code = cSeed + "0001";

            //判断本月有无第一个Code? 取最新Code : 新增第一个Code
            string connectionString = ConfigurationManager.ConnectionStrings[Connection].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //101为请购单固定值
                string sqlGet = @"SELECT FORMAT(ISNULL(MaxSerialNumber, 0) + 1, '0000') AS MaxSerialNumber FROM SM_DocSerialNumber WHERE idvouchertype = @idvouchertype AND SerialNumberPrefixion = @cSeed";
                using (SqlCommand command = new SqlCommand(sqlGet, connection))
                {
                    command.Parameters.AddWithValue("@cSeed", cSeed);
                    command.Parameters.AddWithValue("@idvouchertype", idvouchertype);

                    object result1 = command.ExecuteScalar();
                    int nextSerialNumber = Convert.ToInt32(result1);//取到流水号+1


                    // 检查是否存在序列号
                    if (nextSerialNumber > 1)
                    {
                        //1.如果存在，拼接返回单据号
                        code = cSeed + nextSerialNumber.ToString("D4");

                        // 更新 MaxSerialNumber
                        string sqlUpdate = @"UPDATE SM_DocSerialNumber 
                                         SET MaxSerialNumber = @newSerialNumber 
                                         WHERE idvouchertype = @idvouchertype AND SerialNumberPrefixion = @cSeed";

                        using (SqlCommand updateCommand = new SqlCommand(sqlUpdate, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@newSerialNumber", nextSerialNumber);
                            updateCommand.Parameters.AddWithValue("@cSeed", cSeed);
                            updateCommand.Parameters.AddWithValue("@idvouchertype", idvouchertype);

                            updateCommand.ExecuteNonQuery(); // 执行更新
                        }

                        return code;
                    }
                    else
                    {
                        //2.如果不存在，插入"月起始单据号"，返回"月起始单据号"

                        //取出Idusedrule最大值+1
                        string getIdusedrule = "select ISNULL(max(idusedrule), 0)+1 idusedrule from SM_DocSerialNumber";
                        int newIdusedrule;
                        using (SqlCommand insertCommand = new SqlCommand(getIdusedrule, connection))
                        {
                            object result = insertCommand.ExecuteScalar();
                            newIdusedrule = Convert.ToInt32(result);
                        }

                        //插入月起始单据号
                        string sqlSet = @"INSERT INTO SM_DocSerialNumber (idvouchertype,idusedrule, SerialNumberPrefixion, MaxSerialNumber) VALUES (@idvouchertype,@idusedrule, @cSeed, @serialNumber)";
                        using (SqlCommand insertCommand = new SqlCommand(sqlSet, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@idusedrule", newIdusedrule);
                            insertCommand.Parameters.AddWithValue("@cSeed", cSeed);
                            insertCommand.Parameters.AddWithValue("@serialNumber", 1); // 新序列号从1开始
                            insertCommand.Parameters.AddWithValue("@idvouchertype", idvouchertype);
                            insertCommand.ExecuteNonQuery();
                        }

                        return code;
                    }

                }
            }
        }

    }
}
