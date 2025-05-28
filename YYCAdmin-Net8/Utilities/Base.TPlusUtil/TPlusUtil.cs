using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Base.TPlusUtil
{
    public class TPlusUtil
    {

        public static string CreatCode(string ConnectionName, string idvouchertype)
        {
            string aa = "PQ";                                   // 单据前缀
            string ddate = DateTime.Now.ToString("yyyy-MM");    // 当前年月
            string cSeed = aa + '-' + ddate + '-';              // 组合前缀与日期
            string code = cSeed + "0001";                       // 默认起始流水号

            // 获取数据库连接字符串
            string connectionString = ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // 查询指定单据类型和前缀的月最大流水号，101代表固定请购单类型
                string sqlGet = @"SELECT FORMAT(ISNULL(MaxSerialNumber, 0) + 1, '0000') AS MaxSerialNumber FROM SM_DocSerialNumber WHERE idvouchertype = @idvouchertype AND SerialNumberPrefixion = @cSeed";
                using (SqlCommand command = new SqlCommand(sqlGet, connection))
                {
                    command.Parameters.AddWithValue("@cSeed", cSeed);
                    command.Parameters.AddWithValue("@idvouchertype", idvouchertype);

                    // 获取下一个流水号
                    object result1 = command.ExecuteScalar();
                    int nextSerialNumber = Convert.ToInt32(result1);//取到流水号+1


                    // 1.如果流水号存在
                    if (nextSerialNumber > 1)
                    {
                        // 拼接返回单据号
                        code = cSeed + nextSerialNumber.ToString("D4");

                        // 更新数据库中的最大流水号MaxSerialNumber
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
                        //2.如果没有流水号，插入"月起始单据号"，返回"月起始单据号"

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
