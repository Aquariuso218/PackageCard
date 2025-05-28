using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Sunset.Model.Dto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZR.Service.Sunset
{
    public class PackingUntillService
    {
        public static string connectionStringU8;

        public PackingUntillService()
        {
            // 构建配置
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 设置 appsettings.json 所在的目录
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // 加载 appsettings.json 文件
                .Build();

            // 获取 dbConfigs 数组中索引为 1 的 Conn 值
            connectionStringU8 = config.GetSection("dbConfigs")
                .GetChildren() // 获取 dbConfigs 数组的所有子节点
                .ElementAt(1)  // 选择索引为 1 的项（第二个元素）
                .GetSection("Conn") // 获取该项的 Conn 键
                .Value; // 获取值
        }
    }
}
