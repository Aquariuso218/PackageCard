using EF_HSZY_HDtoU8WebApiProject.Common;
using EF_HSZY_HDtoU8WebApiProject.Models.Rdrecord32;
using EF_HSZY_HDtoU8WebApiProject.Models.Token;
using LeaRun.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Utilities.Base.Data;

namespace EF_HSZY_HDtoU8WebApiProject.Methods
{
    public class Rdrecord32Methods
    {
        /// <summary>
        /// 销售出库单新增
        /// </summary>
        /// <param name="rdrecord32"></param>
        /// <returns></returns>
        public static ApiMessage setRdrecord32(Base_Rdrecord32 rdrecord32)
        {
            try
            {
                //账套数据库
                string cDataBase = ConfigHelper.AppSettings("cDataBase");

                Bdbase.WriteLogSql("海典销售json：" + JsonConvert.SerializeObject(rdrecord32));

                List<string> list = new List<string>();
                if (rdrecord32 != null)
                {
                    //判断子表数据是否传入
                    if (rdrecord32.subData.Count > 0 && rdrecord32.subData != null)
                    {
                        //收发类别编码
                        string cRdCode = "'204'";
                        //采购类型
                        string cPTCode = "Null";
                        //销售类型
                        string cSTCode = "'01'";

                        //账套号
                        string ztCode = rdrecord32.cZtCode;

                        //数据库账套
                        string DataBase = "";
                        if (ztCode == "615" || ztCode == "616")
                        {
                            //数据库账套
                            DataBase = cDataBase.Replace("@ztCode", ztCode).Replace("_2024", "_2025");
                        }
                        else
                        {
                            //数据库账套
                            DataBase = cDataBase.Replace("@ztCode", ztCode);
                        }

                        if (string.IsNullOrWhiteSpace(ztCode))
                        {
                            return new ApiMessage { mescode = 0, message = "账套号不允许为空!" };
                        }

                        if (string.IsNullOrWhiteSpace(rdrecord32.cCode))
                        {
                            return new ApiMessage { mescode = 0, message = "单据号不允许为空!" };
                        }

                        if (string.IsNullOrWhiteSpace(rdrecord32.dDate))
                        {
                            return new ApiMessage { mescode = 0, message = "单据日期不允许为空!" };
                        }
                        else
                        {
                            if (!DataHelper.IsDate(rdrecord32.dDate))
                            {
                                return new ApiMessage { mescode = 0, message = "单据日期请录入正确的日期格式!" };
                            }
                        }
                        if (rdrecord32.bredvouch != 0 && rdrecord32.bredvouch != 1)
                        {
                            return new ApiMessage { mescode = 0, message = "发货退货标识错误,只能为(0-发货，1-退货)!" };
                        }

                        string bReturnFlag = rdrecord32.bredvouch.ToString() == "1" ? "1" : "0";

                        //单据模板
                        string iVTid = "71";
                        if (bReturnFlag == "1")
                        {
                            iVTid = "75";
                        }

                        #region 验证数据有效性


                        string cCusCode = "";
                        string cCusName = "";
                        //客户编码
                        if (string.IsNullOrWhiteSpace(rdrecord32.cCusCode))
                        {
                            return new ApiMessage { mescode = 0, message = "客户编码不允许为空!" };
                        }
                        else
                        {
                            string sql = "select cCusCode,cCusName from " + DataBase + "..Customer (nolock) where cCusCode='" + rdrecord32.cCusCode + "' or cCusDefine10='" + rdrecord32.cCusCode + "'";
                            DataTable dt = Bdbase.Query(sql).Tables[0];
                            if (DataHelper.IsExistRows(dt))
                            {
                                return new ApiMessage { mescode = 0, message = "客户[" + rdrecord32.cCusCode + "]在U8系统中不存在!" };
                            }
                            else
                            {
                                cCusCode = dt.Rows[0]["cCusCode"].ToString();
                                cCusName = dt.Rows[0]["cCusName"].ToString();
                            }
                        }



                        //部门
                        string cDepCode = "NULL";
                        if (!string.IsNullOrWhiteSpace(rdrecord32.cDepCode))
                        {
                            string sql = "select cDepCode from " + DataBase + "..Department (nolock) where cDepCode='" + rdrecord32.cDepCode + "' and bDepEnd='1'";
                            DataTable dt = Bdbase.Query(sql).Tables[0];
                            if (DataHelper.IsExistRows(dt))
                            {
                                return new ApiMessage { mescode = 0, message = "部门编码[" + rdrecord32.cDepCode + "]在U8系统中不存在或者非末级!" };
                            }
                            cDepCode = "'" + rdrecord32.cDepCode + "'";
                        }
                        else
                        {
                            return new ApiMessage { mescode = 0, message = "部门编码不允许为空!" };
                        }

                        //业务员
                        string cPersonCode = "NULL";
                        if (!string.IsNullOrWhiteSpace(rdrecord32.cPersonCode))
                        {
                            string sql = "select cPsn_Num from " + DataBase + "..hr_hi_person (nolock) where cPsn_Num='" + rdrecord32.cPersonCode + "' and bPsnPerson='1'";
                            DataTable dt = Bdbase.Query(sql).Tables[0];
                            if (DataHelper.IsExistRows(dt))
                            {
                                return new ApiMessage { mescode = 0, message = "业务员编码[" + rdrecord32.cPersonCode + "]在U8系统中不存在!" };
                            }
                            cPersonCode = "'" + rdrecord32.cPersonCode + "'";
                        }


                        #endregion

                        Dictionary<string, decimal> dicCuStock = new Dictionary<string, decimal>();

                        foreach (Base_Rdrecords32 item in rdrecord32.subData)
                        {
                            #region 校验明细信息
                            if (string.IsNullOrWhiteSpace(item.irowno.ToString()))
                            {
                                return new ApiMessage { mescode = 0, message = "行号不允许为空!" };
                            }


                            //if (ztCode == "613")
                            //{
                            //    //仓库
                            //    string sql = "select top 1 cWhCode,cWhName,bProxyWh,bWhPos,(case when bInCost=1 then 1 else 0 end)bInCost from " + DataBase + "..Warehouse (nolock) where cDepCode='" + rdrecord32.cDepCode + "' order by cWhCode asc";
                            //    DataSet ds = Bdbase.Query(sql);
                            //    if (DataHelper.IsExistRows(ds.Tables[0]))
                            //    {
                            //        return new ApiMessage { mescode = 0, message = "部门[" + rdrecord32.cDepCode + "]在U8系统仓库档案中未设置对应的仓库!" };
                            //    }
                            //    else
                            //    {
                            //        item.cWhCode = ds.Tables[0].Rows[0]["cWhCode"].ToString();
                            //    }
                            //}

                            string cWhCode = "";
                            string bCosting = "0";
                            //仓库
                            if (string.IsNullOrWhiteSpace(item.cWhCode))
                            {
                                return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 仓库不允许为空!" };
                            }
                            else
                            {
                                string sql = "select * from " + DataBase + "..WareHouse (nolock) where cWhCode='" + item.cWhCode + "'";
                                DataTable dt = Bdbase.Query(sql).Tables[0];
                                if (DataHelper.IsExistRows(dt))
                                {
                                    return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 仓库[" + item.cWhCode + "]在U8系统中不存在!" };
                                }
                                else
                                {
                                    //是否启用代管
                                    if (dt.Rows[0]["bProxyWh"].ToString() == "True")
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 仓库[" + item.cWhCode + "]在U8系统中启用了代管仓,请检查!" };
                                    }
                                    if (dt.Rows[0]["bWhPos"].ToString() == "True")
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 仓库[" + item.cWhCode + "]在U8系统中启用了货位管理,请检查!" };
                                    }
                                    if (dt.Rows[0]["bInCost"].ToString() == "True")
                                    {
                                        bCosting = "1";
                                    }
                                    cWhCode = dt.Rows[0]["cWhCode"].ToString();
                                }
                            }

                            //存货编码
                            //是否启用保质期管理
                            bool bInvQuality = false;
                            //是否启用批次
                            string iMassDate = "NULL";
                            string cMassUnit = "NULL";
                            string iChangRate = "";
                            string cSTComUnitCode = "";
                            string iExpiratDateCalcu = "0";
                            //是否建立批次档案
                            string bBatchCreate = "0";
                            string cInvCode = "";
                            string cInvName = "";
                            if (string.IsNullOrWhiteSpace(item.cInvCode))
                            {
                                return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 存货编码不允许为空!" };
                            }
                            else
                            {
                                string sqlcinv = "select isnull(a.fInExcess,0)fInExcess,isnull(a.cSTComUnitCode,'')cSTComUnitCode,isnull(b.iChangRate,1)iChangRate ,a.bInvBatch,a.bInvQuality,isnull(iMassDate,0)iMassDate,a.cMassUnit,a.cInvCode,a.cInvName,isnull(s.iExpiratDateCalcu,0)iExpiratDateCalcu,(case when s.bBatchCreate=1 then 1 else 0 end)bBatchCreate  from " + DataBase + "..Inventory a (nolock) left join " + DataBase + "..ComputationUnit b (nolock) on a.cSTComUnitCode=b.cComunitCode left join " + DataBase + "..Inventory_sub s (nolock) on s.cInvSubCode=a.cInvCode left join " + DataBase + "..Inventory_extradefine c (nolock) on a.cInvCode=c.cInvCode where (a.cInvCode='" + item.cInvCode + "' or c.cidefine1='" + item.cInvCode + "')";
                                DataTable cinvds = Bdbase.Query(sqlcinv).Tables[0];
                                if (DataHelper.IsExistRows(cinvds))
                                {
                                    return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 存货编码[" + item.cInvCode + "]在U8系统中不存在!" };
                                }
                                else
                                {
                                    cInvCode = cinvds.Rows[0]["cInvCode"].ToString();
                                    cSTComUnitCode = cinvds.Rows[0]["cSTComUnitCode"].ToString();
                                    iChangRate = cinvds.Rows[0]["iChangRate"].ToString();
                                    bBatchCreate = cinvds.Rows[0]["bBatchCreate"].ToString();
                                    cInvName = cinvds.Rows[0]["cInvName"].ToString();
                                    if (cinvds.Rows[0]["bInvQuality"].ToString() == "True")
                                    {
                                        bInvQuality = true;
                                        iMassDate = "'" + cinvds.Rows[0]["iMassDate"].ToString() + "'";
                                        cMassUnit = "'" + cinvds.Rows[0]["cMassUnit"].ToString() + "'";
                                        iExpiratDateCalcu = cinvds.Rows[0]["iExpiratDateCalcu"].ToString();
                                    }
                                    if (cinvds.Rows[0]["bInvBatch"].ToString() == "True")
                                    {
                                        if (string.IsNullOrWhiteSpace(item.cBatch.Trim()))
                                        {
                                            return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 存货编码[" + item.cInvCode + "]已启用批次管理，批号不允许为空!" };
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(item.cBatch.Trim()))
                                        {
                                            return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 存货编码[" + item.cInvCode + "]未启用批次管理，批号不允许录入!" };
                                        }
                                    }
                                }
                            }

                            //计算保质期
                            string dMadeDate = "Null";
                            string dVDate = "Null";
                            string cExpirationdate = "Null";
                            string dExpirationdate = "Null";
                            if (bInvQuality)
                            {

                                if (rdrecord32.bredvouch == 0)
                                {
                                    #region 发货获取生产日期
                                    DataSet dsstock = Bdbase.Query("select dVDate,dMdate,AutoID,iQuantity,dExpirationdate,iMassDate,cMassUnit,isnull(iExpiratDateCalcu,0)iExpiratDateCalcu,cExpirationdate from " + DataBase + "..CurrentStock (nolock) where cWhCode='" + cWhCode + "' and cInvCode='" + cInvCode + "' and isnull(cBatch,'')='" + item.cBatch.Trim() + "' and isnull(cVMIVenCode,'')=''");
                                    if (DataHelper.IsExistRows(dsstock.Tables[0]))
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 仓库[" + item.cWhCode + "] 存货编码[" + cInvCode + "] 批次[" + item.cBatch.Trim() + "] 现存量不足!" };
                                    }
                                    else
                                    {
                                        //失效日期
                                        if (!string.IsNullOrWhiteSpace(dsstock.Tables[0].Rows[0]["dVDate"].ToString()))
                                        {
                                            dVDate = "'" + dsstock.Tables[0].Rows[0]["dVDate"].ToString() + "'";

                                        }
                                        //生产日期
                                        if (!string.IsNullOrWhiteSpace(dsstock.Tables[0].Rows[0]["dMdate"].ToString()))
                                        {
                                            dMadeDate = "'" + dsstock.Tables[0].Rows[0]["dMdate"].ToString() + "'";
                                        }
                                        //有效期计算项 
                                        if (!string.IsNullOrWhiteSpace(dsstock.Tables[0].Rows[0]["dExpirationdate"].ToString()))
                                        {
                                            dExpirationdate = "'" + dsstock.Tables[0].Rows[0]["dExpirationdate"].ToString() + "'";
                                        }
                                        //有效期至
                                        if (!string.IsNullOrWhiteSpace(dsstock.Tables[0].Rows[0]["cExpirationdate"].ToString()))
                                        {
                                            cExpirationdate = "'" + dsstock.Tables[0].Rows[0]["cExpirationdate"].ToString() + "'";
                                        }
                                        if (!string.IsNullOrWhiteSpace(dsstock.Tables[0].Rows[0]["cMassUnit"].ToString()))
                                        {
                                            //保质期单位
                                            cMassUnit = "'" + dsstock.Tables[0].Rows[0]["cMassUnit"].ToString() + "'";
                                        }
                                        if (!string.IsNullOrWhiteSpace(dsstock.Tables[0].Rows[0]["iMassDate"].ToString()))
                                        {
                                            //保质期天数
                                            iMassDate = "'" + dsstock.Tables[0].Rows[0]["iMassDate"].ToString() + "'";
                                        }
                                        //有效期推算方式
                                        if (!string.IsNullOrWhiteSpace(dsstock.Tables[0].Rows[0]["iExpiratDateCalcu"].ToString()))
                                        {
                                            iExpiratDateCalcu = "'" + dsstock.Tables[0].Rows[0]["iExpiratDateCalcu"].ToString() + "'";
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region 退货计算生产日期
                                    if (string.IsNullOrWhiteSpace(item.dMadeDate))
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 存货编码[" + item.cInvCode + "]启用了保质期管理,生产日期不允许为空!" };
                                    }
                                    else
                                    {
                                        //根据生产日期和保质期，计算失效日期
                                        DateTime made;
                                        if (!DateTime.TryParse(item.dMadeDate, out made))
                                        {
                                            return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 生产日期格式不正确!" };
                                        }
                                        dMadeDate = "'" + made.ToString("yyyy-MM-dd") + "'";
                                        if (cMassUnit.Replace("'", "") == "1")
                                        {
                                            dVDate = "'" + made.AddYears(int.Parse(iMassDate.Replace("'", "").ToString())).ToString("yyyy-MM-dd") + "'";
                                        }
                                        else if (cMassUnit.Replace("'", "") == "2")
                                        {
                                            dVDate = "'" + made.AddMonths(int.Parse(iMassDate.Replace("'", "").ToString())).ToString("yyyy-MM-dd") + "'";
                                        }
                                        else if (cMassUnit.Replace("'", "") == "3")
                                        {
                                            dVDate = "'" + made.AddDays(double.Parse(iMassDate.Replace("'", "").ToString())).ToString("yyyy-MM-dd") + "'";
                                        }
                                    }

                                    //按月计算有效期
                                    if (iExpiratDateCalcu == "1")
                                    {
                                        cExpirationdate = "'" + DateTime.Parse(dVDate.Replace("'", "")).AddMonths(-1).ToString("yyyy-MM") + "'";
                                        dExpirationdate = "'" + DateTime.Parse(dVDate.Replace("'", "")).AddMonths(-1).ToString("yyyy-MM-dd") + "'";
                                    }
                                    //按日计算有效期
                                    else if (iExpiratDateCalcu == "2")
                                    {
                                        cExpirationdate = "'" + DateTime.Parse(dVDate.Replace("'", "")).AddDays(-1).ToString("yyyy-MM-dd") + "'";
                                        dExpirationdate = "'" + DateTime.Parse(dVDate.Replace("'", "")).AddDays(-1).ToString("yyyy-MM-dd") + "'";

                                    }
                                    #endregion
                                }
                            }

                            decimal iQuantity = 0;
                            if (item.iQuantity.ToString() == "0" || string.IsNullOrWhiteSpace(item.iQuantity.ToString()))
                            {
                                return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行数量不允许为零或者为空!" };
                            }
                            else
                            {
                                if (!decimal.TryParse(item.iQuantity.ToString(), out iQuantity))
                                {
                                    return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 数量格式不正确!" };
                                }

                                //发货验证
                                if (rdrecord32.bredvouch == 0)
                                {
                                    if (iQuantity < 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售出库数量必须大于0!" };
                                    }

                                    if (item.iOriCost < 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售出库无税单价必须大于或等于0!" };
                                    }

                                    if (item.iOriTaxCost < 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售出库含税单价必须大于或等于0!" };
                                    }

                                    if (item.iOriMoney < 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售出库无税金额必须大于或等于0!" };
                                    }

                                    if (item.ioriSum < 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售出库含税金额必须大于或等于0!" };
                                    }

                                    if (item.iTaxRate < 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售出库税率必须大于或等于0!" };
                                    }
                                }
                                else
                                {
                                    if (iQuantity > 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售退货数量必须小于0!" };
                                    }

                                    if (item.iOriCost < 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售退货无税单价必须大于或等于0!" };
                                    }

                                    if (item.iOriTaxCost < 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售退货含税单价必须大于或等于0!" };
                                    }

                                    if (item.iOriMoney > 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售退货无税金额必须大于或等于0!" };
                                    }

                                    if (item.ioriSum > 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售退货含税金额必须大于或等于0!" };
                                    }

                                    if (item.iTaxRate < 0)
                                    {
                                        return new ApiMessage { mescode = 0, message = "第[" + item.irowno + "]行 销售退货税率必须大于或等于0!" };
                                    }
                                }
                            }


                            //计算件数
                            decimal iNum = 0;
                            string vouchiNum = "Null";

                            if (!string.IsNullOrWhiteSpace(cSTComUnitCode))
                            {
                                //Bdbase.WriteLogSql("计量单位：" + iChangRate.ToString() + "44444["+cInvCode+"]");

                                iNum = Math.Round(iQuantity / decimal.Parse(iChangRate.ToString()), 6, MidpointRounding.AwayFromZero);

                                cSTComUnitCode = "'" + cSTComUnitCode + "'";

                                vouchiNum = iNum.ToString();
                            }
                            else
                            {
                                cSTComUnitCode = "Null";
                                vouchiNum = "Null";
                                iChangRate = "Null";
                            }

                            //处理批次
                            string cVouchcBatch = "Null";
                            if (item.cBatch.Trim() != "")
                            {
                                cVouchcBatch = "'" + item.cBatch.Trim() + "'";
                                //启用批次档案
                                if (bBatchCreate == "1")
                                {
                                    //写入物料批次档案表
                                    U8CommonHelper.SetAA_BatchProperty(DataBase, cInvCode, item.cBatch.Trim());
                                }
                            }

                            //销售发货单号
                            string cDLcCode = U8CommonHelper.GetcDLcCode(DataBase, rdrecord32.dDate);
                            if (cDLcCode == "")
                            {
                                return new ApiMessage { mescode = 0, message = "销售出库单接口失败：销售发货单号获取错误" };
                            }

                            //销售出库单号
                            string cCode = U8CommonHelper.GetRd32cCode(DataBase, rdrecord32.dDate);
                            if (cCode == "")
                            {
                                return new ApiMessage { mescode = 0, message = "销售出库单接口失败：销售出库单号获取错误" };
                            }

                            #region  获取销售发货单ID
                            //获取销售发货单ID
                            string sqlID = @" declare @p5 int
                                set @p5=1000000010
                                declare @p6 int
                                set @p6=1000000011
                                exec sp_getID '','" + ztCode + "','DISPATCH',1,@p5 output,@p6 output select @p5, @p6";

                            DataSet dsid = Bdbase.Query(sqlID);

                            //主表ID
                            string DLID = dsid.Tables[0].Rows[0][0].ToString();
                            //子表ID
                            string iDLsID = dsid.Tables[0].Rows[0][1].ToString();

                            #endregion

                     
                            //发货单表头
                            list.Add("insert into " + DataBase + "..dispatchlist ([DLID],[cDLCode],[cVouchType],[cSTCode],[dDate],[cRdCode],[cDepCode],[cPersonCode],[SBVID],[cSBVCode],[cSOCode],[cCusCode],[cPayCode],[cSCCode],[cShipAddress],[cexch_name],[iExchRate],[iTaxRate],[bFirst],[bReturnFlag],[bSettleAll],[cMemo],[cSaleOut],[cDefine1],[cDefine2],[cDefine3],[cDefine4],[cDefine5],[cDefine6],[cDefine7],[cDefine8],[cDefine9],[cDefine10],[cVerifier],[cMaker],[iNetLock],[iSale],[cCusName],[iVTid],[cBusType],[cCloser],[cAccounter],[cCreChpName],[cDefine11],[cDefine12],[cDefine13],[cDefine14],[cDefine15],[cDefine16],[bIAFirst],[ioutgolden],[cgatheringplan],[dCreditStart],[dGatheringDate],[icreditdays],[bCredit],[caddcode],[iverifystate],[ireturncount],[iswfcontrolled],[icreditstate],[bARFirst],[cmodifier],[dmoddate],[dverifydate],[ccusperson],[dcreatesystime],[dverifysystime],[dmodifysystime],[csvouchtype],[iflowid],[bsigncreate],[bcashsale],[cgathingcode],[cChanger],[cChangeMemo],[outid],[bmustbook],[cBookDepcode],[cBookType],[bSaUsed],[bneedbill],[baccswitchflag],[iPrintCount],[ccuspersoncode],[cSourceCode],[bsaleoutcreatebill],[cSysBarCode],[cCurrentAuditor],[csscode],[cinvoicecompany],[fEBweight],[cEBweightUnit],[cEBExpressCode],[iEBExpressCoID],[SeparateID],[bNotToGoldTax],[cEBTrnumber],[cEBBuyer],[cEBBuyerNote],[ccontactname],[cEBprovince],[cEBcity],[cEBdistrict],[cmobilephone],[cInvoiceCusName],[cweighter],[dweighttime],[cPickVouchCode]) select top 1 '" + DLID + "', '" + cDLcCode + "', '05', " + cSTCode + ", '" + DateTime.Parse(rdrecord32.dDate).ToString("yyyy-MM-dd") + "', Null, " + cDepCode + ", " + cPersonCode + ", 0, Null, Null, '" + cCusCode + "', Null, Null,a.cCusOAddress, '人民币', 1," + item.iTaxRate + ", '0', '" + bReturnFlag + "', '0', '" + rdrecord32.cMemo + "', '" + cCode + "', '" + rdrecord32.cCode + "', Null, Null, Null, Null, Null, 0, Null, Null, Null, '" + rdrecord32.cMaker + "', '" + rdrecord32.cMaker + "', '0', 0, '" + cCusName + "'," + iVTid + ", '普通销售', Null, Null, Null, Null, Null, Null, Null, Null, Null, '0', Null,NULL, Null, Null, Null, '0',b.cAddCode, 0, 0, 0, Null, '0', Null, Null, '" + DateTime.Parse(rdrecord32.dDate).ToString("yyyy-MM-dd") + "', Null, GETDATE(), GETDATE(), Null, Null, Null, '0', '0', Null, Null, Null, Null, '0', Null, Null, '0', '1', '0', Null, Null, Null, '0', '||SA01|" + cDLcCode + "', Null, Null,a.cInvoiceCompany, Null, Null, Null, Null, Null, 0, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null from " + DataBase + "..Customer a (nolock) left join " + DataBase + "..CusDeliverAdd b (nolock) on a.cCusCode=b.cCusCode and b.bDefault=1 where a.cCusCode='" + cCusCode + "'");

                            //是否存在扩展自定义项表
                            string sqlext = "IF EXISTS(SELECT name FROM " + DataBase + "..[sysobjects] WHERE name = 'dispatchlist_extradefine') select '1' ELSE select '0'";
                            string isext = Bdbase.GetValue(sqlext);

                            if (isext == "1")
                            {
                                list.Add("insert into " + DataBase + "..dispatchlist_extradefine(DLID) values('" + DLID + "')");
                            }


                            //发货单表体
                            list.Add("insert into " + DataBase + "..dispatchlists ([DLID],[iCorID],[cWhCode],[cInvCode],[iQuantity],[iNum],[iQuotedPrice],[iUnitPrice],[iTaxUnitPrice],[iMoney],[iTax],[iSum],[iDisCount],[iNatUnitPrice],[iNatMoney],[iNatTax],[iNatSum],[iNatDisCount],[iSettleNum],[iSettleQuantity],[iBatch],[cBatch],[bSettleAll],[cMemo],[cFree1],[cFree2],[iTB],[dvDate],[TBQuantity],[TBNum],[iSOsID],[iDLsID],[KL],[KL2],[cInvName],[iTaxRate],[cDefine22],[cDefine23],[cDefine24],[cDefine25],[cDefine26],[cDefine27],[fOutQuantity],[fOutNum],[cItemCode],[cItem_class],[fSaleCost],[fSalePrice],[cVenAbbName],[cItemName],[cItem_CName],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[bIsSTQc],[iInvExchRate],[cUnitID],[cCode],[iRetQuantity],[fEnSettleQuan],[fEnSettleSum],[iSettlePrice],[cDefine28],[cDefine29],[cDefine30],[cDefine31],[cDefine32],[cDefine33],[cDefine34],[cDefine35],[cDefine36],[cDefine37],[dMDate],[bGsp],[cGspState],[cSoCode],[cCorCode],[iPPartSeqID],[iPPartID],[iPPartQty],[cContractID],[cContractTagCode],[cContractRowGuid],[iMassDate],[cMassUnit],[bQANeedCheck],[bQAUrgency],[bQAChecking],[bQAChecked],[iQAQuantity],[iQANum],[cCusInvCode],[cCusInvName],[fsumsignquantity],[fsumsignnum],[cbaccounter],[bcosting],[cordercode],[iorderrowno],[fcusminprice],[icostquantity],[icostsum],[ispecialtype],[cvmivencode],[iexchsum],[imoneysum],[irowno],[frettbquantity],[fretsum],[iExpiratDateCalcu],[dExpirationdate],[cExpirationdate],[cbatchproperty1],[cbatchproperty2],[cbatchproperty3],[cbatchproperty4],[cbatchproperty5],[cbatchproperty6],[cbatchproperty7],[cbatchproperty8],[cbatchproperty9],[cbatchproperty10],[dblPreExchMomey],[dblPreMomey],[idemandtype],[cdemandcode],[cdemandmemo],[cdemandid],[idemandseq],[cvencode],[cReasonCode],[cInvSN],[iInvSNCount],[bneedsign],[bsignover],[bneedloss],[flossrate],[frlossqty],[fulossqty],[isettletype],[crelacuscode],[cLossMaker],[dLossDate],[dLossTime],[icoridlsid],[fretoutqty],[body_outid],[fVeriBillQty],[fVeriBillSum],[fVeriRetQty],[fVeriRetSum],[fLastSettleQty],[fLastSettleSum],[cBookWhcode],[cInVouchType],[cPosition],[fretqtywkp],[fretqtyykp],[frettbqtyykp],[fretsumykp],[dkeepdate],[cSCloser],[isaleoutid],[bsaleprice],[bgift],[bmpforderclosed],[cbSysBarCode],[fxjquantity],[fxjnum],[bIAcreatebill],[cParentCode],[cChildCode],[fchildqty],[fchildrate],[iCalcType],[fappretwkpqty],[fappretwkpsum],[fappretykpqty],[fappretykpsum],[fappretwkptbqty],[fappretykptbqty],[irtnappid],[crtnappcode],[fretailrealamount],[fretailsettleamount]) values('" + DLID + "', Null, '" + cWhCode + "', '" + cInvCode + "', '" + iQuantity + "'," + vouchiNum + ", '" + item.iOriTaxCost + "', '" + item.iOriCost + "', '" + item.iOriTaxCost + "'," + item.iOriMoney + ", " + item.iOriTaxPrice + ", " + item.ioriSum + ", 0, '" + item.iOriCost + "', " + item.iOriMoney + ", " + item.iOriTaxPrice + ", " + item.ioriSum + ", 0, '0', '0', Null, " + cVouchcBatch + ", '0', Null, Null, Null, 0," + dVDate + ", '0', Null, Null, " + iDLsID + ", '100', '100', '" + cInvName + "', '" + item.iTaxRate + "', Null, Null, Null, Null, Null, Null, '" + iQuantity + "', " + iNum + ", Null, Null, '" + item.iOriTaxCost + "', '" + item.ioriSum + "', Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, '0'," + iChangRate + "," + cSTComUnitCode + ", Null, '0', '0', 0, '0', Null, Null, Null, Null, Null, Null, Null, Null, Null, Null," + dMadeDate + ", '0', Null, Null, Null, Null, Null, Null, Null, Null, Null," + iMassDate + ", " + cMassUnit + ", '0', '0', '0', '0', '0', '0', Null, Null, Null, Null, Null, '" + bCosting + "', Null, Null, '0', Null, Null, Null, Null, 0, 0, '" + item.irowno + "', '0', '0', " + iExpiratDateCalcu + ", " + dExpirationdate + "," + cExpirationdate + ", Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, '0', '0', Null, Null, Null, Null, Null, Null, Null, Null, Null, '0', '0', '0', Null, '0', '0', Null, Null, Null, Null, Null, Null, '0', Null, '0', 0, '0', 0, Null, Null, Null, Null, Null, '0', '0', '0', '0', Null, Null, Null, '1', '0', '0', '||SA01|" + cDLcCode + "|" + item.irowno + "', Null, Null, '0', Null, Null, Null, Null, Null, '0', '0', '0', '0', '0', '0', Null, Null, '0', '0')");


                            //是否存在扩展自定义项表
                            sqlext = "IF EXISTS(SELECT name FROM " + DataBase + "..[sysobjects] WHERE name = 'dispatchlists_extradefine') select '1' ELSE select '0'";
                            isext = Bdbase.GetValue(sqlext);

                            if (isext == "1")
                            {
                                list.Add("insert into " + DataBase + "..dispatchlists_extradefine(iDLsID) values('" + iDLsID + "')");
                            }


                            //获取销售出库单ID
                            DataSet dsrdID = U8CommonHelper.GetRdID(ztCode, 1);
                            string Id = dsrdID.Tables[0].Rows[0][0].ToString();
                            string zbid = dsrdID.Tables[0].Rows[0][1].ToString();

                            //销售出库单主表
                            list.Add("insert into " + DataBase + "..rdrecord32 ([ID],[bRdFlag],[cVouchType],[cBusType],[cSource],[cBusCode],[cWhCode],[dDate],[cCode],[cRdCode],[cDepCode],[cPersonCode],[cPTCode],[cSTCode],[cCusCode],[cVenCode],[cOrderCode],[cARVCode],[cBillCode],[cDLCode],[cProBatch],[cHandler],[cMemo],[bTransFlag],[cAccounter],[cMaker],[cDefine1],[cDefine2],[cDefine3],[cDefine4],[cDefine5],[cDefine6],[cDefine7],[cDefine8],[cDefine9],[cDefine10],[dKeepDate],[dVeriDate],[bpufirst],[biafirst],[iMQuantity],[dARVDate],[cChkCode],[dChkDate],[cChkPerson],[VT_ID],[bIsSTQc],[cDefine11],[cDefine12],[cDefine13],[cDefine14],[cDefine15],[cDefine16],[gspcheck],[isalebillid],[iExchRate],[cExch_Name],[cShipAddress],[caddcode],[bOMFirst],[bFromPreYear],[bIsLsQuery],[bIsComplement],[iDiscountTaxType],[ireturncount],[iverifystate],[iswfcontrolled],[cModifyPerson],[dModifyDate],[dnmaketime],[dnmodifytime],[dnverifytime],[bredvouch],[iFlowId],[cPZID],[cSourceLs],[cSourceCodeLs],[iPrintCount],[csysbarcode],[cCurrentAuditor],[cinvoicecompany],[fEBweight],[cEBweightUnit],[cEBExpressCode],[bScanExpress],[cinspector],[dinspecttime],[cweighter],[dweighttime],[bUpLoaded],[outid],[isMDdispatch]) select top 1 '" + Id + "', 0, '32', '普通销售', '发货单', '" + cDLcCode + "', '" + cWhCode + "', '" + DateTime.Parse(rdrecord32.dDate).ToString("yyyy-MM-dd") + "', '" + cCode + "'," + cRdCode + "," + cDepCode + "," + cPersonCode + ", " + cPTCode + ", " + cSTCode + ", '" + cCusCode + "', Null, Null, Null, Null, '" + DLID + "',Null, '" + rdrecord32.cMaker + "', '" + rdrecord32.cMemo + "', '0', Null, '" + rdrecord32.cMaker + "', '" + rdrecord32.cCode + "', Null, Null, Null, Null, Null, 0, Null, Null, Null, Null, '" + DateTime.Parse(rdrecord32.dDate).ToString("yyyy-MM-dd") + "', '0', '0', Null, Null, Null, Null, Null, 87, '0', Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, a.cCusOAddress, b.cAddCode, '0', '0', '0', 0, 0, 0, 0, 0, '', Null, GETDATE(), Null, GETDATE(), 0, Null, Null, Null, Null, 0, '||st32|" + cCode + "', Null, a.cInvoiceCompany, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null from " + DataBase + "..Customer a (nolock) left join " + DataBase + "..CusDeliverAdd b (nolock) on a.cCusCode=b.cCusCode and b.bDefault=1 where a.cCusCode='" + cCusCode + "'");


                            //是否存在扩展自定义项表
                            sqlext = "IF EXISTS(SELECT name FROM " + DataBase + "..[sysobjects] WHERE name = 'rdrecord32_extradefine') select '1' ELSE select '0'";
                            isext = Bdbase.GetValue(sqlext);

                            if (isext == "1")
                            {
                                list.Add("insert into " + DataBase + "..rdrecord32_extradefine(ID) values('" + Id + "')");
                            }


                            //销售出库单表体
                            list.Add("insert into " + DataBase + "..rdrecords32 ([AutoID],[ID],[cInvCode],[iNum],[iQuantity],[iUnitCost],[iPrice],[iAPrice],[iPUnitCost],[iPPrice],[cBatch],[cVouchCode],[cInVouchCode],[cinvouchtype],[iSOutQuantity],[iSOutNum],[coutvouchid],[coutvouchtype],[iSRedOutQuantity],[iSRedOutNum],[cFree1],[cFree2],[iFlag],[iFNum],[iFQuantity],[dVDate],[cPosition],[cDefine22],[cDefine23],[cDefine24],[cDefine25],[cDefine26],[cDefine27],[cItem_class],[cItemCode],[iDLsID],[iSBsID],[iSendQuantity],[iSendNum],[iEnsID],[cName],[cItemCName],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[cBarCode],[iNQuantity],[iNNum],[cAssUnit],[dMadeDate],[iMassDate],[cDefine28],[cDefine29],[cDefine30],[cDefine31],[cDefine32],[cDefine33],[cDefine34],[cDefine35],[cDefine36],[cDefine37],[iCheckIds],[cBVencode],[bGsp],[cGspState],[cCheckCode],[iCheckIdBaks],[cRejectCode],[iRejectIds],[cCheckPersonCode],[dCheckDate],[cMassUnit],[iRefundInspectFlag],[strContractId],[strCode],[bChecked],[iEqDID],[bLPUseFree],[iRSRowNO],[iOriTrackID],[coritracktype],[cbaccounter],[dbKeepDate],[bCosting],[bVMIUsed],[iVMISettleQuantity],[iVMISettleNum],[cvmivencode],[iInvSNCount],[cwhpersoncode],[cwhpersonname],[cserviceoid],[cbserviceoid],[iinvexchrate],[cbdlcode],[corufts],[strContractGUID],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cciqbookcode],[iBondedSumQty],[ccusinvcode],[ccusinvname],[iorderdid],[iordertype],[iordercode],[iorderseq],[ipesodid],[ipesotype],[cpesocode],[ipesoseq],[isodid],[isotype],[csocode],[isoseq],[cBatchProperty1],[cBatchProperty2],[cBatchProperty3],[cBatchProperty4],[cBatchProperty5],[cBatchProperty6],[cBatchProperty7],[cBatchProperty8],[cBatchProperty9],[cBatchProperty10],[cbMemo],[irowno],[strowguid],[ipreuseqty],[ipreuseinum],[iDebitIDs],[fsettleqty],[fretqtywkp],[fretqtyykp],[cbsysbarcode],[bIAcreatebill],[bsaleoutcreatebill],[isaleoutid],[bneedbill],[iposflag],[body_outid]) values('" + zbid + "', '" + Id + "', '" + cInvCode + "'," + vouchiNum + ", '" + iQuantity + "','" + item.iOriCost + "', '" + item.iOriMoney + "', Null, Null, Null, " + cVouchcBatch + ", Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, 0, Null, Null, " + dVDate + ", Null, Null, Null, Null, Null, Null, Null, Null, Null, '" + iDLsID + "', Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, '" + iQuantity + "', '" + iNum + "'," + cSTComUnitCode + ", " + dMadeDate + ", " + iMassDate + ", Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, '0', Null, Null, Null, Null, Null, Null, Null, " + cMassUnit + ", Null, Null, Null, '0', Null, '0', 0, '0', Null, Null, Null, '" + bCosting + "', '0', Null, Null, Null, Null, Null, Null, Null, Null, " + iChangRate + ", '" + cDLcCode + "',convert(nvarchar,convert(money,@@DBTS),2), Null, " + iExpiratDateCalcu + ", " + cExpirationdate + ", " + dExpirationdate + ", Null, Null, Null, Null, Null, 0, Null, Null, Null, 0, Null, Null, Null, 0, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, " + item.irowno + ", Null, Null, Null, Null, Null, Null, Null, '||st32|" + cCode + "|" + item.irowno + "', '0', '0', Null, '1', Null, Null)");

                            if (bCosting == "1")
                            {
                                // 未记账单据列表
                                list.Add(@"insert into " + DataBase + "..IA_ST_UnAccountVouch32(IDUN,IDSUN,cVouTypeUN,cBustypeUN) VALUES('" + Id + "','" + zbid + "','32','销售出库')");
                            }


                            //是否存在扩展自定义项表
                            sqlext = "IF EXISTS(SELECT name FROM " + DataBase + "..[sysobjects] WHERE name = 'rdrecords32_extradefine') select '1' ELSE select '0'";
                            isext = Bdbase.GetValue(sqlext);
                            if (isext == "1")
                            {
                                list.Add("insert into " + DataBase + "..rdrecords32_extradefine(AutoID) values('" + zbid + "')");
                            }

                            string InvStr = cWhCode + "₮" + cInvCode + "₮" + item.cBatch.Trim();
                            //记录物料领料数量
                            decimal cval = 0;
                            if (dicCuStock.TryGetValue(InvStr, out cval))
                            {

                                dicCuStock[InvStr] = dicCuStock[InvStr] + iQuantity;
                            }
                            else
                            {
                                dicCuStock.Add(InvStr, iQuantity);
                            }

                            //处理现存量
                            if (rdrecord32.bredvouch == 0)
                            {
                                //修改现存量数量
                                list.Add("update " + DataBase + "..CurrentStock set iQuantity=isnull(iQuantity,0)-'" + iQuantity + "' where cInvCode='" + cInvCode + "' and cWhCode='" + cWhCode + "' and isnull(cBatch,'')='" + item.cBatch.Trim() + "' and isnull(cvmivencode,'')=''");

                            }
                            else
                            {
                                //判断是否需新增现存量表
                                string isStock = U8CommonHelper.SetCurrentStockByInv(DataBase, cWhCode, cInvCode, item.cBatch.Trim());
                                if (isStock == "")
                                {
                                    return new ApiMessage { mescode = 0, message = "销售出库单接口失败：物料写入库存错误" };
                                }

                                //修改现存量数量和保质期
                                list.Add("update " + DataBase + "..CurrentStock set iQuantity=isnull(iQuantity,0)-'" + iQuantity + "',dVDate=" + dVDate + ",dMdate=" + dMadeDate + ",cMassUnit=" + cMassUnit + ",iMassDate=" + iMassDate + ",iExpiratDateCalcu=" + iExpiratDateCalcu + ",dExpirationdate=" + dExpirationdate + ",cExpirationdate=" + cExpirationdate + " where cInvCode='" + cInvCode + "' and cWhCode='" + cWhCode + "' and isnull(cBatch,'')='" + item.cBatch.Trim() + "' and isnull(cvmivencode,'')=''");
                            }

                            #endregion
                        }

                        string MesError = "";
                        #region 校验仓库现存量
                        foreach (KeyValuePair<string, decimal> item in dicCuStock)
                        {
                            if (item.Value > 0)
                            {
                                string[] cWStr = item.Key.Split('₮');
                                //验证现存量是否充足
                                string sqlxcl = "select iQuantity from " + DataBase + "..currentStock (nolock) where cWhCode='" + cWStr[0] + "' and cInvCode='" + cWStr[1] + "' and cBatch='" + cWStr[2] + "' and cvmivencode=''";
                                DataSet dsxcl = Bdbase.Query(sqlxcl);
                                if (!DataHelper.IsExistRows(dsxcl.Tables[0]))
                                {
                                    decimal Qty = decimal.Parse(dsxcl.Tables[0].Rows[0]["iQuantity"].ToString());
                                    if (Qty < item.Value)
                                    {
                                        MesError = "仓库[" + cWStr[0] + "] 存货编码[" + cWStr[1] + "] 批次[" + cWStr[2] + "] 现存量不足";
                                        break;
                                    }
                                }
                                else
                                {
                                    MesError = "仓库[" + cWStr[0] + "] 存货编码[" + cWStr[1] + "] 批次[" + cWStr[2] + "] 现存量不足";
                                    break;
                                }
                            }
                        }

                        #endregion

                        if (MesError == "")
                        {
                            bool rs = Bdbase.ExecuteSqlTran(list);
                            if (rs)
                            {
                                return new ApiMessage { mescode = 1, message = "成功" };
                            }
                            else
                            {
                                return new ApiMessage { mescode = 0, message = "销售出库单接口错误!" };
                            }
                        }
                        else
                        {
                            return new ApiMessage { mescode = 0, message = MesError };
                        }
                    }
                    else
                    {
                        return new ApiMessage { mescode = 0, message = "表体至少有一条数据行!" };
                    }
                }
                else
                {
                    return new ApiMessage { mescode = 0, message = "请传入有效的json!" };
                }
            }
            catch (Exception ex)
            {
                Bdbase.WriteLogSql("海典销售接口异常：" + ex.Message);
                return new ApiMessage { mescode = 0, message = "销售出库单接口异常:" + ex.Message };
            }
        }


        /// <summary>
        /// 销售出库单价格变更
        /// </summary>
        /// <returns></returns>
        public static ApiMessage setRdrecord32changePrice(List<Base_Rdrecord32Price> rd32Price)
        {
            try
            {
                //账套数据库
                string cDataBase = ConfigHelper.AppSettings("cDataBase");

                List<string> list = new List<string>();
                Dictionary<string, decimal> dicCurrent = new Dictionary<string, decimal>();
                if (rd32Price != null && rd32Price.Count > 0)
                {
                    string Msg = "";
                    foreach (Base_Rdrecord32Price item in rd32Price)
                    {
                        //账套号
                        string ztCode = item.cZtCode;
                        //数据库账套
                        string DataBase = cDataBase.Replace("@ztCode", ztCode);

                        //存货编码
                        string cInvCode = Bdbase.GetValue("select a.cInvCode from " + DataBase + "..Inventory a (nolock) left join " + DataBase + "..Inventory_extradefine c (nolock) on a.cInvCode=c.cInvCode where (a.cInvCode='" + item.cInvCode + "' or c.cidefine1='" + item.cInvCode + "')");

                        //查询该销售出库单是否存在
                        DataSet dsRd = Bdbase.Query("select ddate,iDLsID,isnull(iSettleQuantity,0)iSettleQuantity,cbaccounter from " + DataBase + "..DispatchList a (nolock) left join " + DataBase + "..DispatchLists b (nolock) on a.DLID=b.DLID where a.cDefine1='" + item.cCode + "' and b.cInvCode='" + item.cInvCode + "' and b.irowno='" + item.irowno + "'");

                        if (dsRd != null && dsRd.Tables.Count > 0 && dsRd.Tables[0].Rows.Count > 0)
                        {
                            //销售发货单子表id
                            string iDLsID = dsRd.Tables[0].Rows[0]["iDLsID"].ToString();
                            //销售发货单开票数量
                            string iSettleQuantity = dsRd.Tables[0].Rows[0]["iSettleQuantity"].ToString();
                            //销售发货单记账人
                            string cbaccounter = dsRd.Tables[0].Rows[0]["cbaccounter"].ToString();
                            //销售发货单日期
                            string dDate = dsRd.Tables[0].Rows[0]["ddate"].ToString();

                            //查询会计期间是否结账
                            string bflag_SA = Bdbase.GetValue("select (case when bflag_SA=1 then 1 else 0 end)bflag_ST from (select iYear, iid from UFSystem..UA_Period (nolock) where cAcc_Id = '" + ztCode + "' and '" + dDate + "' between dBegin and dEnd)a left join " + DataBase + "..GL_mend b (nolock) on a.iYear = b.iyear and a.iId = b.iperiod");
                            if (int.Parse(bflag_SA) == 1)
                            {
                                Msg = "销售出单号[" + item.cCode + "] 行号[" + item.irowno + "] 存货编码[" + item.cInvCode + "] 单据日期所在的会计期间在U8系统中销售已结账，不可修改价格";
                                break;
                            }

                            //查询会计期间是否结账
                            string bflag_ST = Bdbase.GetValue("select (case when bflag_ST=1 then 1 else 0 end)bflag_ST from (select iYear, iid from UFSystem..UA_Period (nolock) where cAcc_Id = '" + ztCode + "' and '" + dDate + "' between dBegin and dEnd)a left join " + DataBase + "..GL_mend b (nolock) on a.iYear = b.iyear and a.iId = b.iperiod");
                            if (int.Parse(bflag_ST) == 1)
                            {
                                Msg = "销售出单号[" + item.cCode + "] 行号[" + item.irowno + "] 存货编码[" + item.cInvCode + "] 单据日期所在的会计期间在U8系统中库存已结账，不可修改价格";
                                break;
                            }

                            if (decimal.Parse(iSettleQuantity) != 0)
                            {
                                Msg = "销售出单号[" + item.cCode + "] 行号[" + item.irowno + "] 存货编码[" + item.cInvCode + "] 在U8系统已开票，不可修改价格";
                                break;
                            }

                            if (cbaccounter != "")
                            {
                                Msg = "销售出单号[" + item.cCode + "] 行号[" + item.irowno + "] 存货编码[" + item.cInvCode + "] 在U8系统已记账，不可修改价格";
                                break;
                            }

                            //销售发货单存在则修改价格
                            list.Add("update " + DataBase + "..DispatchLists set iQuotedPrice=" + item.iOriTaxCost + ",iUnitPrice=" + item.iOriCost + ",iTaxUnitPrice=" + item.iOriTaxCost + ",iMoney=" + item.iOriMoney + ",iTax=" + item.iOriTaxPrice + ",iSum=" + item.ioriSum + ",iNatUnitPrice=" + item.iOriCost + ",iNatMoney=" + item.iOriMoney + ",iNatTax=" + item.iOriTaxPrice + ",iTaxRate=" + item.iTaxRate + ",iNatSum=" + item.ioriSum + ",fSaleCost=" + item.iOriTaxCost + ",fSalePrice=" + item.ioriSum + " where iDLsID='" + iDLsID + "'");

                            //销售出库单存在则修改价格
                            list.Add("update " + DataBase + "..Rdrecords32 set iUnitCost='" + item.iOriCost + "',iPrice=" + item.iOriMoney + " where iDLsID='" + iDLsID + "' ");
                        }
                        else
                        {
                            Msg = "销售出单号[" + item.cCode + "] 行号[" + item.irowno + "] 存货编码[" + item.cInvCode + "] 在U8系统不存在!";
                        }
                    }

                    if (Msg == "")
                    {
                        if (list.Count > 0)
                        {
                            bool rs = Bdbase.ExecuteSqlTran(list);
                            if (rs)
                            {
                                return new ApiMessage { mescode = 1, message = "成功" };
                            }
                            else
                            {
                                return new ApiMessage { mescode = 0, message = "销售出库物料价格变更接口错误!" };
                            }
                        }
                        else
                        {
                            return new ApiMessage { mescode = 1, message = "成功" };
                        }
                    }
                    else
                    {
                        return new ApiMessage { mescode = 0, message = Msg };
                    }
                }
                else
                {
                    return new ApiMessage { mescode = 0, message = "请传入有效的json!" };
                }
            }
            catch (Exception ex)
            {
                return new ApiMessage { mescode = 0, message = "销售出库物料价格变更接口异常:" + ex.Message };
            }
        }
    }
}