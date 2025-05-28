using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model.Dto
{
    public class rdrecords10
    {
        /// <summary>
        /// 箱号
        /// </summary>
        public string boxNumber { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string invCode { get; set; }

        /// <summary>
        /// 流转卡号
        /// </summary>
        public string flowCard { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int quantity { get; set; }

        /// <summary>
        /// 产品id号
        /// </summary>
        public string invID { get; set; }

        /// <summary>
        /// 生产订单行ID
        /// </summary>
        public string MoDId { get; set; }

        /// <summary>
        /// 流转卡ID
        /// </summary>
        public string PFId { get; set; }

        /// <summary>
        /// 生产部门
        /// </summary>
        public string MDeptCode { get; set; }

        /// <summary>
        /// 是否启用批次
        /// </summary>
        public string bInvBatch { get; set; }
        /// <summary>
        /// 生产订单号
        /// </summary>
        public string MoCode { get; set; }
        /// <summary>
        /// 自由项1
        /// </summary>
        public string Free1 { get; set; }
        /// <summary>
        /// 生产订单id
        /// </summary>
        public string MoId { get; set; }

    }
}
