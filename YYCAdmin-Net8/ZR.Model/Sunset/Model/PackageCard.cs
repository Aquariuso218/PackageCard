using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Business.Model
{
    [SugarTable("base_PackageCard ", "包装卡主表")]
    [Tenant("0")]
    public class PackageCard
    {
        /// <summary>
        /// id
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public int Id { get; set; }

        /// <summary>
        /// 箱号
        /// </summary>
        public string boxNumber { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string  invCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string invName { get; set; }

        /// <summary>
        /// 产品图号
        /// </summary>
        public string pictureCode { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int quantity { get; set; }

        /// <summary>
        /// 打印次数
        /// </summary>
         [SugarColumn(IsOnlyIgnoreInsert = true)] //插入时忽略
        public string printCount { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string createBy { get; set; }

        public DateTime createdTime {  get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [SugarColumn(IsOnlyIgnoreInsert = true)] //插入时忽略
        public DateTime modifiedTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SugarColumn(IsOnlyIgnoreInsert = true)] //插入时忽略
        public string modifiedBy { get; set; }

        #region 表额外字段

        /// <summary>
        /// 明细数据
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<PackageCardDetails> Details { get; set; }

        #endregion

    }
}
