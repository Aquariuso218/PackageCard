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

        public DateTime createdTime {  get; set; }

        public string customerName {  get; set; }

    }
}
