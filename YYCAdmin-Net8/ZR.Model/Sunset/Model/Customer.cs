using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model
{
    [SugarTable("Customer ", "客户表")]
    [Tenant("1")]
    public class Customer
    {
        /// <summary>
        /// 客户编码
        /// </summary>
        public string cCusCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string cCusName { get; set; }
    }
}
