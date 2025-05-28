using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model.Dto
{
    public class Materialappvouch
    {
        /// <summary>
        /// 产品编码
        /// </summary>
        public string InvCode { get; set; }

        /// <summary>
        /// 产品数量
        /// </summary>
        public string Qty { get; set; }

        /// <summary>
        /// 生产订单行ID
        /// </summary>
        public string MoDId { get; set; }

        /// <summary>
        /// 生产部门
        /// </summary>
        public string MDeptCode { get; set; }
        /// <summary>
        /// 生产订单号
        /// </summary>
        public string MoCode { get; set; }
        /// <summary>
        /// 生产订单行号
        /// </summary>
        public string SortSeq { get; set; }
        
        /// <summary>
        /// 子件编码
        /// </summary>
        public string zInvCode { get; set; }

        /// <summary>
        /// 子件数量
        /// </summary>
        public string cAppQty { get; set; }
        /// <summary>
        /// 需求日期
        /// </summary>
        public string SoType { get; set; }

        /// <summary>
        /// 需求日期
        /// </summary>
        public string SoDId { get; set; }

        /// <summary>
        /// 需求日期
        /// </summary>
        public string SoCode { get; set; }

        /// <summary>
        /// 需求日期
        /// </summary>
        public string SoSeq { get; set; }

        /// <summary>
        /// 生产订单子件用料表id
        /// </summary>
        public string AllocateId { get; set; }
        /// <summary>
        /// 需求日期
        /// </summary>
        public string dDueDate { get; set; }
        /// <summary>
        /// 工序行号
        /// </summary>
        public string iopseq { get; set; }
        /// <summary>
        /// 子件自由项
        /// </summary>
        public string Free1 { get; set; }


    }
}
