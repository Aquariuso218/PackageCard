using Infrastructure.Attribute;
using Infrastructure.Model;
using Microsoft.Extensions.Configuration;
using SqlSugar.IOC;
using System.Data;
using Utilities.Base.Data;
using ZR.Model.Sunset.Model.Dto;
using ZR.Service.IService;

namespace ZR.Service
{
    [AppService(ServiceType = typeof(IRdrecord32Service), ServiceLifetime = LifeTime.Transient)]
    public class Rdrecord32Service : IRdrecord32Service
    {
        private readonly ISqlSugarClient U8db;
        private readonly ISqlSugarClient db;
        private readonly IConfiguration config;

        public Rdrecord32Service()
        {
            config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).
                AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            U8db = DbScoped.SugarScope.GetConnectionScope(1);
            db = DbScoped.SugarScope.GetConnectionScope(0);
        }

        public ApiResult stockOut(DispatchList dispatchList)
        {
            try
            {
                //账套编码
                string ztCode = config.GetSection("U8Configs").GetSection("ZTCode").Value;

                List<string> list = new List<string>();
                string MesError = "";

               
                //1.将group中明细按cWhCode分组,然后循环处理明细
                var whGroups = dispatchList.DispatchDetails.GroupBy(detail => detail.cWhCode);
                foreach (var whGroup in whGroups)
                {
                    string cWhCode = whGroup.Key;

                    //是否启用代管
                    string bProxyWh = "0";
                    //是否记账标识    
                    string bCosting = "1";

                    string iExpiratDateCalcu = "0";
                    string bWhPos = "0";
                    if (string.IsNullOrWhiteSpace(cWhCode))
                    {
                        return new ApiResult(0, "仓库不允许为空!");
                    }
                    else
                    {
                        string sql = "select * from WareHouse (nolock) where cWhCode='" + cWhCode + "'";
                        DataTable dt = U8db.Ado.GetDataTable(sql);
                        if (DataHelper.IsExistRows(dt))
                        {
                            return new ApiResult(0, "仓库[" + cWhCode + "]在U8系统中不存在!");
                        }
                        else
                        {
                            if (dt.Rows[0]["bWhPos"].ToString() == "True")
                            {
                                bWhPos = "1";
                            }
                            if (dt.Rows[0]["bProxyWh"].ToString() == "True")
                            {
                                bProxyWh = "1";
                            }
                        }
                    }
                    //不允许仓库启用货位管理
                    if (bWhPos == "1")
                    {
                        return new ApiResult(0, "仓库[" + cWhCode + "]设置了货位管理，请取消货位管理!");
                    }
                    //不允许仓库启用货位管理
                    if (bProxyWh == "1")
                    {
                        return new ApiResult(0, "仓库[" + cWhCode + "]设置启用代管仓，请检查!");
                    }

                    Dictionary<string, decimal> diciDLsID = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> dicCuStock = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> dicPosSum = new Dictionary<string, decimal>();

                    //2.将DispatchDetails中的DispatchListDetail按DLCode分组，并循环。
                    var groupedDetails = whGroup.GroupBy(detail => detail.cDLCode);
                    foreach (var dlGroup in groupedDetails)
                    {
                        string DLCode = dlGroup.Key;

                        //发货单id
                        string sql1 = "select DLID from DispatchList (nolock) where cDLCode='" + DLCode + "'";
                        string DLID = U8db.Ado.GetString(sql1);
                        if (DLID == "")
                        {
                            return new ApiResult(-1, "发货单号[" + DLCode + "]在U8系统中不存在!");
                        }

                        //收发类别编码
                        string cRdCode = "202";

                        #region 获取销售出库单ID和单号
                        string sqlid = @"declare @p5 int
                                set @p5=1000000134
                                declare @p6 int
                                set @p6=1000000154
                                exec sp_GetId N'',N'" + ztCode + "',N'rd'," + dlGroup.Count() + ",@p5 output,@p6 output,default select @p5, @p6";

                        DataSet db = U8db.Ado.GetDataSetAll(sqlid);//获取材料出库主表子表ID
                        #endregion

                        int cid = (int.Parse(db.Tables[0].Rows[0][1].ToString()) - dlGroup.Count()) + 1;//第一列子表标识
                        string cCode = Rd32code();               //获取销售出库单单号
                        string id = db.Tables[0].Rows[0][0].ToString(); //获取销售出库单ID

                        //销售出库单主表
                        list.Add("insert into rdrecord32(id,brdflag,cvouchtype,cbustype,csource,cbuscode,cwhcode,ddate,ccode,crdcode,cdepcode,cstcode,ccuscode,cdlcode,cmaker,vt_id,bisstqc,bomfirst,iswfcontrolled,dnmaketime,cinvoicecompany,csysbarcode,dnverifytime,dVeriDate,cHandler,cPersonCode,cExch_Name,cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16,cMemo,cShipAddress,caddcode) select '" + id + "','0','32',cbustype,'发货单',cDLCode,'" + cWhCode + "',convert(varchar(10),getdate(),120),'" + cCode + "'," + cRdCode + ",cdepcode,cstcode,ccuscode,DLID,'" + dispatchList.Cmaker + "',87,0,0,0, getdate(),cinvoicecompany,'||st32|" + cCode + "',getdate(),convert(varchar(10),getdate(),120),'" + dispatchList.Cmaker + "',cPersonCode,cExch_Name,cDefine1,cDefine2,cDefine3,cDefine4,cDefine5,cDefine6,cDefine7,cDefine8,cDefine9,cDefine10,cDefine11,cDefine12,cDefine13,cDefine14,cDefine15,cDefine16,cMemo,cShipAddress,caddcode  from DispatchList (nolock) where DLID='" + DLID + "'");

                        //3.循环仓库分组中明细并处理表体
                        foreach (var detail in dlGroup)
                        {
                            //获取销售发货单
                            DataSet dsID = U8db.Ado.GetDataSetAll("select a.cFree1,cbatchproperty6,iDLsID,cPosition,cWhCode,cInvCode,cBatch,iQuantity,(iQuantity-isnull(fOutQuantity,0))cCKQty,cvmivencode,dMDate,dvDate,isnull(iExpiratDateCalcu,0)iExpiratDateCalcu,dExpirationdate,cExpirationdate,iMassDate,cMassUnit,(case when bReturnFlag=1 then 1 else 0 end)bReturnFlag  from DispatchLists a (nolock) left join DispatchList b (nolock) on a.DLID=b.DLID where a.DLID='" + DLID + "' and irowno='" + detail.irowno + "' and isnull(cSCloser,'')=''");

                            if (dsID != null && dsID.Tables.Count > 0 && dsID.Tables[0].Rows.Count > 0)
                            {
                                //待写入批次属性
                                string cbatchproperty6 = dsID.Tables[0].Rows[0]["cbatchproperty6"].ToString();
                                //待写入自由项
                                string cFree1 = dsID.Tables[0].Rows[0]["cFree1"].ToString();

                                // 销售出库单表体
                                list.Add("insert into rdrecords32(AutoID,ID,cInvCode,iNum,iQuantity,cBatch,iFlag,dVDate,cPosition,iDLsID,iNQuantity,iNNum,cAssUnit,dMadeDate,iMassDate,bGsp,cMassUnit,bLPUseFree,iRSRowNO,iOriTrackID,bCosting,bVMIUsed,iinvexchrate,cvmivencode,cbdlcode,corufts,iExpiratDateCalcu,cExpirationdate,dExpirationdate,iorderdid,iordertype,iordercode,iorderseq,ipesodid,ipesotype,cpesocode,ipesoseq,isodid,isotype,csocode,isoseq,irowno,cbsysbarcode,bIAcreatebill,bsaleoutcreatebill,bneedbill,iposflag,cbMemo,cDefine22,cDefine23,cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37,cItemCode,cItem_class,cName,cItemCName,cFree1,cFree2,cFree3,cFree4,cFree5,cFree6,cFree7,cFree8,cFree9,cFree10,cbatchproperty1,cbatchproperty2,cbatchproperty3,cbatchproperty4,cbatchproperty5,cbatchproperty6,cbatchproperty7,cbatchproperty8,cbatchproperty9,cbatchproperty10) select '" + cid + "', '" + id + "','" + detail.CInvCode + "',(case when cUnitID is null then null else Round(" + detail.IQuantity + "/iInvExchRate,2) end),'" + detail.IQuantity + "'," + isTrNull(detail.CBatch) + ", 0,Null, Null, iDLsID,(isnull(iQuantity,0)-isnull(fOutQuantity,0)),(case when iInvExchRate is null then null else (isnull(iNum,0)-isnull(fOutNum,0)) end),cUnitID,Null,Null,bGsp,Null, 0, 0, 0, " + bCosting + ", " + bProxyWh + ", iInvExchRate,Null, '" + DLCode + "',convert(nvarchar,convert(money,@@DBTS),2)," + iExpiratDateCalcu + ",Null,Null, iSOsID,(case when iSOsID is null then 0 else 1 end), cordercode, iorderrowno, iSOsID,(case when iSOsID is null then 0 else 1 end), cordercode, iorderrowno,Null,0,Null,Null," + detail.irowno + ", '||st32|" + cCode + "|" + detail.irowno + "', a.bIAcreatebill, b.bsaleoutcreatebill, b.bneedbill,NULL,b.cMemo,cDefine22,'" + cCode + "',cDefine24,cDefine25,cDefine26,cDefine27,cDefine28,cDefine29,cDefine30,cDefine31,cDefine32,cDefine33,cDefine34,cDefine35,cDefine36,cDefine37,cItemCode,cItem_class,cItemName,cItem_CName,cFree1,cFree2,cFree3,cFree4,cFree5,cFree6,cFree7,cFree8,cFree9,cFree10,cbatchproperty1,cbatchproperty2,cbatchproperty3,cbatchproperty4,cbatchproperty5,cbatchproperty6,cbatchproperty7,cbatchproperty8,cbatchproperty9,cbatchproperty10 from DispatchLists a (nolock) left join DispatchList b (nolock) on a.DLID=b.DLID  where iDLsID = '" + detail.iDLsID + "'");

                                if (bCosting == "1")
                                {
                                    // 未记账单据列表
                                    list.Add(@"insert into IA_ST_UnAccountVouch32(IDUN,IDSUN,cVouTypeUN,cBustypeUN) VALUES('" + db.Tables[0].Rows[0][0].ToString() + "','" + cid + "','32','销售出库')");
                                }

                                //回写发货单已出库数量
                                list.Add("update DispatchLists set fOutQuantity=isnull(fOutQuantity,0)+'" + detail.IQuantity + "'  where iDLsID = '" + detail.iDLsID + "' ");

                                //回写发货单库存出库标识
                                list.Add("update DispatchList set csaleout='ST'  where DLID = '" + DLID + "' ");

                                //回写销售订单已出库数量
                                list.Add("update c set c.fOutQuantity=isnull(c.fOutQuantity,0)+'" + detail.IQuantity + "' from DispatchLists a (nolock) inner join SO_SODetails c (nolock) on c.iSOsID = a.iSOsID where a.iDLsID = '" + detail.iDLsID + "'");

                                //修改现存量 (添加cfree1！)
                                list.Add("update CurrentStock set iQuantity=isnull(iQuantity,0)-'" + detail.IQuantity + "',fOutQuantity=isnull(fOutQuantity,0)-'" + detail.IQuantity + "' where cInvCode='" + detail.CInvCode + "' and cWhCode='" + detail.cWhCode + "' and cBatch='" + detail.CBatch + "' and isnull(cvmivencode,'')='' and isnull(cFree1, '')='"+ cFree1 + "'");

                                //记录发货出库数量
                                decimal val;
                                if (diciDLsID.TryGetValue(detail.iDLsID, out val))
                                {

                                    diciDLsID[detail.iDLsID] = diciDLsID[detail.iDLsID] + detail.IQuantity;
                                }
                                else
                                {
                                    diciDLsID.Add(detail.iDLsID, detail.IQuantity);
                                }

                                string InvStr = detail.cWhCode + "₮" + detail.CInvCode + "₮" + detail.CBatch + "₮" + cFree1 + "₮";
                                //记录物料领料数量
                                decimal cval = 0;
                                if (dicCuStock.TryGetValue(InvStr, out cval))
                                {

                                    dicCuStock[InvStr] = dicCuStock[InvStr] + detail.IQuantity;
                                }
                                else
                                {
                                    dicCuStock.Add(InvStr, detail.IQuantity);
                                }
                            }
                            else
                            {
                                return new ApiResult(-1, $"发货单[" + DLCode + "] 行号[" + detail.irowno + "]在ERP中不存在或已关闭!");
                            }
                            cid++;
                        }
                    }

                    #region 校验销售发货出库数量
                    foreach (KeyValuePair<string, decimal> item in diciDLsID)
                    {
                        //验证现存量是否充足
                        string sqlxcl = "SELECT cInvCode,(iQuantity-isnull(fOutQuantity,0))cCKQty,cBatch from DispatchLists (nolock) where iDLsID = '" + item.Key + "'";
                        DataSet dsxcl = U8db.Ado.GetDataSetAll(sqlxcl);
                        if (!DataHelper.IsExistRows(dsxcl.Tables[0]))
                        {
                            decimal Qty = decimal.Parse(dsxcl.Tables[0].Rows[0]["cCKQty"].ToString());
                            if (Qty < item.Value)
                            {
                                MesError = "产品编码[" + dsxcl.Tables[0].Rows[0]["cInvCode"].ToString() + "],批次[" + dsxcl.Tables[0].Rows[0]["cBatch"].ToString() + "]已经完成出库, 请刷新列表";
                                break;
                            }
                        }
                        else
                        {
                            MesError = "产品编码[" + dsxcl.Tables[0].Rows[0]["cInvCode"].ToString() + "],批次[" + dsxcl.Tables[0].Rows[0]["cBatch"].ToString() + "]已经完成出库, 请刷新列表";
                            break;
                        }

                    }
                    #endregion

                    if (MesError == "")
                    {
                        #region 校验仓库现存量
                        foreach (KeyValuePair<string, decimal> item in dicCuStock)
                        {
                            string[] cWStr = item.Key.Split('₮');
                            //验证现存量是否充足
                            string sqlxcl = "select iQuantity from currentStock (nolock) where cWhCode='" + cWStr[0] + "' and cInvCode='" + cWStr[1] + "' and cBatch='" + cWStr[2] + "' and cFree1='" + cWStr[3] + "'";
                            DataSet dsxcl = U8db.Ado.GetDataSetAll(sqlxcl);
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
                        #endregion
                    }
                }

                if (MesError == "")
                {
                    try
                    {
                        // 开启U8db事务
                        U8db.Ado.BeginTran();
                        // 执行U8db的SQL语句
                        foreach (string sql in list)
                        {
                            U8db.Ado.ExecuteCommand(sql);
                        }
                        // 提交事务
                        U8db.Ado.CommitTran();
                    }
                    catch (Exception ex)
                    {
                        U8db.Ado.RollbackTran();
                        return new ApiResult(-1, $"销售出库单生成失败:{ex.Message}");
                    }
                }
                else
                {
                    return new ApiResult(-1, MesError);
                }

                return new ApiResult(1, $"出库成功！");
            }
            catch (Exception ex)
            {
                return new ApiResult(-1, $"销售出库单发生异常:{ex.Message}");
            }
        }


        /// <summary>
        /// 获取单据号
        /// </summary>
        /// <returns></returns>
        public string Rd32code()
        {

            #region 获取单号
            string cSeed = DateTime.Now.ToString("yyyyMMdd").Substring(2, 6); // 输出 "250424"
            string cCode = "XSCK" + cSeed + "0001";
            string iscsql = @"select RIGHT('0000' +cast((cNumber+1) as varchar),4) as cNumber From VoucherHistory  with (ROWLOCK)  Where  CardNumber='0303' and cContent='日期' and cSeed= '" + cSeed + "'";
            DataSet iscds = U8db.Ado.GetDataSetAll(iscsql);
            if (iscds != null && iscds.Tables.Count > 0 && iscds.Tables[0].Rows.Count > 0)
            {
                //修改流水号
                U8db.Ado.ExecuteCommand(@"update VoucherHistory set cNumber=cNumber+1 Where  CardNumber='0303' and cContent='日期' and cSeed='" + cSeed + "'");

                cCode = "XSCK" + cSeed + iscds.Tables[0].Rows[0]["cNumber"].ToString();

            }
            else
            {
                string sqlin = @"insert into VoucherHistory(CardNumber,cContent,ccontentrule,cSeed,cNumber,bEmpty) values('0303','日期','日','" + cSeed + "','1', '0')";//修改单号
                U8db.Ado.ExecuteCommand(sqlin);
            }
            #endregion

            return cCode;
        }

        public static string isTrNull(string strvalue)
        {
            if (string.IsNullOrWhiteSpace(strvalue))
            {
                return "Null";
            }
            else
            {
                return "'" + strvalue + "'";
            }
        }
    }
}
