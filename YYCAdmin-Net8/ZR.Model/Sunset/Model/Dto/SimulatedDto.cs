using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model.Dto
{
    public class SimulatedDto
    {
        public string id { get; set; }

        /// <summary>
        /// 流转卡
        /// </summary>
        public string flowCard { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string invCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string invName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int quantity { get; set; }

        /// <summary>
        /// 是否装箱
        /// </summary>
        public int isflag { get; set; }

        /// <summary>
        /// 合箱编码
        /// </summary>
        public string mergeBoxCode { get; set; }

        /// <summary>
        /// 箱号
        /// </summary>
        public string BoxNumber { get; set; }

        /// <summary>
        /// 产品图号
        /// </summary>
        public string pictureCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string customerName { get; set; }

        /// <summary>
        /// 生产订单行ID
        /// </summary>
        public string MoDId { get; set; }

        /// <summary>
        /// 流转卡ID
        /// </summary>
        public string PFId { get; set; }
    }
}
