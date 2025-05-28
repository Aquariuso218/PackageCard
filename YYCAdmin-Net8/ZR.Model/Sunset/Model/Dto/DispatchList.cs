using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model.Dto
{
    // 发货单类
    public class DispatchList
    {
        /// <summary>
        /// 制单人
        /// </summary>
        public string Cmaker { get; set; }

        /// <summary>
        /// 发货单明细集合
        /// </summary>
        public List<DispatchListDetail> DispatchDetails { get; set; }

    }

    // 发货单明细类
    public class DispatchListDetail
    {
        /// <summary>
        /// 发货单号
        /// </summary>
        public string cDLCode { get; set; }
        public string iDLsID { get; set; }
        public string CInvCode { get; set; }
        public string CInvName { get; set; }
        public string cWhCode { get; set; }
        public decimal IQuantity { get; set; }
        public string CBatch { get; set; }
        public string irowno { get; set; }
    }
}