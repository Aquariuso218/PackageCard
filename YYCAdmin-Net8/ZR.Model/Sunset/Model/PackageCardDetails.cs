using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Business.Model
{
    [SugarTable("base_PackageCardDetails ", "包装卡子表")]
    [Tenant("0")]
    public class PackageCardDetails
    {
        /// <summary>
        /// id
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public int id { get; set; }

        /// <summary>
        /// 箱号
        /// </summary>
        public string boxNumber { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string invCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string invName { get; set; }

        /// <summary>
        /// 流转卡号
        /// </summary>
        public string flowCard { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int quantity { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string createdBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createdTime {  get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string customerName {  get; set; }

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
        /// 产品图号
        /// </summary>
        public string invAddCode {  get; set; }

        /// <summary>
        /// 是否入库标识
        /// </summary>
        public int isFlag { get; set; }


        public string RDCode { get; set; }

        public string RDTime { get; set; }

        public string RDMaker { get; set; }  

    }
}
