using Infrastructure.Attribute;
using Infrastructure.Model;
using JinianNet.JNTemplate.Nodes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using SqlSugar.IOC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utilities.Base.Data;
using ZR.Model.Business.Model;
using ZR.Model.Sunset.Model;
using ZR.Model.Sunset.Model.Dto;
using ZR.Repository;
using ZR.Service.IService;

namespace ZR.Service
{
    [AppService(ServiceType = typeof(IRdrecord10Service), ServiceLifetime = LifeTime.Transient)]
    public class Rdrecord10Service : IRdrecord10Service, IDynamicApi
    {

        private readonly ISqlSugarClient db;
        private readonly ISqlSugarClient U8db;
        private readonly IConfiguration config;

        public Rdrecord10Service()
        {
            // 构建配置
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 设置 appsettings.json 所在的目录
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // 加载 appsettings.json 文件
                .Build();

            db = DbScoped.SugarScope.GetConnectionScope(0);
            U8db = DbScoped.SugarScope.GetConnectionScope(1);
        }

        /// <summary>
        /// 扫描查询箱子详情列表
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        public ApiResult getBoxDetails(string barCode)
        {
            string sql = @"SELECT b.id,b.PFId,b.MoDId,b.boxNumber,b.flowCard,b.invID,b.invCode,b.invName,b.invAddCode,b.quantity,b.isFlag FROM base_packageCard A LEFT JOIN base_packageCardDetails B ON A.boxNumber = B.boxNumber WHERE a.barCode=@barCode  ORDER  by b.createdTime asc";

            var result = db.Ado.SqlQuery<PackageCardDetails>(sql, new { barCode });
            if (result.Count > 0)
            {
                return new ApiResult(1, "", result);
            }
            else
            {
                return new ApiResult(0, "未查询到条码信息");
            }
        }

        /// <summary>
        /// 入库产品列表
        /// </summary>
        /// <returns></returns>
        public ApiResult getStockInList(string startTime, string endTime, string invCode, string keyWord)
        {
            string U8Code = config.GetSection("U8Configs").GetSection("U8Code").Value;


            // 添加筛选条件
            List<string> conditions = new List<string>();
            string where = "";

            if (!string.IsNullOrEmpty(startTime))
            {
                conditions.Add("DocDate >= '" + startTime + "'");
            }

            if (!string.IsNullOrEmpty(endTime))
            {
                conditions.Add("DocDate <= '" + endTime + "'");
            }

            if (!string.IsNullOrEmpty(invCode))
            {
                conditions.Add("invCode LIKE '%" + invCode + "%'");
            }

            if (!string.IsNullOrEmpty(keyWord))
            {
                conditions.Add("(invID LIKE '%" + keyWord + "%' OR invAddCode LIKE '%" + keyWord + "%' OR invName LIKE '%" + keyWord + "%' OR flowCard LIKE '%" + keyWord + "%' OR boxNumber LIKE '%" + keyWord + "%')");
            }

            // 合并所有条件到子查询中
            if (conditions.Any())
            {
                where += " AND " + string.Join(" AND ", conditions);
            }

            // 构建基础SQL语句  //更改为sfc_pfreport的主表时间
            string sql = $"SELECT * FROM (SELECT b.DocDate, a.* FROM (SELECT * FROM base_packageCardDetails WHERE isFlag = 0) a LEFT JOIN (SELECT * FROM (SELECT b.PFId, c.PFDId, c.Description, MAX(e.DocDate) DocDate, SUM(d.QualifiedQty) QualifiedQty FROM [{U8Code}].[dbo].[sfc_processflow] b (nolock) LEFT JOIN [{U8Code}].[dbo].[sfc_processflowdetail] c (nolock) ON b.PFId = c.PFId LEFT JOIN [{U8Code}].[dbo].[sfc_pfreportdetail] d (nolock) ON c.PFDId = d.PFDId LEFT JOIN [{U8Code}].[dbo].[sfc_pfreport] e (nolock) ON d.PFReportId = e.PFReportId LEFT JOIN [{U8Code}].[dbo].[sfc_moroutingdetail] f (nolock) ON c.MoRoutingDId = f.MoRoutingDId WHERE e.DocDate > '2025-05-12' AND f.LastFlag = 1 GROUP BY b.PFId, c.PFDId, c.Description ) BB WHERE QualifiedQty = 1) b ON a.PFId = b.PFId WHERE isnull(b.DocDate, '1901-01-01') <> '1901-01-01') RankedData WHERE 1 = 1 {where} ORDER BY DocDate";

            var result = db.Ado.GetDataSetAll(sql);

            List<PackageCardDetailsDto> PCDList = new List<PackageCardDetailsDto>();

            if (result != null && result.Tables.Count > 0)
            {
                foreach (DataRow row in result.Tables[0].Rows)
                {
                    PCDList.Add(new PackageCardDetailsDto
                    {
                        id = Convert.ToInt32(row["id"]),
                        boxNumber = row["boxNumber"]?.ToString(),
                        invCode = row["invCode"]?.ToString(),
                        invName = row["invName"]?.ToString(),
                        flowCard = row["flowCard"]?.ToString(),
                        quantity = Convert.ToInt32(row["quantity"]),
                        createdBy = row["createdBy"]?.ToString(),
                        createdTime = Convert.ToDateTime(row["createdTime"]),
                        invID = row["invID"]?.ToString(),
                        invAddCode = row["invAddCode"]?.ToString(),
                        isFlag = Convert.ToInt32(row["isFlag"]),
                        reportTime = row["DocDate"]?.ToString(),
                        PFId = row["PFId"]?.ToString(),
                        MoDId = row["MoDId"]?.ToString(),
                        isChange = Convert.ToInt32(row["isChange"])
                    });
                }
            }

            if (PCDList.Count > 0)
            {
                return new ApiResult(1, "成功", PCDList);
            }
            else
            {
                return new ApiResult(0, "暂无待入库数据");
            }
        }


        /// <summary>
        /// 待出库产品列表
        /// </summary>
        /// <returns></returns>
        public ApiResult getStockOutList(string cDLCode, string startDate, string endDate,string keyWord)
        {

            string where = "";
            if (!string.IsNullOrEmpty(cDLCode)) {
                where = $" AND cDLCode LIKE '%{cDLCode}%'";
            }
            if (!string.IsNullOrEmpty(startDate))
            {
                where += $" AND dDate >= '{startDate}'";
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                where += $" AND dDate <= '{endDate}'";
            }
            if (!string.IsNullOrEmpty(keyWord)) {
                where += $" AND( cDLCode LIKE '%{keyWord}%' OR cCusName LIKE '%{keyWord}%' OR cInvCode LIKE '%{keyWord}%' OR cInvName LIKE '%{keyWord}%' OR cBatch LIKE '%{keyWord}%')";
            }

            string sql = $"SELECT cDLCode,dDate,cCusName,iDLsID,cInvCode, cInvName,cWhCode,iQuantity,cBatch,irowno,cCKQty from v_stockOutList where 1=1 {where} ORDER BY dDate DESC";

            var result = db.Ado.SqlQuery<DispatchListDto>(sql);
            if (result.Count > 0)
            {
                return new ApiResult(1, "", result);
            }
            else
            {
                return new ApiResult(0, "暂无待出库产品");
            }
        }

        public PagedInfo<Warehouse> wareHouseList(string text, PagerInfo pager)
        {
            var exp = Expressionable.Create<Warehouse>();

            if (!string.IsNullOrEmpty(text))//实现模糊查询
            {
                exp.And(w => w.cWhCode.Contains(text) || w.cWhName.Contains(text));
            }

            var query = U8db.Queryable<Warehouse>().Where(exp.ToExpression());

            var wareList = query.ToPage(pager);

            return wareList;
        }

        public PagedInfo<Customer> customerList(string text, PagerInfo pager)
        {
            var exp = Expressionable.Create<Customer>();

            if (!string.IsNullOrEmpty(text))
            {
                exp.And(c => c.cCusCode.Contains(text) || c.cCusName.Contains(text));
            }

            var query = U8db.Queryable<Customer>().Where(exp.ToExpression());

            var customerList = query.ToPage(pager);

            return customerList;
        }

        /// <summary>
        /// 产成品入库
        /// </summary>
        /// <param name="salesInvoiceQuery"></param>
        /// <returns></returns>
        public ApiResult stockIn(SalesInvoiceQuery salesInvoiceQuery)
        {
            string cMaker = salesInvoiceQuery.cMaker;
            string _cWhCode = salesInvoiceQuery.cWhCode;
            List<PackageCardDetails> PCDList = salesInvoiceQuery.PCDList;
            try
            {
                // 获取 U8Configs 中U8Code的值
                string U8Code = config.GetSection("U8Configs").GetSection("U8Code").Value; // 获取该项的Conn键 再获取值
                string ZTCode = config.GetSection("U8Configs").GetSection("ZTCode").Value;

                //仓库
                //string _cWhCode = "20";
                //货位
                string _cPosCode = "";

                //制单人
                if (string.IsNullOrWhiteSpace(cMaker))
                {
                    return new ApiResult(0, "制单人不允许为空!");
                }

                string bWhPos = "0";
                string cWhCode = "";
                //仓库
                if (string.IsNullOrWhiteSpace(_cWhCode))
                {
                    return new ApiResult(0, "仓库不允许为空!");
                }
                else
                {
                    string sql = "select * from WareHouse (nolock) where cWhCode='" + _cWhCode + "'";
                    DataTable dt = U8db.Ado.GetDataTable(sql);
                    if (DataHelper.IsExistRows(dt))
                    {
                        return new ApiResult(0, "仓库[" + _cWhCode + "]在U8系统中不存在!");
                    }
                    else
                    {
                        if (dt.Rows[0]["bWhPos"].ToString() == "True")
                        {
                            bWhPos = "1";
                        }
                        cWhCode = dt.Rows[0]["cWhCode"].ToString();
                    }
                }

                //不允许仓库启用货位管理
                string iposflag = "NULL";
                string cPosCode = "";
                string cPosName = "";
                if (bWhPos == "1")
                {
                    return new ApiResult(0, "仓库[" + cWhCode + "]设置了货位管理，请取消货位管理!");
                }

                //是否记账标识
                string bCosting = U8db.Ado.GetString("select (case when bInCost=1 then 1 else 0 end)bInCost from Warehouse (nolock) where cWhCode='" + cWhCode + "'");


                List<string> list = new List<string>();
                List<string> _list = new List<string>();
                Hashtable ht = new Hashtable();
                List<string> listBatch = new List<string>();

                if (PCDList != null && PCDList.Count > 0)
                {
                    Dictionary<string, decimal> dicmodid = new Dictionary<string, decimal>();

                    List<rdrecords10> listrds10 = new List<rdrecords10>();

                    foreach (PackageCardDetails rdrecord10 in PCDList)
                    {
                        string cInvCode = rdrecord10.invCode;
                        string bInvBatch = "0";
                        //存货编码
                        if (string.IsNullOrWhiteSpace(cInvCode))
                        {
                            return new ApiResult(-1, "存货编码不允许为空!");
                        }
                        else
                        {
                            string sqlcinv = "select cInvCode,bInvBatch from Inventory (nolock) where cInvCode='" + cInvCode + "'";
                            DataTable cinvds = U8db.Ado.GetDataTable(sqlcinv);
                            if (DataHelper.IsExistRows(cinvds))
                            {
                                return new ApiResult(-1, "存货编码[" + cInvCode + "]在U8系统中不存在!");
                            }
                            if (cinvds.Rows[0]["bInvBatch"].ToString() == "True")
                            {
                                bInvBatch = "1";
                            }
                        }

                        decimal iQuantity = 0;
                        if (rdrecord10.quantity.ToString() == "0" || string.IsNullOrWhiteSpace(rdrecord10.quantity.ToString()))
                        {
                            return new ApiResult(0, "数量不允许为空或者0!");
                        }
                        else
                        {
                            if (!decimal.TryParse(rdrecord10.quantity.ToString(), out iQuantity))
                            {
                                return new ApiResult(0, "数量格式不正确!");
                            }
                        }

                        //查询 生产订单号:MoCode 生产部门:MDeptCode
                        DataSet MomSet = U8db.Ado.GetDataSetAll(@"SELECT TOP 1 A.MoId, A.MoCode, MDeptCode, WcCode,B.SortSeq,D.OpSeq,D.Description,B.SoDId,SoType,SoCode,SoSeq,MolotCode, B.Free1 FROM mom_order A (nolock) LEFT JOIN mom_orderdetail B (nolock) ON A.MoId = B.MoId LEFT JOIN sfc_processflow C (nolock) ON B.MoDId = C.MoDId LEFT JOIN sfc_processflowdetail D (nolock) ON C.PFId= D.PFId LEFT JOIN sfc_workcenter E (nolock) ON D.WcId  = E.WcId  WHERE B.MoDId = '" + rdrecord10.MoDId + "' ORDER BY OpSeq");

                        if (MomSet != null && MomSet.Tables.Count > 0 && MomSet.Tables[0].Rows.Count > 0)
                        {
                            #region 校验流转卡是否报工完成
                            //查询该流转卡最后一序是否报工完成
                            string sqlaaas = "select top 1 QualifiedQty from (select a.PFDId,a.OpSeq,sum(b.QualifiedQty)QualifiedQty from sfc_processflowdetail a (nolock) left join sfc_pfreportdetail b (nolock) on a.PFDId=b.PFDId where PFId='" + rdrecord10.PFId + "' group by a.PFDId,a.OpSeq)te1 order by  OpSeq desc";

                            int lastNum = (int)U8db.Ado.GetDecimal(sqlaaas);
                            if (lastNum < rdrecord10.quantity)
                            {
                                return new ApiResult(-1, $"{rdrecord10.flowCard} 未完成报工，不允许入库!");
                            }
                            #endregion

                            ////校验是否领料完成
                            //string str1 = "select COUNT(1) from mom_moallocate (nolock) where IssQty<Qty and MoDId='" + rdrecord10.MoDId + "'";
                            //int thuum = (int)U8db.Ado.GetDecimal(str1);
                            //if (thuum > 0)
                            //{
                            //    return new ApiResult(-1, $"{rdrecord10.flowCard} 领料未完成，不允许入库!");
                            //}

                            rdrecords10 rds10 = new rdrecords10();

                            if (MomSet.Tables[0].Rows[0]["MDeptCode"].ToString() == "")
                            {

                                rds10.MDeptCode = "01";
                            }
                            else
                            {
                                rds10.MDeptCode = MomSet.Tables[0].Rows[0]["MDeptCode"].ToString();
                            }

                            rds10.invCode = cInvCode;
                            rds10.flowCard = rdrecord10.flowCard;
                            rds10.boxNumber = rdrecord10.boxNumber;
                            rds10.PFId = rdrecord10.PFId;
                            rds10.MoDId = rdrecord10.MoDId;
                            rds10.quantity = rdrecord10.quantity;
                            rds10.invID = rdrecord10.invID;
                            rds10.bInvBatch = bInvBatch;
                            rds10.MoCode = MomSet.Tables[0].Rows[0]["MoCode"].ToString();
                            rds10.MoId = MomSet.Tables[0].Rows[0]["MoId"].ToString();
                            rds10.Free1 = MomSet.Tables[0].Rows[0]["Free1"].ToString();

                            listrds10.Add(rds10);

                        }
                        else
                        {
                            return new ApiResult(-1, $"{rdrecord10.flowCard} 不存在，请检查!");
                        }
                    }

                    var grouprd10 = listrds10.GroupBy(item => new
                    {
                        item.MoId,
                        item.MoCode,
                        item.MDeptCode
                    });

                    foreach (var geoupitem in grouprd10)
                    {
                        #region 获取ID
                        string sqlId = @"declare @p5 int
                                        declare @p6 int
                                        exec sp_GetId N'',N'" + ZTCode + "',N'rd'," + geoupitem.Count() + ",@p5 output,@p6 output,default select @p5, @p6";
                        DataSet dsID = U8db.Ado.GetDataSetAll(sqlId);
                        string Id = dsID.Tables[0].Rows[0][0].ToString();
                        #endregion

                        string cCode = Rd10code();
                        //
                        list.Add("insert into rdrecord10 ([ID],[bRdFlag],[cVouchType],[cBusType],[cSource],[cBusCode],[cWhCode],[dDate],[cCode],[cRdCode],[cDepCode],[cPersonCode],[cPTCode],[cSTCode],[cCusCode],[cVenCode],[cOrderCode],[cARVCode],[cBillCode],[cDLCode],[cProBatch],[cHandler],[cMemo],[bTransFlag],[cAccounter],[cMaker],[cDefine1],[cDefine2],[cDefine3],[cDefine4],[cDefine5],[cDefine6],[cDefine7],[cDefine8],[cDefine9],[cDefine10],[dKeepDate],[dVeriDate],[bpufirst],[biafirst],[iMQuantity],[dARVDate],[cChkCode],[dChkDate],[cChkPerson],[VT_ID],[bIsSTQc],[cDefine11],[cDefine12],[cDefine13],[cDefine14],[cDefine15],[cDefine16],[cMPoCode],[gspcheck],[ipurorderid],[iproorderid],[iExchRate],[cExch_Name],[bOMFirst],[bFromPreYear],[bIsLsQuery],[bIsComplement],[iDiscountTaxType],[ireturncount],[iverifystate],[iswfcontrolled],[cModifyPerson],[dModifyDate],[dnmaketime],[dnmodifytime],[dnverifytime],[bredvouch],[iFlowId],[cPZID],[cSourceLs],[cSourceCodeLs],[iPrintCount],[csysbarcode],[cCurrentAuditor],[outid],[cCheckSignFlag]) values('" + Id + "', 1, '10', '成品入库', '生产订单', Null, '" + cWhCode + "', convert(varchar(10),getdate(),120), '" + cCode + "', '102', '" + geoupitem.Key.MDeptCode + "', Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, '" + cMaker + "', Null, '0', Null, '" + cMaker + "', Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, convert(varchar(10),getdate(),120), '0', '0', Null, Null, Null, Null, Null, 63, '0', Null, Null, Null, 'Y', Null, Null, '" + geoupitem.Key.MoCode + "', Null, Null, '" + geoupitem.Key.MoId + "', Null, Null, '0', '0', '0', 0, 0, 0, 0, 0, '', Null,getdate(), Null, getdate(), 0, Null, Null, Null, Null, 0, '||st10|" + cCode + "', Null, Null, '" + Guid.NewGuid().ToString().Replace("-", "") + "')");


                        int zbid = int.Parse(dsID.Tables[0].Rows[0][1].ToString()) - geoupitem.Count();
                        int irowno = 1;

                        foreach (var item in geoupitem)
                        {

                            string cInvCode = item.invCode;
                            string PFId = item.PFId;

                            //校验生产订单是否已经入库完成
                            int counts = U8db.Ado.GetInt("select count(*) from rdrecords10 (nolock) where cBatch='" + item.invID + "' and cInvCode='" + cInvCode + "' and iMPoIds='" + item.MoDId + "'");

                            if (counts > 0)
                            {
                                //直接修改明细流转卡id号的入库标识
                                _list.Add("UPDATE base_packageCardDetails SET  isflag = 1 WHERE PFId = '" + item.PFId + "'");
                            }
                            else
                            {

                                int iQuantity = item.quantity;

                                //可入库数量
                                string strcc = "select sum(isnull(BalQualifiedQty,0))BalQualifiedQty from sfc_moroutingdetail (nolock) where MoDId = '" + item.MoDId + "' and LastFlag = '1'";
                                decimal BalQualifiedQty = U8db.Ado.GetDecimal(strcc);
                                if (BalQualifiedQty < item.quantity)
                                {
                                    return new ApiResult(-1, $"{item.flowCard} 可入库数量不足，不允许入库! {item.MoDId}");
                                }

                                #region 处理现存量
                                //批次号
                                string cBatch = "";

                                #region 获取itemid
                                string itemid = "";
                                string sqlitemid = "select * from dbo.SCM_Item (nolock) where cInvCode='" + item.invCode + "' AND cFree1 = '" + item.Free1 + "'";
                                DataTable dtitemid = U8db.Ado.GetDataTable(sqlitemid);
                                if (DataHelper.IsExistRows(dtitemid))
                                {
                                    U8db.Ado.ExecuteCommand("insert into dbo.scm_item ([cInvCode],[cFree1],[cFree2],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[PartId]) values('" + item.invCode + "',' " + item.Free1 + "','','','','','','','','','',0)");
                                    //重新获取itemid
                                    itemid = U8db.Ado.GetDataTable(sqlitemid).Rows[0]["id"].ToString();
                                }
                                else
                                {
                                    //获取itemid
                                    itemid = dtitemid.Rows[0]["id"].ToString();
                                }
                                #endregion

                                //启用批次
                                if (item.bInvBatch == "1")
                                {
                                    cBatch = item.invID;
                                    //判断批次档案是否存在，不存在则新增
                                    string sqlcbat = "select cInvCode from AA_BatchProperty (nolock) where cBatch='" + cBatch + "' and cInvCode='" + cInvCode + "' AND cFree1 = '" + item.Free1 + "'";
                                    DataSet dsbat = U8db.Ado.GetDataSetAll(sqlcbat);
                                    if (dsbat.Tables[0].Rows.Count == 0 && !listBatch.Contains(cInvCode + cBatch))
                                    {
                                        listBatch.Add(cInvCode + cBatch);
                                        list.Add("insert into AA_BatchProperty(cBatchPropertyGUID,cInvCode,cBatch,cFree1,cFree2,cFree3,cFree4,cFree5,cFree6,cFree7,cFree8,cFree9,cFree10,cBatchProperty6,cBatchProperty7,cBatchProperty8,cBatchProperty9) values(newid(),'" + cInvCode + "','" + cBatch + "','" + item.Free1 + "','','','','','','','','','','" + item.boxNumber + "','','','')");
                                    }
                                }
                                else
                                {
                                    return new ApiResult(-1, $"产品 {item.invCode} 未启用批次！");
                                }
                                #endregion

                                #region 现存量表
                                string sqlxcl = "select * from CurrentStock (nolock) where cInvCode='" + cInvCode + "' and cWhCode='" + cWhCode + "' and cBatch='" + cBatch + "' AND cFree1 = '" + item.Free1 + "'";
                                DataTable dtxcl = U8db.Ado.GetDataTable(sqlxcl);
                                if (DataHelper.IsExistRows(dtxcl))
                                {
                                    //如果现存量记录不存在则新增
                                    U8db.Ado.ExecuteCommand(@"insert into dbo.CurrentStock([cWhCode],[cInvCode],[ItemId],[cBatch],[cVMIVenCode],[iSoType],[iSodid],[iQuantity],[iNum],[cFree1],[cFree2],[fOutQuantity],[fOutNum],[fInQuantity],[fInNum],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[dVDate],[bStopFlag],[fTransInQuantity],[dMdate],[fTransInNum],[fTransOutQuantity],[fTransOutNum] ,[fPlanQuantity] ,[fPlanNum],[fDisableQuantity],[fDisableNum],[fAvaQuantity],[fAvaNum],[iMassDate],[BGSPSTOP],[cMassUnit],[fStopQuantity] ,[fStopNum],[dLastCheckDate],[cCheckState],[dLastYearCheckDate],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[ipeqty],[ipenum]) values('" + cWhCode + "','" + cInvCode + "','" + itemid + "','" + cBatch + "','',0,'','0',0,'" + item.Free1 + "','',0,0,0,0,'','','','','','','','',NULL,0,0,NULL,0,0,0,0,0,0,0,0,0,NULL,0,NULL,0,0,NULL,'',NULL,'0',NULL,NULL,0,0)");
                                    //如果现存量记录存在则修改数量
                                }
                                list.Add("update CurrentStock set iQuantity=iQuantity+" + iQuantity + " where cInvCode='" + cInvCode + "' and cWhCode='" + cWhCode + "' and cBatch='" + cBatch + "' AND cFree1 = '" + item.Free1 + "'");
                                #endregion

                                #region 处理货位存量表
                                if (!string.IsNullOrWhiteSpace(cPosCode))
                                {
                                    string sqlhwcl = "select * from invpositionsum (nolock) where cWhCode='" + cWhCode + "' and cInvCode='" + cInvCode + "' and cPosCode='" + cPosCode + "' and cBatch='" + cBatch + "' AND cFree1 = '" + item.Free1 + "'";
                                    DataTable hwcldt = U8db.Ado.GetDataTable(sqlhwcl);
                                    if (DataHelper.IsExistRows(hwcldt))
                                    {
                                        U8db.Ado.ExecuteCommand("insert into invpositionsum ([cWhCode],[cPosCode],[cInvCode],[iQuantity],[inum],[cBatch],[cFree1],[cFree2],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[iTrackid],[cvmivencode],[cMassUnit],[iMassDate],[dMadeDate],[dVDate],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cInVouchType]) values('" + cWhCode + "','" + cPosCode + "','" + cInvCode + "',0,NULL,'" + cBatch + "','" + item.Free1 + "','','','','','','','','','','0','',Null,Null,Null,Null,0,Null,Null,'')");
                                    }
                                    list.Add("update invpositionsum set iQuantity=iQuantity+'" + iQuantity + "' where cWhCode='" + cWhCode + "' and cInvCode='" + cInvCode + "' and cPosCode='" + cPosCode + "' and cBatch='" + cBatch + "' AND cFree1 = '" + item.Free1 + "'");
                                    list.Add("insert into invposition ([RdsID],[RdID],[cWhCode],[cPosCode],[cInvCode],[cBatch],[cFree1],[cFree2],[dVDate],[iQuantity],[iNum],[cMemo],[cHandler],[dDate],[bRdFlag],[cSource],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[cAssUnit],[cBVencode],[iTrackId],[dMadeDate],[iMassDate],[cMassUnit],[cvmivencode],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cvouchtype],[cInVouchType],[cVerifier],[dVeriDate],[dVouchDate]) values('" + zbid + "','" + Id + "','" + cWhCode + "','" + cPosCode + "','" + cInvCode + "','" + cBatch + "','" + item.Free1 + "','',Null,'" + iQuantity + "',Null,'','" + cMaker + "',convert(varchar(10),getdate(),120),1,'','','','','','','','','',Null,Null,0,Null,Null,Null,'',0,Null,Null,'10','',Null,Null,convert(varchar(10),getdate(),120))");
                                }
                                #endregion


                                //查询 生产订单号:MoCode 生产部门:MDeptCode
                                DataSet MomSet = U8db.Ado.GetDataSetAll(@"SELECT TOP 1 A.MoId, A.MoCode, MDeptCode, WcCode,B.SortSeq,D.OpSeq,D.Description,B.SoDId,B.SoType,B.SoCode,B.SoSeq,MolotCode, B.Free1 FROM mom_order A (nolock) LEFT JOIN mom_orderdetail B (nolock) ON A.MoId = B.MoId LEFT JOIN sfc_processflow C (nolock) ON B.MoDId = C.MoDId LEFT JOIN sfc_processflowdetail D (nolock) ON C.PFId= D.PFId LEFT JOIN sfc_workcenter E (nolock) ON D.WcId  = E.WcId  WHERE B.MoDId = '" + item.MoDId + "' ORDER BY OpSeq");

                                string WcCode = MomSet.Tables[0].Rows[0]["WcCode"].ToString();
                                string SortSeq = MomSet.Tables[0].Rows[0]["SortSeq"].ToString();
                                string OpSeq = MomSet.Tables[0].Rows[0]["OpSeq"].ToString();
                                string Description = MomSet.Tables[0].Rows[0]["Description"].ToString();
                                string SoDId = MomSet.Tables[0].Rows[0]["SoDId"].ToString();
                                string SoType = MomSet.Tables[0].Rows[0]["SoType"].ToString();
                                string SoCode = MomSet.Tables[0].Rows[0]["SoCode"].ToString();
                                string SoSeq = MomSet.Tables[0].Rows[0]["SoSeq"].ToString();
                                string MolotCode = MomSet.Tables[0].Rows[0]["MolotCode"].ToString();

                                list.Add(@"insert into rdrecords10 ([AutoID],[ID],[cInvCode],[iNum],[iQuantity],[iUnitCost],[iPrice],[iAPrice],[iPUnitCost],[iPPrice],[cBatch],[cVouchCode],[cInVouchCode],[cinvouchtype],[iSOutQuantity],[iSOutNum],[cFree1],[cFree2],[iFlag],[iFNum],[iFQuantity],[dVDate],[cPosition],[cDefine22],[cDefine23],[cDefine24],[cDefine25],[cDefine26],[cDefine27],[cItem_class],[cItemCode],[cName],[cItemCName],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[cBarCode],[iNQuantity],[iNNum],[cAssUnit],[dMadeDate],[iMassDate],[cDefine28],[cDefine29],[cDefine30],[cDefine31],[cDefine32],[cDefine33],[cDefine34],[cDefine35],[cDefine36],[cDefine37],[iMPoIds],[iCheckIds],[cBVencode],[bGsp],[cGspState],[cCheckCode],[iCheckIdBaks],[cRejectCode],[iRejectIds],[cCheckPersonCode],[dCheckDate],[cMassUnit],[cMoLotCode],[bChecked],[bRelated],[cmworkcentercode],[bLPUseFree],[iRSRowNO],[iOriTrackID],[coritracktype],[cbaccounter],[dbKeepDate],[bCosting],[bVMIUsed],[iVMISettleQuantity],[iVMISettleNum],[cvmivencode],[iInvSNCount],[cwhpersoncode],[cwhpersonname],[cserviceoid],[cbserviceoid],[iinvexchrate],[corufts],[cmocode],[imoseq],[iopseq],[copdesc],[strContractGUID],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cciqbookcode],[iBondedSumQty],[productinids],[iorderdid],[iordertype],[iordercode],[iorderseq],[isodid],[isotype],[csocode],[isoseq],[cBatchProperty1],[cBatchProperty2],[cBatchProperty3],[cBatchProperty4],[cBatchProperty5],[cBatchProperty6],[cBatchProperty7],[cBatchProperty8],[cBatchProperty9],[cBatchProperty10],[cbMemo],[irowno],[strowguid],[cservicecode],[ipreuseqty],[ipreuseinum],[OutCopiedQuantity],[cbsysbarcode],[cplanlotcode],[bmergecheck],[imergecheckautoid],[iposflag],[iorderdetailid],[body_outid]) values('" + zbid + "','" + Id + "','" + item.invCode + "',Null,'" + iQuantity + "',Null,Null,Null,Null,Null,'" + cBatch + "',Null,Null,Null,Null,Null,'" + item.Free1 + "',Null,0,Null,Null,Null,'" + cPosCode + "','" + item.flowCard + "',Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,'" + iQuantity + "',Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,NULL,'" + item.MoDId + "',Null,Null,'0',Null,Null,Null,Null,Null,Null,Null,Null,Null,'0','0','" + WcCode + "','0',0,'0',Null,Null,Null,'" + bCosting + "','0',Null,Null,Null,Null,Null,Null,Null,Null,Null,convert(nvarchar,convert(money,@@DBTS),2),'" + item.MoCode + "','" + SortSeq + "','" + OpSeq + "','" + Description + "',Null,0,Null,Null,Null,Null,Null,'" + SoDId + "','" + SoType + "','" + SoCode + "','" + SoSeq + "','" + SoDId + "','" + SoType + "','" + SoCode + "','" + SoSeq + "',Null,Null,Null,Null,Null,'" + item.boxNumber + "',Null,Null,Null,Null,Null,'" + irowno + "',Null,Null,Null,Null,Null,'||st10|" + cCode + "|" + irowno + "','" + MolotCode + "','0',Null," + iposflag + ",Null,Null)");

                                if (bCosting == "1")
                                {
                                    //未记账单据列表
                                    list.Add("insert into IA_ST_UnAccountVouch10(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values('" + Id + "','" + zbid + "','10','成品入库') ");
                                }
                                //修改生产订单-入库累计数量
                                list.Add("update mom_orderdetail set QualifiedInQty=isnull(QualifiedInQty,0)+'" + iQuantity + "' where MoDId='" + item.MoDId + "'");
                                //修改生产订单工序资料可入库数量
                                list.Add("update sfc_moroutingdetail set BalQualifiedQty=BalQualifiedQty-'" + iQuantity + "'  where MoDId='" + item.MoDId + "' and LastFlag='1'");

                                //回写条码记录表
                                _list.Add("UPDATE base_packageCardDetails SET  isflag = 1, RDCode = '" + cCode + "', RDTime = '" + DateTime.Now.ToString() + "', RDMaker = '" + cMaker + "' WHERE PFId = '" + item.PFId + "'");

                                decimal val;
                                if (dicmodid.TryGetValue(item.MoDId, out val))
                                {

                                    dicmodid[item.MoDId] = dicmodid[item.MoDId] + iQuantity;
                                }
                                else
                                {
                                    dicmodid.Add(item.MoDId, iQuantity);
                                }

                                zbid++;

                                irowno++;
                            }
                        }
                    }


                    #region 验证订单是否超量入库
                    foreach (KeyValuePair<string, decimal> item in dicmodid)
                    {
                        string mosql = "select a.MoDId,MoCode,a.SortSeq,(a.Qty-isnull(a.QualifiedInQty,0))iQuantity from mom_orderdetail a (nolock) left join mom_order m (nolock) on m.MoId=a.MoId where MoDId='" + item.Key + "'";
                        DataSet dsmo = U8db.Ado.GetDataSetAll(mosql);
                        if (dsmo != null && dsmo.Tables.Count > 0 && dsmo.Tables[0].Rows.Count > 0)
                        {
                            if (item.Value > decimal.Parse(dsmo.Tables[0].Rows[0]["iQuantity"].ToString()))
                            {
                                return new ApiResult(-1, "生产订单号[" + dsmo.Tables[0].Rows[0]["MoCode"].ToString() + "]行号[" + dsmo.Tables[0].Rows[0]["SortSeq"].ToString() + "]不允许超量入库!");
                            }
                            else
                            {
                                string counts = U8db.Ado.GetString("select count(1) from mom_orderdetail a (nolock) inner join mom_moallocate b (nolock) on a.MoDId = b.MoDId where a.MoDId = '" + item.Key + "' and b.IssQty<>b.Qty and (isnull(a.QualifiedInQty, 0) +" + item.Value + ") * (BaseQtyN / BaseQtyD) > b.IssQty + 0.02");
                                if (int.Parse(counts) > 0)
                                {
                                    return new ApiResult(-1, "生产订单号[" + dsmo.Tables[0].Rows[0]["MoCode"].ToString() + "]行号[" + dsmo.Tables[0].Rows[0]["SortSeq"].ToString() + "]领用材料不足!");
                                }
                            }
                        }
                        else
                        {
                            return new ApiResult(-1, "生产订单明细id[" + item.Key + "]在U8中不存在");
                        }
                    }
                    #endregion

                    // 开启事务并循环执行list和_list中的sql语句 (list为U8db;_list为db),都执行成功时返回rs=true,否则返回rs=false
                    bool rs = false;
                    try
                    {
                        // 开启U8db事务
                        U8db.Ado.BeginTran();
                        // 开启db事务
                        db.Ado.BeginTran();

                        // 执行U8db的SQL语句
                        foreach (string sql in list)
                        {
                            U8db.Ado.ExecuteCommand(sql);
                        }

                        // 执行db的SQL语句
                        foreach (string sql in _list)
                        {
                            db.Ado.ExecuteCommand(sql);
                        }

                        // 提交两个事务
                        U8db.Ado.CommitTran();
                        db.Ado.CommitTran();

                        rs = true;
                    }
                    catch (Exception)
                    {
                        // 发生异常时回滚两个事务
                        U8db.Ado.RollbackTran();
                        db.Ado.RollbackTran();
                        rs = false;
                    }

                    if (rs)
                    {
                        return new ApiResult(1, "产品入库成功");
                    }
                    else
                    {
                        return new ApiResult(-1, "产成品入库单接口异常错误");
                    }
                }
                else
                {
                    return new ApiResult(-1, "请传入有效的json!");
                }
            }
            catch (Exception ex)
            {
                return new ApiResult(-1, "产成品入库接口异常:" + ex.Message);
            }
        }


        /// <summary>
        /// 获取单据号
        /// </summary>
        /// <returns></returns>
        public string Rd10code()
        {

            #region 获取单号
            string cSeed = DateTime.Now.ToString("yyyyMMdd").Substring(2, 6);
            string cCode = "CPRK" + cSeed + "0001";
            string iscsql = @"select RIGHT('0000' +cast((cNumber+1) as varchar),4) as cNumber From VoucherHistory  with (ROWLOCK)  Where  CardNumber='0411' and cContent='日期' and cContentRule = '日' and cSeed='" + cSeed + "'";
            DataSet iscds = U8db.Ado.GetDataSetAll(iscsql);
            if (iscds != null && iscds.Tables.Count > 0 && iscds.Tables[0].Rows.Count > 0)
            {
                //修改流水号
                U8db.Ado.ExecuteCommand(@"update VoucherHistory set cNumber=cNumber+1 Where  CardNumber='0411' and cContent='日期'  and cContentRule = '日' and cSeed='" + cSeed + "'");

                cCode = "CPRK" + cSeed + iscds.Tables[0].Rows[0]["cNumber"].ToString();

            }
            else
            {
                string sqlin = @"insert into VoucherHistory(CardNumber,cContent,ccontentrule,cSeed,cNumber,bEmpty) values('0411','日期','日','" + cSeed + "','1', '0')";//修改单号
                U8db.Ado.ExecuteCommand(sqlin);
            }
            #endregion

            return cCode;
        }
    }
}
