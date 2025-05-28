using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model
{
    [SugarTable("Warehouse ", "仓库表")]
    [Tenant("1")]
    public class Warehouse
    {
        /// <summary>
        /// 仓库编码
        /// </summary>
        public string cWhCode { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string cWhName { get; set; }
    }
}
