using LeaRun.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using YYC_SRMApiProject.Common;
using YYC_SRMApiProject.Models.Rdrecord10;
using YYC_SRMApiProject.Models.Token;

namespace YYC_SRMApiProject.Webapi.Methods
{
    public class Rdrecord10Methods
    {
        static string U8Code = ConfigHelper.AppSettings("U8Code");//U8账套编码
        static string GysDatabase = ConfigHelper.AppSettings("GysDatabase");//数据库名称

        /// <summary>
        /// 入库条码查询接口
        /// </summary>
        /// <param name="Account"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static ApiMessage<DataTable> GetProcessflowBarCode(Base_flowBarCode Base_flowBarCode)
        {
            try
            {
                if (Base_flowBarCode != null)
                {
                    //条码
                    if (string.IsNullOrWhiteSpace(Base_flowBarCode.BarCode))
                    {
                        return new ApiMessage<DataTable> { success = false, statecode = -1, message = "条码不允许为空" };
                    }
                    else
                    {
                        string sqlcc = "select a.BarCode,a.DoCodeStr,a.BarQty,a.cBatch,a.cInvCode,b.cInvName,b.cInvStd,b.cInvAddCode,Replace(b.cInvMnemCode,'@','')cInvMnemCode,c.cComUnitName,te.rkstate,a.cWhCode,w.cWhName,a.cPostion,p.cPosName from " + GysDatabase + "..ProcessflowBarCode a (nolock)  left join Inventory b on a.cInvCode = b.cInvCode left join ComputationUnit c (nolock) on b.cComunitCode = c.cComunitCode left join (select a.BarID,max(case when isnull(b.ccode,'')='' then '0' else '1' end)rkstate from " + GysDatabase + "..ProcessflowBarCodes a (nolock) left join rdrecord10 b (nolock) on a.rkcCode=b.cCode where BarID in(select BarID from " + GysDatabase + "..ProcessflowBarCode (nolock) where BarCodeGroup in (select BarCodeGroup from " + GysDatabase + "..ProcessflowBarCode (nolock) where BarCode = '" + Base_flowBarCode.BarCode + "')) group by a.BarID)te on a.BarID=te.BarID left join Warehouse  w (nolock) on a.cWhCode = w.cWhCode left join Position p (nolock)on a.cPostion = p.cPosCode  where BarCodeGroup in (select BarCodeGroup from " + GysDatabase + "..ProcessflowBarCode (nolock) where BarCode = '" + Base_flowBarCode.BarCode + "')";
                        DataSet ds = Bdbase.QueryU8(sqlcc);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            //查询是否存在未入库条码
                            DataRow[] dr = ds.Tables[0].Select("rkstate=0");
                            if (dr.Length > 0)
                            {
                                return new ApiMessage<DataTable> { success = true, statecode = 1, message = "查询成功", subdata = ds.Tables[0] };

                            }
                            else
                            {
                                return new ApiMessage<DataTable> { success = false, statecode = -1, message = "查询失败：该条码已办理入库" };
                            }
                        }
                        else
                        {
                            return new ApiMessage<DataTable> { success = false, statecode = -1, message = "查询失败：该条码在系统中不存在" };
                        }
                    }
                }
                else
                {
                    return new ApiMessage<DataTable> { success = false, statecode = -1, message = "查询失败：请传入有效的数据" };
                }
            }
            catch (Exception ex)
            {
                return new ApiMessage<DataTable> { success = false, statecode = -1, message = "查询失败:" + ex.Message };
            }
        }


        /// <summary>
        /// 产品入库单新增
        /// </summary>
        /// <param name="rdrecord10"></param>
        /// <returns></returns>
        public static ApiMessage<string> setRdrecord10(List<Base_Rdrecord10> rdrecord10List)
        {
            try
            {
                List<string> list = new List<string>();
                Hashtable ht = new Hashtable();
                List<string> listBatch = new List<string>();
                if (rdrecord10List != null && rdrecord10List.Count > 0)
                {

                    Dictionary<string, decimal> dicmodid = new Dictionary<string, decimal>();

                    foreach (Base_Rdrecord10 rdrecord10 in rdrecord10List)
                    {

                        string sqlBar = "select a.BarId,b.Autoid,a.BarID,a.BarCode,a.BarQty,a.cBatch,c.InvCode cInvCode,a.PrintCount,b.DocCode,b.PFID,b.MoDId,b.MoCode,b.SortSeq,b.Qty,c.MDeptCode,c.SoDId,c.SoCode,c.SoSeq,c.SoType,c.MolotCode,t.OpSeq,t.Description,t.WcCode,c.MoId,(case when isnull(rd10.cCode,'')='' then '0' else '1' end)rkstate from " + GysDatabase + "..ProcessflowBarCode a (nolock) left join " + GysDatabase + "..ProcessflowBarCodes b (nolock) on a.BarID=b.BarID left join rdrecord10 rd10 (nolock)  on b.rkccode=rd10.cCode   left join mom_orderdetail c (nolock) on c.MoDId=b.MoDId LEFT JOIN (select * from(SELECT *,ROW_NUMBER()OVER(PARTITION BY MoDId ORDER BY OpSeq desc) AS RN FROM (SELECT d.MoDId,OpSeq,e.Description,s.WcCode FROM sfc_morouting d (nolock) left join sfc_moroutingdetail e (nolock) on e.MoRoutingId=d.MoRoutingId left join sfc_workcenter s (nolock) on s.WcId=e.WcId )a ) AS T WHERE RN=1)t on b.MoDId=t.MoDId where BarCode = '" + rdrecord10.BarCode + "'  select distinct c.MDeptCode from " + GysDatabase + "..ProcessflowBarCode a (nolock) left join " + GysDatabase + "..ProcessflowBarCodes b (nolock) on a.BarID=b.BarID left join mom_orderdetail c (nolock) on c.MoDId=b.MoDId where BarCode='" + rdrecord10.BarCode + "'";
                        DataSet dsBar = Bdbase.QueryU8(sqlBar);
                        if (dsBar.Tables[0].Rows.Count == 0)
                        {
                            return new ApiMessage<string> { statecode = -1, success = false, message = "条码[" + rdrecord10.BarCode + "]未找到相应的明细数据!" };
                        }
                        else
                        {
                            DataRow[] dsdr = dsBar.Tables[0].Select("rkstate=1");
                            if (dsdr.Length > 0)
                            {
                                return new ApiMessage<string> { statecode = -1, success = false, message = "条码[" + rdrecord10.BarCode + "]已办理入库!" };
                            }
                        }

                        //----------------------------------------------
                        //制单人
                        if (string.IsNullOrWhiteSpace(rdrecord10.cMaker))
                        {
                            return new ApiMessage<string> { statecode = -1, success = false, message = "制单人不允许为空!" };
                        }

                        string bWhPos = "0";
                        string cWhCode = "";
                        //仓库
                        if (string.IsNullOrWhiteSpace(rdrecord10.cWhCode))
                        {
                            return new ApiMessage<string> { statecode = -1, success = false, message = "仓库不允许为空!" };
                        }
                        else
                        {
                            string sql = "select * from WareHouse (nolock) where cWhCode='" + rdrecord10.cWhCode + "'";
                            DataTable dt = Bdbase.QueryU8(sql).Tables[0];
                            if (DataHelper.IsExistRows(dt))
                            {
                                return new ApiMessage<string> { statecode = -1, success = false, message = "仓库[" + rdrecord10.cWhCode + "]在U8系统中不存在!" };
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



                        //当前仓库启用货位管理
                        string iposflag = "NULL";
                        string cPosCode = "";
                        string cPosName = "";
                        if (bWhPos == "1")
                        {
                            if (string.IsNullOrWhiteSpace(rdrecord10.cPosCode))
                            {
                                return new ApiMessage<string> { statecode = -1, success = false, message = "仓库[" + cWhCode + "]设置货位管理，货位不允许为空!" };
                            }
                            else
                            {
                                string sqlhw = "select * from Position (nolock) where cWhCode='" + cWhCode + "' and cPosCode='" + rdrecord10.cPosCode + "' and bPosEnd=1";
                                DataSet dshw = Bdbase.QueryU8(sqlhw);
                                if (DataHelper.IsExistRows(dshw.Tables[0]))
                                {
                                    return new ApiMessage<string> { statecode = -1, success = false, message = "U8仓库[" + rdrecord10.cWhCode + "]未设置货位[" + rdrecord10.cPosCode + "]" };
                                }
                                iposflag = "1";
                                cPosCode = dshw.Tables[0].Rows[0]["cPosCode"].ToString();
                                cPosName = dshw.Tables[0].Rows[0]["cPosName"].ToString();

                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(rdrecord10.cPosCode))
                            {
                                return new ApiMessage<string> { statecode = -1, success = false, message = "U8仓库[" + rdrecord10.cWhCode + "]未启用货位管理，不允许录入货位!" };
                            }
                        }

                        //根据部门拆单
                        for (int i = 0; i < dsBar.Tables[1].Rows.Count; i++)
                        {
                            //生产部门
                            string MDeptCode = dsBar.Tables[1].Rows[i]["MDeptCode"].ToString();
                            DataRow[] drs = dsBar.Tables[0].Select("MDeptCode='" + MDeptCode + "'");

                            string cInvCode = drs[0]["cInvCode"].ToString();
                            string bInvBatch = "0";
                            //存货编码
                            if (string.IsNullOrWhiteSpace(cInvCode))
                            {
                                return new ApiMessage<string> { statecode = -1, success = false, message = "存货编码不允许为空!" };
                            }
                            else
                            {
                                string sqlcinv = "select cInvCode,bInvBatch from Inventory (nolock) where cInvCode='" + cInvCode + "'";
                                DataTable cinvds = Bdbase.Query(sqlcinv).Tables[0];
                                if (DataHelper.IsExistRows(cinvds))
                                {
                                    return new ApiMessage<string> { statecode = -1, success = false, message = "存货编码[" + cInvCode + "]在U8系统中不存在!" };
                                }
                                if (cinvds.Rows[0]["bInvBatch"].ToString() == "True")
                                {
                                    bInvBatch = "1";
                                }
                            }

                            string cCode = GetRd10cCode();

                            #region 获取ID
                            string sqlId = @"declare @p5 int
                                        declare @p6 int
                                        exec sp_GetId N'',N'" + U8Code + "',N'rd'," + drs.Length + ",@p5 output,@p6 output,default select @p5, @p6";
                            DataSet dsID = Bdbase.QueryU8(sqlId);
                            string Id = dsID.Tables[0].Rows[0][0].ToString();
                            #endregion

                            list.Add("insert into rdrecord10 ([ID],[bRdFlag],[cVouchType],[cBusType],[cSource],[cBusCode],[cWhCode],[dDate],[cCode],[cRdCode],[cDepCode],[cPersonCode],[cPTCode],[cSTCode],[cCusCode],[cVenCode],[cOrderCode],[cARVCode],[cBillCode],[cDLCode],[cProBatch],[cHandler],[cMemo],[bTransFlag],[cAccounter],[cMaker],[cDefine1],[cDefine2],[cDefine3],[cDefine4],[cDefine5],[cDefine6],[cDefine7],[cDefine8],[cDefine9],[cDefine10],[dKeepDate],[dVeriDate],[bpufirst],[biafirst],[iMQuantity],[dARVDate],[cChkCode],[dChkDate],[cChkPerson],[VT_ID],[bIsSTQc],[cDefine11],[cDefine12],[cDefine13],[cDefine14],[cDefine15],[cDefine16],[cMPoCode],[gspcheck],[ipurorderid],[iproorderid],[iExchRate],[cExch_Name],[bOMFirst],[bFromPreYear],[bIsLsQuery],[bIsComplement],[iDiscountTaxType],[ireturncount],[iverifystate],[iswfcontrolled],[cModifyPerson],[dModifyDate],[dnmaketime],[dnmodifytime],[dnverifytime],[bredvouch],[iFlowId],[cPZID],[cSourceLs],[cSourceCodeLs],[iPrintCount],[csysbarcode],[cCurrentAuditor],[outid],[cCheckSignFlag]) values('" + Id + "', 1, '10', '成品入库', '生产订单', Null, '" + cWhCode + "', convert(varchar(10),getdate(),120), '" + cCode + "', '102', '" + MDeptCode + "', Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, '" + rdrecord10.cMaker + "', Null, '0', Null, '" + rdrecord10.cMaker + "', Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, convert(varchar(10),getdate(),120), '0', '0', Null, Null, Null, Null, Null, 63, '0', Null, Null, Null, 'Y', Null, Null, '" + drs[0]["MoCode"].ToString() + "', Null, Null, '" + drs[0]["MoId"].ToString() + "', Null, Null, '0', '0', '0', 0, 0, 0, 0, 0, '', Null,getdate(), Null, getdate(), 0, Null, Null, Null, Null, 0, '||st10|" + cCode + "', Null, Null, '" + Guid.NewGuid().ToString().Replace("-", "") + "')");

                            //是否记账标识
                            string bCosting = Bdbase.GetValueU8("select (case when bInCost=1 then 1 else 0 end)bInCost from Warehouse (nolock) where cWhCode='" + cWhCode + "'");

                            int zbid = int.Parse(dsID.Tables[0].Rows[0][1].ToString()) - drs.Length + 1;
                            int irowno = 1;
                            foreach (DataRow drBar in drs)
                            {
                                decimal iQuantity = 0;
                                if (drBar["Qty"].ToString() == "0" || string.IsNullOrWhiteSpace(drBar["Qty"].ToString()))
                                {
                                    return new ApiMessage<string> { statecode = -1, success = false, message = "数量不允许为空或者0!" };
                                }
                                else
                                {
                                    if (!decimal.TryParse(drBar["Qty"].ToString(), out iQuantity))
                                    {
                                        return new ApiMessage<string> { statecode = -1, success = false, message = "数量格式不正确!" };
                                    }
                                }

                                #region 处理现存量
                                //批次号
                                string cBatch = "";
                                #region 获取itemid
                                string itemid = "";
                                string sqlitemid = "select * from dbo.SCM_Item (nolock) where cInvCode='" + cInvCode + "'";
                                DataTable dtitemid = Bdbase.QueryU8(sqlitemid).Tables[0];
                                if (DataHelper.IsExistRows(dtitemid))
                                {
                                    Bdbase.ExecuteSqlU8("insert into dbo.scm_item ([cInvCode],[cFree1],[cFree2],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[PartId]) values('" + cInvCode + "','','','','','','','','','','',0)");
                                    //重新获取itemid
                                    itemid = Bdbase.QueryU8(sqlitemid).Tables[0].Rows[0]["id"].ToString();
                                }
                                else
                                {
                                    //获取itemid
                                    itemid = dtitemid.Rows[0]["id"].ToString();
                                }
                                #endregion

                                //启用批次
                                if (bInvBatch == "1")
                                {
                                    //cBatch = DateTime.Now.ToString("yyyy-MM-dd") + "//" + drBar["MoCode"].ToString() + "-" + drBar["SortSeq"].ToString();
                                    cBatch = drBar["cBatch"].ToString();
                                    //判断批次档案是否存在，不存在则新增
                                    string sqlcbat = "select cInvCode from AA_BatchProperty (nolock) where cBatch='" + cBatch + "' and cInvCode='" + cInvCode + "'";
                                    DataSet dsbat = Bdbase.QueryU8(sqlcbat);
                                    if (dsbat.Tables[0].Rows.Count == 0 && !listBatch.Contains(cInvCode + cBatch))
                                    {
                                        listBatch.Add(cInvCode + cBatch);
                                        list.Add("insert into AA_BatchProperty(cBatchPropertyGUID,cInvCode,cBatch,cFree1,cFree2,cFree3,cFree4,cFree5,cFree6,cFree7,cFree8,cFree9,cFree10,cBatchProperty6,cBatchProperty7,cBatchProperty8,cBatchProperty9) values(newid(),'" + cInvCode + "','" + cBatch + "','','','','','','','','','','','" + rdrecord10.cbMemo1 + "','" + rdrecord10.cbMemo2 + "','" + rdrecord10.cbMemo3 + "','" + rdrecord10.cbMemo4 + "')");
                                    }
                                    else
                                    {
                                        list.Add("update AA_BatchProperty set cBatchProperty6='" + rdrecord10.cbMemo1 + "',cBatchProperty7='" + rdrecord10.cbMemo2 + "',cBatchProperty8='" + rdrecord10.cbMemo3 + "',cBatchProperty9='" + rdrecord10.cbMemo4 + "' where cInvCode='" + cInvCode + "' and cBatch='" + cBatch + "'");
                                    }
                                    #region 现存量表
                                    string sqlxcl = "select * from CurrentStock (nolock) where cInvCode='" + cInvCode + "' and cWhCode='" + cWhCode + "' and cBatch='" + cBatch + "'";
                                    DataTable dtxcl = Bdbase.QueryU8(sqlxcl).Tables[0];
                                    if (DataHelper.IsExistRows(dtxcl))
                                    {
                                        //如果现存量记录不存在则新增
                                        Bdbase.ExecuteSqlU8(@"insert into dbo.CurrentStock([cWhCode],[cInvCode],[ItemId],[cBatch],[cVMIVenCode],[iSoType],[iSodid],[iQuantity],[iNum],[cFree1],[cFree2],[fOutQuantity],[fOutNum],[fInQuantity],[fInNum],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[dVDate],[bStopFlag],[fTransInQuantity],[dMdate],[fTransInNum],[fTransOutQuantity],[fTransOutNum] ,[fPlanQuantity] ,[fPlanNum],[fDisableQuantity],[fDisableNum],[fAvaQuantity],[fAvaNum],[iMassDate],[BGSPSTOP],[cMassUnit],[fStopQuantity] ,[fStopNum],[dLastCheckDate],[cCheckState],[dLastYearCheckDate],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[ipeqty],[ipenum]) values('" + cWhCode + "','" + cInvCode + "','" + itemid + "','" + cBatch + "','',0,'','0',0,'','',0,0,0,0,'','','','','','','','',NULL,0,0,NULL,0,0,0,0,0,0,0,0,0,NULL,0,NULL,0,0,NULL,'',NULL,'0',NULL,NULL,0,0)");
                                        //如果现存量记录存在则修改数量
                                    }
                                    list.Add("update CurrentStock set iQuantity=iQuantity+" + iQuantity + " where cInvCode='" + cInvCode + "' and cWhCode='" + cWhCode + "' and cBatch='" + cBatch + "'");
                                    #endregion

                                    #region 处理货位存量表
                                    if (!string.IsNullOrWhiteSpace(cPosCode))
                                    {
                                        string sqlhwcl = "select * from invpositionsum (nolock) where cWhCode='" + cWhCode + "' and cInvCode='" + cInvCode + "' and cPosCode='" + cPosCode + "' and cBatch='" + cBatch + "'";
                                        DataTable hwcldt = Bdbase.QueryU8(sqlhwcl).Tables[0];
                                        if (DataHelper.IsExistRows(hwcldt))
                                        {
                                            Bdbase.ExecuteSqlU8("insert into invpositionsum ([cWhCode],[cPosCode],[cInvCode],[iQuantity],[inum],[cBatch],[cFree1],[cFree2],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[iTrackid],[cvmivencode],[cMassUnit],[iMassDate],[dMadeDate],[dVDate],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cInVouchType]) values('" + cWhCode + "','" + cPosCode + "','" + cInvCode + "',0,NULL,'" + cBatch + "','','','','','','','','','','','0','',Null,Null,Null,Null,0,Null,Null,'')");
                                        }
                                        list.Add("update invpositionsum set iQuantity=iQuantity+'" + iQuantity + "' where cWhCode='" + cWhCode + "' and cInvCode='" + cInvCode + "' and cPosCode='" + cPosCode + "' and cBatch='" + cBatch + "'");
                                        list.Add("insert into invposition ([RdsID],[RdID],[cWhCode],[cPosCode],[cInvCode],[cBatch],[cFree1],[cFree2],[dVDate],[iQuantity],[iNum],[cMemo],[cHandler],[dDate],[bRdFlag],[cSource],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[cAssUnit],[cBVencode],[iTrackId],[dMadeDate],[iMassDate],[cMassUnit],[cvmivencode],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cvouchtype],[cInVouchType],[cVerifier],[dVeriDate],[dVouchDate]) values('" + zbid + "','" + Id + "','" + cWhCode + "','" + cPosCode + "','" + cInvCode + "','" + cBatch + "','','',Null,'" + iQuantity + "',Null,'','" + rdrecord10.cMaker + "',convert(varchar(10),getdate(),120),1,'','','','','','','','','',Null,Null,0,Null,Null,Null,'',0,Null,Null,'10','',Null,Null,convert(varchar(10),getdate(),120))");
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region 现存量表
                                    string sqlxcl = "select * from CurrentStock (nolock) where cInvCode='" + cInvCode + "' and cWhCode='" + cWhCode + "'";
                                    DataTable dtxcl = Bdbase.QueryU8(sqlxcl).Tables[0];
                                    if (DataHelper.IsExistRows(dtxcl))
                                    {
                                        //如果现存量记录不存在则新增
                                        Bdbase.ExecuteSqlU8(@"insert into dbo.CurrentStock([cWhCode],[cInvCode],[ItemId],[cBatch],[cVMIVenCode],[iSoType],[iSodid],[iQuantity],[iNum],[cFree1],[cFree2],[fOutQuantity],[fOutNum],[fInQuantity],[fInNum],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[dVDate],[bStopFlag],[fTransInQuantity],[dMdate],[fTransInNum],[fTransOutQuantity],[fTransOutNum] ,[fPlanQuantity] ,[fPlanNum],[fDisableQuantity],[fDisableNum],[fAvaQuantity],[fAvaNum],[iMassDate],[BGSPSTOP],[cMassUnit],[fStopQuantity] ,[fStopNum],[dLastCheckDate],[cCheckState],[dLastYearCheckDate],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[ipeqty],[ipenum]) values('" + cWhCode + "','" + cInvCode + "','" + itemid + "','','',0,'','0',0,'','',0,0,0,0,'','','','','','','','',NULL,0,0,NULL,0,0,0,0,0,0,0,0,0,NULL,0,NULL,0,0,NULL,'',NULL,'0',NULL,NULL,0,0)");
                                        //如果现存量记录存在则修改数量
                                        list.Add("update CurrentStock set iQuantity=iQuantity+" + iQuantity + " where cInvCode='" + cInvCode + "' and cWhCode='" + cWhCode + "'");
                                    }
                                    else
                                    {
                                        //如果现存量记录存在则修改数量
                                        list.Add("update CurrentStock set iQuantity=iQuantity+" + iQuantity + " where cInvCode='" + cInvCode + "' and cWhCode='" + cWhCode + "'");
                                    }
                                    #endregion

                                    #region 处理货位存量表
                                    if (!string.IsNullOrWhiteSpace(cPosCode))
                                    {
                                        string sqlhwcl = "select * from invpositionsum (nolock) where cWhCode='" + cWhCode + "' and cInvCode='" + cInvCode + "' and cPosCode='" + cPosCode + "'";
                                        DataTable hwcldt = Bdbase.QueryU8(sqlhwcl).Tables[0];
                                        if (DataHelper.IsExistRows(hwcldt))
                                        {
                                            Bdbase.ExecuteSqlU8("insert into invpositionsum ([cWhCode],[cPosCode],[cInvCode],[iQuantity],[inum],[cBatch],[cFree1],[cFree2],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[iTrackid],[cvmivencode],[cMassUnit],[iMassDate],[dMadeDate],[dVDate],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cInVouchType]) values('" + cWhCode + "','" + cPosCode + "','" + cInvCode + "',0,NULL,'','','','','','','','','','','','0','',Null,Null,Null,Null,0,Null,Null,'')");
                                        }
                                        list.Add("update invpositionsum set iQuantity=iQuantity+'" + iQuantity + "' where cWhCode='" + cWhCode + "' and cInvCode='" + cInvCode + "' and cPosCode='" + cPosCode + "'");
                                        list.Add("insert into invposition ([RdsID],[RdID],[cWhCode],[cPosCode],[cInvCode],[cBatch],[cFree1],[cFree2],[dVDate],[iQuantity],[iNum],[cMemo],[cHandler],[dDate],[bRdFlag],[cSource],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[cAssUnit],[cBVencode],[iTrackId],[dMadeDate],[iMassDate],[cMassUnit],[cvmivencode],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cvouchtype],[cInVouchType],[cVerifier],[dVeriDate],[dVouchDate]) values('" + zbid + "','" + Id + "','" + cWhCode + "','" + cPosCode + "','" + cInvCode + "','','','',Null,'" + iQuantity + "',Null,'','" + rdrecord10.cMaker + "',convert(varchar(10),getdate(),120),1,'','','','','','','','','',Null,Null,0,Null,Null,Null,'',0,Null,Null,'10','',Null,Null,convert(varchar(10),getdate(),120))");
                                    }
                                    #endregion
                                }
                                #endregion

                                list.Add(@"insert into rdrecords10 ([AutoID],[ID],[cInvCode],[iNum],[iQuantity],[iUnitCost],[iPrice],[iAPrice],[iPUnitCost],[iPPrice],[cBatch],[cVouchCode],[cInVouchCode],[cinvouchtype],[iSOutQuantity],[iSOutNum],[cFree1],[cFree2],[iFlag],[iFNum],[iFQuantity],[dVDate],[cPosition],[cDefine22],[cDefine23],[cDefine24],[cDefine25],[cDefine26],[cDefine27],[cItem_class],[cItemCode],[cName],[cItemCName],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[cBarCode],[iNQuantity],[iNNum],[cAssUnit],[dMadeDate],[iMassDate],[cDefine28],[cDefine29],[cDefine30],[cDefine31],[cDefine32],[cDefine33],[cDefine34],[cDefine35],[cDefine36],[cDefine37],[iMPoIds],[iCheckIds],[cBVencode],[bGsp],[cGspState],[cCheckCode],[iCheckIdBaks],[cRejectCode],[iRejectIds],[cCheckPersonCode],[dCheckDate],[cMassUnit],[cMoLotCode],[bChecked],[bRelated],[cmworkcentercode],[bLPUseFree],[iRSRowNO],[iOriTrackID],[coritracktype],[cbaccounter],[dbKeepDate],[bCosting],[bVMIUsed],[iVMISettleQuantity],[iVMISettleNum],[cvmivencode],[iInvSNCount],[cwhpersoncode],[cwhpersonname],[cserviceoid],[cbserviceoid],[iinvexchrate],[corufts],[cmocode],[imoseq],[iopseq],[copdesc],[strContractGUID],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cciqbookcode],[iBondedSumQty],[productinids],[iorderdid],[iordertype],[iordercode],[iorderseq],[isodid],[isotype],[csocode],[isoseq],[cBatchProperty1],[cBatchProperty2],[cBatchProperty3],[cBatchProperty4],[cBatchProperty5],[cBatchProperty6],[cBatchProperty7],[cBatchProperty8],[cBatchProperty9],[cBatchProperty10],[cbMemo],[irowno],[strowguid],[cservicecode],[ipreuseqty],[ipreuseinum],[OutCopiedQuantity],[cbsysbarcode],[cplanlotcode],[bmergecheck],[imergecheckautoid],[iposflag],[iorderdetailid],[body_outid]) values('" + zbid + "','" + Id + "','" + cInvCode + "',Null,'" + iQuantity + "',Null,Null,Null,Null,Null,'" + cBatch + "',Null,Null,Null,Null,Null,Null,Null,0,Null,Null,Null,'" + cPosCode + "','" + drBar["DocCode"].ToString() + "',Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,'" + iQuantity + "',Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,NULL,'" + drBar["MoDId"].ToString() + "',Null,Null,'0',Null,Null,Null,Null,Null,Null,Null,Null,Null,'0','0','" + drBar["WcCode"].ToString() + "','0',0,'0',Null,Null,Null,'" + bCosting + "','0',Null,Null,Null,Null,Null,Null,Null,Null,Null,convert(nvarchar,convert(money,@@DBTS),2),'" + drBar["MoCode"].ToString() + "','" + drBar["SortSeq"].ToString() + "','" + drBar["OpSeq"].ToString() + "','" + drBar["Description"].ToString() + "',Null,0,Null,Null,Null,Null,Null,'" + drBar["SoDId"].ToString() + "','" + drBar["SoType"].ToString() + "','" + drBar["SoCode"].ToString() + "','" + drBar["SoSeq"].ToString() + "','" + drBar["SoDId"].ToString() + "','" + drBar["SoType"].ToString() + "','" + drBar["SoCode"].ToString() + "','" + drBar["SoSeq"].ToString() + "',Null,Null,Null,Null,Null,'" + rdrecord10.cbMemo1 + "','" + rdrecord10.cbMemo2 + "','" + rdrecord10.cbMemo3 + "','" + rdrecord10.cbMemo4+"',Null,Null,'" + irowno + "',Null,Null,Null,Null,Null,'||st10|" + cCode + "|" + irowno + "','" + drBar["MolotCode"].ToString() + "','0',Null," + iposflag + ",Null,Null)");

                                if (bCosting == "1")
                                {
                                    //未记账单据列表
                                    list.Add("insert into IA_ST_UnAccountVouch10(IDUN,IDSUN,cVouTypeUN,cBustypeUN) values('" + Id + "','" + zbid + "','10','成品入库') ");
                                }
                                //修改生产订单-入库累计数量
                                list.Add("update mom_orderdetail set QualifiedInQty=isnull(QualifiedInQty,0)+'" + iQuantity + "' where MoDId='" + drBar["MoDId"].ToString() + "'");
                                //修改生产订单工序资料可入库数量
                                list.Add("update sfc_moroutingdetail set BalQualifiedQty=BalQualifiedQty-'" + iQuantity + "'  where MoDId='" + drBar["MoDId"].ToString() + "' and LastFlag='1'");



                                //回写条码记录表
                                list.Add("update " + GysDatabase + "..ProcessflowBarCode set cWhCode='" + rdrecord10.cWhCode + "',cPostion='" + rdrecord10.cPosCode + "',rkbs=1,rktime=getdate(),rkuser='" + rdrecord10.cMaker + "' where BarID='" + drBar["BarId"].ToString() + "'");

                                list.Add("update " + GysDatabase + "..ProcessflowBarCodes set rkccode='" + cCode + "',rknumber='" + irowno + "' where Autoid='" + drBar["Autoid"].ToString() + "'");

                                //if (ht.ContainsKey(drBar["MoDId"].ToString()))
                                //{
                                //    ht[drBar["MoDId"].ToString()] = decimal.Parse(ht[drBar["MoDId"].ToString()].ToString()) + iQuantity;
                                //}
                                //else
                                //{
                                //    ht.Add(drBar["MoDId"].ToString(), iQuantity);
                                //}

                                decimal val;
                                if (dicmodid.TryGetValue(drBar["MoDId"].ToString(), out val))
                                {

                                    dicmodid[drBar["MoDId"].ToString()] = dicmodid[drBar["MoDId"].ToString()] + iQuantity;
                                }
                                else
                                {
                                    dicmodid.Add(drBar["MoDId"].ToString(), iQuantity);
                                }

                                zbid++;
                                irowno++;
                            }
                        }

                        //写入扫描条码入库表
                        list.Add("insert into " + GysDatabase + "..TableRKBarCode(BarCode,createdate) values('" + rdrecord10.BarCode + "',getdate())");
                    }


                    #region 验证订单是否超量入库
                    foreach (KeyValuePair<string, decimal> item in dicmodid)
                    {
                        string mosql = "select a.MoDId,MoCode,a.SortSeq,(a.Qty-isnull(a.QualifiedInQty,0))iQuantity from mom_orderdetail a (nolock) left join mom_order m (nolock) on m.MoId=a.MoId where MoDId='" + item.Key + "'";
                        DataSet dsmo = Bdbase.QueryU8(mosql);
                        if (dsmo != null && dsmo.Tables.Count > 0 && dsmo.Tables[0].Rows.Count > 0)
                        {
                            if (item.Value > decimal.Parse(dsmo.Tables[0].Rows[0]["iQuantity"].ToString()))
                            {
                                return new ApiMessage<string> { success = false, statecode = -1, message = "生产订单号[" + dsmo.Tables[0].Rows[0]["MoCode"].ToString() + "]行号[" + dsmo.Tables[0].Rows[0]["SortSeq"].ToString() + "]不允许超量入库!" };
                            }
                            else
                            {
                                string counts = Bdbase.GetValueU8("select count(1) from mom_orderdetail a (nolock) inner join mom_moallocate b (nolock) on a.MoDId = b.MoDId where a.MoDId = '" + item.Key + "' and b.IssQty<>b.Qty and (isnull(a.QualifiedInQty, 0) +" + item.Value + ") * (BaseQtyN / BaseQtyD) > b.IssQty + 0.02");
                                if (int.Parse(counts) > 0)
                                {
                                    return new ApiMessage<string> { success = false, statecode = -1, message = "生产订单号[" + dsmo.Tables[0].Rows[0]["MoCode"].ToString() + "]行号[" + dsmo.Tables[0].Rows[0]["SortSeq"].ToString() + "]领用材料不足" };
                                }
                            }
                        }
                        else
                        {
                            return new ApiMessage<string> { success = false, statecode = -1, message = "生产订单明细id[" + item.Key + "]在U8中不存在" };
                        }
                    }

                    #endregion


                    bool rs = Bdbase.ExecuteSqlTranU8(list);
                    if (rs)
                    {
                        return new ApiMessage<string> { success = true, statecode = 1, message = "成功" };
                    }
                    else
                    {
                        return new ApiMessage<string> { success = false, statecode = -1, message = "产成品入库单接口异常错误或条码已重复入库" };
                    }
                }
                else
                {
                    return new ApiMessage<string> { success = false, statecode = -1, message = "请传入有效的json!" };
                }
            }
            catch (Exception ex)
            {
                return new ApiMessage<string> { success = false, statecode = -1, message = "产成品入库接口异常:" + ex.Message };
            }
        }

        /// <summary>
        /// 获取单据号
        /// </summary>
        /// <returns></returns>
        public static string Rd10code()
        {

            #region 获取单号
            string cSeed = DateTime.Now.ToString("yyyyMM");
            string cCode = DateTime.Now.ToString("yyMM") + "B" + "00001";
            string iscsql = @"select RIGHT('00000' +cast((cNumber+1) as varchar),5) as cNumber From VoucherHistory  with (ROWLOCK)  Where  CardNumber='0411' and cContent='日期' and cSeed=right(CONVERT(varchar(6),GETDATE(), 112),4)";
            DataSet iscds = Bdbase.QueryU8(iscsql);
            if (iscds != null && iscds.Tables.Count > 0 && iscds.Tables[0].Rows.Count > 0)
            {
                //修改流水号
                Bdbase.ExecuteSqlU8("update VoucherHistory set cNumber=cNumber+1 Where  CardNumber='0411' and cContent='日期' and cSeed=right(CONVERT(varchar(6),GETDATE(), 112),4)");

                cCode = DateTime.Now.ToString("yyMM") + "B" + iscds.Tables[0].Rows[0]["cNumber"].ToString();

            }
            else
            {
                string sqlin = @"insert into VoucherHistory(CardNumber,cContent,cSeed,cNumber,bEmpty,ccontentrule) values('0411','日期',right(CONVERT(varchar(6),GETDATE(), 112),4),'1','0','月')";//修改单号
                Bdbase.ExecuteSqlU8(sqlin);
            }
            #endregion

            return cCode;
        }

        /// <summary>
        /// 获取单据号
        /// </summary>
        /// <returns></returns>
        public static string GetRd10cCode()
        {
            try
            {
                string ccode = Rd10code();
                List<string> listc = new List<string>();
                listc.Add("insert into " + GysDatabase + "..TableRd10cCode(RdcCode) values('" + ccode + "')");
                bool rest = Bdbase.ExecuteSqlTran(listc);
                if (rest)
                {
                    return ccode;
                }
                else
                {
                    return GetRd10cCode();
                }
            }
            catch (Exception)
            {
                return GetRd10cCode();
            }

        }
    }
}