using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Business.Model;

namespace ZR.Model.Sunset.Model.Dto
{
    public class SalesInvoiceQuery
    {
        /// <summary>
        /// 制单人
        /// </summary>
        public string cMaker { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string cWhCode { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string cCusCode { get; set; }

        /// <summary>
        /// 产品列表
        /// </summary>
        public List<PackageCardDetails> PCDList { get; set; }
    }
}
