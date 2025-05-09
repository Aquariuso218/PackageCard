using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Business.Model
{
    [SugarTable("v_ProcessFlowInspection", "产品列表")]
    [Tenant("0")]
    public class SimulatedData
    {

        /// <summary>
        /// 产品ID
        /// </summary>
        [SugarColumn(ColumnName = "Define10")]
        public string id { get; set; }

        /// <summary>
        /// 存货父级编码
        /// </summary>
        [SugarColumn(ColumnName = "cInvcCode")]
        public string cInvcCode { get; set; }

        /// <summary>
        /// 流转卡
        /// </summary>
        [SugarColumn(ColumnName = "DocCode")]
        public string flowCard { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        [SugarColumn(ColumnName = "InvCode")]
        public string invCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [SugarColumn(ColumnName = "cInvName")]
        public string invName { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [SugarColumn(ColumnName = "cInvStd")]
        public string cInvStd { get; set; }

        /// <summary>
        /// 合格数量
        /// </summary>
        [SugarColumn(ColumnName = "QualifiedQty")]
        public int quantity { get; set; }

        /// <summary>
        /// 合箱编码
        /// </summary>
        [SugarColumn(ColumnName = "cInvDefine1")]
        public string mergeBoxCode { get; set; }

        /// <summary>
        /// 产品图号
        /// </summary>
        [SugarColumn(ColumnName = "cInvAddCode")]
        public string pictureCode { get; set; }

        /// <summary>
        /// 生产订单行ID
        /// </summary>
        [SugarColumn(ColumnName = "MoDId")]
        public string MoDId { get ; set; }

        /// <summary>
        /// 流转卡ID
        /// </summary>
        [SugarColumn(ColumnName = "PFId")]
        public string PFId { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [SugarColumn(ColumnName = "cInvDefine3")]
        public string customerName { get; set; }

        /// <summary>
        /// 报工时间
        /// </summary>
        [SugarColumn(ColumnName = "docTime")]
        public string docDate { get; set; }

        [SugarColumn(IsIgnore = true)] //映射时忽略
        public DateTime? beginTime { get; set; }

        [SugarColumn(IsIgnore = true)] //映射时忽略
        public DateTime? endTime { get; set; }

    }
}
