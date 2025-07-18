﻿using Infrastructure.Attribute;
using SqlSugar.IOC;
using ZR.Model.Business.Model;
using ZR.Service.IService;
using ZR.Repository;
using ZR.Model.Sunset.Model.Dto;
using ZR.Model.Sunset.Model;
using System.Data;
using Microsoft.Extensions.Configuration;
using Infrastructure.Model;
using Infrastructure;
using ZR.Model.Sunset;

namespace ZR.Service
{
    [AppService(ServiceType = typeof(IPackagingLabelService), ServiceLifetime = LifeTime.Transient)]
    public class PackingLabelService : BaseService<PackageCard>, IPackagingLabelService, IDynamicApi
    {

        private readonly ISqlSugarClient db;
        private readonly ISqlSugarClient U8db;
        private readonly IConfiguration config;
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public PackingLabelService()
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
        /// 删除整箱
        /// </summary>
        /// <param name="boxNumber"></param>
        /// <returns></returns>
        public MyApiResult DeletePackageCard(string boxNumber)
        {
            // 检查箱内是否有已入库的产品（isFlag == 1）
            bool hasStockedProducts = db.Queryable<PackageCardDetails>()
                                      .Any(pc => pc.boxNumber == boxNumber && pc.isFlag == 1);

            // 如果有已入库产品，不允许删除
            if (hasStockedProducts)
            {

                return new MyApiResult
                {
                    Code = 0,
                    Msg = "箱子存在已出库产品不允许删除!"
                };
            }

            try
            {
                // 开启事务，确保数据一致性
                db.Ado.BeginTran();

                // 先删除子表（PackageCardDetails）
                db.Deleteable<PackageCardDetails>()
                  .Where(pc => pc.boxNumber == boxNumber)
                  .ExecuteCommand();

                // 再删除主表（PackageCard）
                db.Deleteable<PackageCard>()
                  .Where(pc => pc.boxNumber == boxNumber)
                  .ExecuteCommand();

                // 提交事务
                db.Ado.CommitTran();
                return new MyApiResult
                {
                    Code = 1,
                    Msg = "删除成功!"
                };
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                logger.Info($"删除子表数据异常: {ex.Message}");
                return new MyApiResult
                {
                    Code = -1,
                    Msg = $"数据删除失败：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 删除单个产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MyApiResult DeletePCDetails(int id)
        {
            try
            {
                // 1. 先获取要删除的明细记录（用于后续操作）
                var detail = db.Queryable<PackageCardDetails>()
                              .Where(pcd => pcd.id == id)
                              .First();
                if (detail == null)
                {
                    return new MyApiResult{ 
                        Code= 0,
                        Msg = "未找到对应的明细记录"
                    };
                }

                // 2. 获取关联的主表实体
                var mainCard = db.Queryable<PackageCard>()
                               .Where(pc => pc.boxNumber == detail.boxNumber)
                               .First();
                if (mainCard == null)
                {
                    return new MyApiResult
                    {
                        Code = 0,
                        Msg = "未找到对应的主表记录"
                    };
                }

                // 3. 执行删除明细操作
                int affectedRows = db.Deleteable<PackageCardDetails>()
                                   .Where(pcd => pcd.id == id)
                                   .ExecuteCommand();

                if (affectedRows <= 0)
                {
                    return new MyApiResult
                    {
                        Code = 0,
                        Msg = "删除明细失败"
                    };
                }

                // 4. 检查箱子是否还有剩余产品
                if (mainCard.quantity - affectedRows > 0)
                {
                    // 4.1 更新箱子已装箱数量
                    mainCard.quantity -= affectedRows;

                    // 4.2 更新条码信息

                    var barcodeParts = mainCard.barCode.Split('|');
                    if (barcodeParts.Length >= 3)
                    {
                        // 获取当前产品ID列表
                        var productIds = barcodeParts[2].Split('/').ToList();
                        // 移除被删除的产品ID
                        productIds.RemoveAll(x => x == detail.invID.ToString());
                        // 重新拼接条码
                        barcodeParts[2] = string.Join("/", productIds);
                        mainCard.barCode = string.Join("|", barcodeParts);
                        db.Updateable(mainCard).ExecuteCommand();
                    }

                    return new MyApiResult
                    {
                        Code = 1,
                        Msg = "删除成功"
                    };
                }
                else
                {
                    // 5. 没有剩余产品，删除主表记录
                    db.Deleteable<PackageCard>()
                      .Where(pc => pc.boxNumber == mainCard.boxNumber)
                      .ExecuteCommand();

                    return new MyApiResult
                    {
                        Code = 1,
                        Msg = "删除成功，箱子已被永久删除"
                    };
                }
            }
            catch (Exception ex)
            {
                // 记录日志
                logger.Error($"删除包装卡明细失败：{ex.Message}", ex);
                return new MyApiResult
                {
                    Code = 0,
                    Msg = $"删除操作失败：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 更新 PackageCard 主表及明细表
        /// </summary>
        /// <param name="card">要更新的卡片对象</param>
        /// <returns>操作结果消息</returns>
        public int UpdatePC(PackageCard card)
        {
            try
            {
                db.Ado.UseTran(() =>
                {
                    // 1. 同步更新明细表 PackageCardDetails
                    db.Updateable<PackageCardDetails>()
                        .SetColumns(it => it.isChange == card.isChange)
                        .Where(it => it.boxNumber == card.boxNumber && it.id == card.Details.First().id)
                        .ExecuteCommand();

                    // 2. 查询是否有符合条件的记录
                    bool hasChanged = db.Queryable<PackageCardDetails>()
                        .Where(it => it.boxNumber == card.boxNumber && it.isChange == 1)
                        .Any();

                    // 3. 更新主表 PackageCard
                    db.Updateable<PackageCard>()
                        .SetColumns(it => it.isChange == (hasChanged ? 1 : 0))
                        .Where(it => it.boxNumber == card.boxNumber)
                        .ExecuteCommand();
                });
                return 1;
            }
            catch (Exception ex)
            {
                // 记录日志
                logger.Info($"更新失败: {ex.Message}");

                return 0;
            }
        }


        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="simulatedData"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public PagedInfo<SimulatedData> getInvList(SimulatedData simulatedData, PagerInfo pager)
        {
            try
            {
                var exp = Expressionable.Create<SimulatedData>();

                //if (!string.IsNullOrEmpty(simulatedData.cInvcCode))    //判断是否特殊装箱
                //{
                //    exp.And(u => u.cInvcCode == simulatedData.cInvcCode);
                //}
                //else
                //{
                //    exp.And(u => u.cInvcCode != "1064");
                //}

                exp.AndIF(!string.IsNullOrEmpty(simulatedData.mergeBoxCode), u => u.invName.Contains(simulatedData.mergeBoxCode));
                exp.AndIF(!string.IsNullOrEmpty(simulatedData.invCode), u => u.invCode.Contains(simulatedData.invCode));
                exp.AndIF(!string.IsNullOrEmpty(simulatedData.invName), u => u.invName.Contains(simulatedData.invName));
                exp.AndIF(simulatedData.beginTime != DateTime.MinValue && simulatedData.beginTime != null, u => DateTime.Parse(u.docDate) >= simulatedData.beginTime);
                exp.AndIF(simulatedData.endTime != DateTime.MinValue && simulatedData.endTime != null, u => DateTime.Parse(u.docDate) <= simulatedData.endTime);

                // 查询所有数据
                var query = db.Queryable<SimulatedData>().Where(exp.ToExpression()).OrderByDescending(x => x.docDate);

                // 直接进行分页处理并返回
                return query.ToPage(pager);
            }
            catch (Exception ex)
            {
                throw new Exception("产品列表获取异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 装箱列表
        /// </summary>
        /// <param name="package"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public PagedInfo<PackageCard> getPCList(PackageCard package, PagerInfo pager)
        {
            try
            {
                // 创建表达式构建器，根据 boxNumber 和 invCode 进行筛选
                var exp = Expressionable.Create<PackageCard>();
                exp.AndIF(!string.IsNullOrEmpty(package.boxNumber), u => u.boxNumber.Contains(package.boxNumber));
                exp.AndIF(!string.IsNullOrEmpty(package.invCode), u => u.invCode.Contains(package.invCode));
                exp.AndIF(!string.IsNullOrEmpty(package.invName), u => u.invName.Contains(package.invName));
                exp.AndIF(package.isZeroBox, u => u.boxQty > u.quantity);
                exp.AndIF(package.beginTime != DateTime.MinValue && package.beginTime != null, u => u.createdTime >= package.beginTime);
                exp.AndIF(package.endTime != DateTime.MinValue && package.endTime != null, u => u.createdTime <= package.endTime);

                // 查询主表数据（PackageCard）并应用筛选条件
                var query = db.Queryable<PackageCard>()
                    .Where(exp.ToExpression())
                    .OrderByDescending(d => d.Id);

                // 筛选项：箱子产品是否全部入库
                if (package.isNoInStock)
                {
                    // 未全部入库：检查存在 isflag != 1 的记录
                    query = query.Where(pc =>
                        SqlFunc.Subqueryable<PackageCardDetails>()
                            .Where(d => d.boxNumber == pc.boxNumber && d.isFlag == 0)
                            .Any());
                }

                // 获取分页数据
                var packageCards = query.ToPage(pager);

                // 遍历主表数据，查询并填充子表数据
                foreach (var card in packageCards.Result)
                {
                    // 查询对应的子表数据
                    var packageDetails = db.Queryable<PackageCardDetails>()
                        .Where(d => d.boxNumber == card.boxNumber) // 通过boxNumber与主表关联
                        .OrderBy(d => d.id) // 按createdTime升序排序
                                            // 或者使用.OrderByDescending(d => d.id) 按降序排序
                        .ToList();

                    // 将查询到的子表数据添加到主表数据中
                    card.Details = packageDetails;
                }

                return packageCards;
            }
            catch (Exception ex)
            {
                throw new Exception("装箱列表获取异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 普通装箱
        /// </summary>
        /// <returns>信息</returns>
        public string createPackage(PackageQuery query)
        {
            List<SimulatedData> simulatedData = query.simulatedDatas;
            string createName = query.createName;

            if (simulatedData == null || simulatedData.Count == 0)
            {
                return "请选择装箱数据";
            }

            try
            {
                db.Ado.BeginTran();

                // 过滤未装箱的数据
                var unPackedData = simulatedData.ToList();
                if (!unPackedData.Any())
                {
                    db.Ado.CommitTran();
                    return "您选择的数据已经完成装箱";
                }

                //检索simulatedData中是否有多个不同的invCode,存在时return
                var invCodes = unPackedData
                .GroupBy(d => d.invCode)
                .Select(d => d.Key)
                .ToList();
                if (invCodes.Count > 1)
                {
                    db.Ado.RollbackTran();
                    return "请选择相同产品进行装箱";
                }

                List<string> MoDIdList = new List<string>();

                int isClosePacking = 0; //是否封箱(0:否; 1:是)

                //按产品编码分组
                var groupedData = unPackedData.GroupBy(d => d.invCode);
                foreach (var group in groupedData)
                {

                    //初始化分组变量(为后续装箱准备必要数据,流转卡号用于子表记录，ID 用于更新 isflag。)
                    string invCode = group.Key;//分组的键，即产品编码。
                    string invName = group.First().invName;//从分组第一个元素获取产品名称（假设同一 invCode 的名称相同）
                    string pictureCode = group.First().pictureCode;
                    string customerName = group.First().customerName;
                    string sql = "SELECT top 1 cInvDefine12 FROM Inventory (nolock) where cInvCode = '" + invCode + "'";
                    int MAX_PER_BOX = U8db.Ado.GetInt(sql);
                    if (MAX_PER_BOX == 0)
                    {
                        return $"产品  {invCode} 未设置装箱数量！";
                    }

                    int totalQuantity = group.Sum(x => x.quantity);//计算该产品总数量。
                    var flowCards = group.Select(x => new { x.flowCard, x.id, x.MoDId, x.PFId, x.pictureCode, x.quantity }).ToList(); //提取流转卡号和 ID 组成匿名对象列表。
                    int flowCardIndex = 0;//追踪当前处理的流转卡索引

                    int remainingQuantity = totalQuantity;  //初始化为总数量，表示剩余待装箱数量

                    string currentBoxNumber = "";

                    //用户选择零箱时操作
                    if (query.isZero)
                    {
                        //检索未装满的箱子
                        List<PackageCard> lastUnfilledCards = db.Queryable<PackageCard>()
                        .Where(pc => pc.invCode == invCode && pc.isClosePacking == 0 && pc.quantity < pc.boxQty && pc.isClosePacking == 0)
                        .OrderBy(pc => pc.createdTime, OrderByType.Desc)
                        .ToList();

                        //填满未装满的箱子
                        if (lastUnfilledCards != null && lastUnfilledCards.Count > 0)
                        {
                            foreach (var lastUnfilledCard in lastUnfilledCards)
                            {
                                currentBoxNumber = lastUnfilledCard.boxNumber;

                                // 如果全部已发货，此箱子不再处理
                                if (IsBoxFullyShipped(db, currentBoxNumber))
                                {
                                    db.Updateable<PackageCard>()
                                  .SetColumns(p => new PackageCard { isClosePacking = 1 })
                                  .Where(p => p.boxNumber == currentBoxNumber)
                                  .ExecuteCommand();
                                    continue; // 跳过当前箱子
                                }

                                int currentBoxQuantity = lastUnfilledCard?.quantity ?? 0;  //获取当前箱子已装数量
                                string thisBarCode = lastUnfilledCard?.barCode;                 //获取条码信息
                                int spaceLeft = lastUnfilledCard.boxQty - currentBoxQuantity;
                                int fillQuantity = Math.Min(spaceLeft, remainingQuantity);

                                for (int i = 0; i < fillQuantity && flowCardIndex < flowCards.Count; i++)
                                {
                                    //存入订单号供生成领料单使用
                                    if (!MoDIdList.Contains(flowCards[flowCardIndex].MoDId))
                                    {
                                        MoDIdList.Add(flowCards[flowCardIndex].MoDId);
                                    }

                                    thisBarCode += "/" + flowCards[i].id;

                                    var detail = new PackageCardDetails
                                    {
                                        invID = flowCards[flowCardIndex].id,
                                        boxNumber = currentBoxNumber,
                                        invCode = invCode,
                                        invName = invName,
                                        flowCard = flowCards[flowCardIndex].flowCard,
                                        quantity = flowCards[flowCardIndex].quantity,
                                        createdBy = createName,
                                        createdTime = DateTime.Now,
                                        MoDId = flowCards[flowCardIndex].MoDId,
                                        PFId = flowCards[flowCardIndex].PFId,
                                        invAddCode = flowCards[flowCardIndex].pictureCode,
                                        isFlag = 0
                                    };

                                    db.Insertable(detail).ExecuteCommand();

                                    flowCardIndex++;
                                }

                                db.Updateable<PackageCard>()
                                    .SetColumns(pc => new PackageCard { barCode = thisBarCode, quantity = currentBoxQuantity + fillQuantity, modifiedTime = DateTime.Now, modifiedBy = createName })
                                    .Where(pc => pc.boxNumber == currentBoxNumber)
                                    .ExecuteCommand();

                                remainingQuantity -= fillQuantity;
                                currentBoxQuantity += fillQuantity;

                            }
                        }
                    }
                    else
                    {
                        isClosePacking = 1; //封箱
                    }

                    //处理剩余数量（新箱子）
                    while (remainingQuantity > 0)
                    {
                        int packQuantity = Math.Min(MAX_PER_BOX, remainingQuantity);
                        currentBoxNumber = getSerialNumber();
                        string newBarCode = pictureCode + "|" + currentBoxNumber + "|";

                        for (int i = 0; i < packQuantity && flowCardIndex < flowCards.Count; i++)
                        {
                            //存入订单号供生成领料单使用
                            if (!MoDIdList.Contains(flowCards[flowCardIndex].MoDId))
                            {
                                MoDIdList.Add(flowCards[flowCardIndex].MoDId);
                            }

                            if (i == 0)
                            {
                                newBarCode += flowCards[flowCardIndex].id;
                            }
                            else
                            {
                                newBarCode += "/" + flowCards[flowCardIndex].id;
                            }

                            var detail = new PackageCardDetails
                            {
                                invID = flowCards[flowCardIndex].id,
                                boxNumber = currentBoxNumber,
                                invCode = invCode,
                                invName = invName,
                                flowCard = flowCards[flowCardIndex].flowCard,
                                quantity = 1,
                                createdBy = createName,
                                createdTime = DateTime.Now,
                                MoDId = flowCards[flowCardIndex].MoDId,
                                PFId = flowCards[flowCardIndex].PFId,
                                invAddCode = flowCards[flowCardIndex].pictureCode,
                                isFlag = 0
                            };
                            db.Insertable(detail).ExecuteCommand();

                            flowCardIndex++;
                        }

                        var packageCard = new PackageCard
                        {
                            boxNumber = currentBoxNumber,
                            invCode = invCode,
                            invName = invName,
                            pictureCode = pictureCode,
                            customerName = customerName,
                            quantity = packQuantity,
                            createBy = createName,
                            createdTime = DateTime.Now,
                            boxQty = MAX_PER_BOX,
                            barCode = newBarCode,
                            isClosePacking = isClosePacking
                        };
                        db.Insertable(packageCard).ExecuteCommand();

                        remainingQuantity -= packQuantity;
                    }
                }
                db.Ado.CommitTran();

                string message = materialappvouchs(MoDIdList, createName);

                if (message == "1")
                {
                    return "装箱成功!领料成功!";
                }
                if (message == "2")
                {
                    return "装箱成功!";
                }
                else
                {
                    db.Ado.RollbackTran();
                    return "装箱失败:" + message;
                }

            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                return $"装箱异常: {ex.Message}";
            }
        }

        /// <summary>
        /// 产品合并装箱
        /// </summary>
        /// <returns>信息</returns>
        public string mergeCreatePackage(PackageQuery query)
        {
            List<SimulatedData> simulatedData = query.simulatedDatas;
            string createName = query.createName;
            if (simulatedData == null || simulatedData.Count == 0)
            {
                return "请选择装箱数据";
            }

            try
            {
                db.Ado.BeginTran();

                // 过滤未装箱的数据
                var unPackedData = simulatedData.ToList();
                if (unPackedData.Count == 0)
                {
                    db.Ado.CommitTran();
                    return "您选择的数据已经完成装箱";
                }

                //检索simulatedData中是否有多个不同的invCode,存在时return
                var invCodes = unPackedData
                                .GroupBy(d => d.invCode)
                                .Select(d => d.Key)
                                .ToList();

                if (invCodes.Count > 1)
                {
                    db.Ado.RollbackTran();
                    return "请选择相同产品进行装箱";
                }

                List<string> MoDIdList = new List<string>();

                // 按 invCode 分组
                var groupedData = unPackedData.GroupBy(d => d.invCode);
                foreach (var group in groupedData)
                {
                    string invCode = group.Key;
                    string invName = group.First().invName;
                    string mergeBoxCode = group.First().mergeBoxCode;
                    string pictureCode = group.First().pictureCode;
                    string customerName = group.First().customerName;
                    int totalQuantity = group.Sum(x => x.quantity);
                    var flowCards = group.Select(x => new { x.flowCard, x.id, x.MoDId, x.PFId, x.pictureCode, x.quantity }).ToList();
                    int flowCardIndex = 0;
                    int remainingQuantity = totalQuantity;

                    string sql = "SELECT top 1 cInvDefine12 FROM Inventory (nolock) where cInvCode = '" + invCode + "'";
                    int MAX_PER_BOX = U8db.Ado.GetInt(sql);
                    if (MAX_PER_BOX == 0)
                    {
                        return $"产品  {invCode} 未设置装箱数量";
                    }

                    // 步骤1：检查该产品自身的未装满箱子
                    var lastUnfilledCard = db.Queryable<PackageCard>()
                        .Where(pc => pc.invCode == invCode && (pc.quantity < pc.boxQty) && pc.isClosePacking == 0)
                        .OrderBy(pc => pc.createdTime, OrderByType.Desc).ToList();

                    string currentBoxNumber = "";
                    int currentBoxQuantity = 0;

                    if (lastUnfilledCard.Any())
                    {
                        foreach (var card in lastUnfilledCard)
                        {
                            currentBoxNumber = card.boxNumber;

                            // 如果全部已发货，此箱子不再处理
                            if (IsBoxFullyShipped(db, currentBoxNumber))
                            {
                                db.Updateable<PackageCard>()
                                  .SetColumns(p => new PackageCard { isClosePacking = 1 })
                                  .Where(p => p.boxNumber == currentBoxNumber)
                                  .ExecuteCommand();

                                continue; // 跳过当前箱子
                            }

                            currentBoxQuantity = card.quantity;
                            int spaceLeft = card.boxQty - currentBoxQuantity;
                            int fillQuantity = Math.Min(spaceLeft, remainingQuantity);
                            string thisBarCode = card.barCode;

                            for (int i = 0; i < fillQuantity && flowCardIndex < flowCards.Count; i++)
                            {
                                //存入订单号供生成领料单使用
                                if (!MoDIdList.Contains(flowCards[flowCardIndex].MoDId))
                                {
                                    MoDIdList.Add(flowCards[flowCardIndex].MoDId);
                                }

                                thisBarCode += "/" + flowCards[flowCardIndex].id;

                                var detail = new PackageCardDetails
                                {
                                    invID = flowCards[flowCardIndex].id,
                                    boxNumber = currentBoxNumber,
                                    invCode = invCode,
                                    invName = invName,
                                    flowCard = flowCards[flowCardIndex].flowCard,
                                    quantity = flowCards[flowCardIndex].quantity,
                                    createdBy = createName,
                                    createdTime = DateTime.Now,
                                    MoDId = flowCards[flowCardIndex].MoDId,
                                    PFId = flowCards[flowCardIndex].PFId,
                                    invAddCode = flowCards[flowCardIndex].pictureCode,
                                    isFlag = 0
                                };
                                db.Insertable(detail).ExecuteCommand();

                                flowCardIndex++;
                            }

                            db.Updateable<PackageCard>()
                                .SetColumns(pc => new PackageCard { barCode = thisBarCode, quantity = currentBoxQuantity + fillQuantity, modifiedTime = DateTime.Now, modifiedBy = createName })
                                .Where(pc => pc.boxNumber == currentBoxNumber)
                                .ExecuteCommand();

                            remainingQuantity -= fillQuantity;
                            currentBoxQuantity = card.boxQty; // 已装满
                        }
                    }

                    // 步骤2：检查合箱码对应的未装满箱子
                    if (!string.IsNullOrEmpty(mergeBoxCode))
                    {
                        if (remainingQuantity > 0)
                        {
                            var mergeUnfilledCard = db.Queryable<PackageCard>()
                                .Where(pc => pc.invCode == mergeBoxCode && pc.quantity < pc.boxQty && pc.isClosePacking == 0)
                                .OrderBy(pc => pc.createdTime, OrderByType.Desc)
                                .ToList();

                            if (mergeUnfilledCard.Any())
                            {
                                foreach (var card in mergeUnfilledCard)
                                {
                                    currentBoxNumber = card.boxNumber;

                                    // 如果全部已发货，此箱子不再处理
                                    if (IsBoxFullyShipped(db, currentBoxNumber))
                                    {
                                        db.Updateable<PackageCard>()
                                          .SetColumns(p => new PackageCard { isClosePacking = 1 })
                                          .Where(p => p.boxNumber == currentBoxNumber)
                                          .ExecuteCommand();

                                        continue; // 跳过当前箱子
                                    }

                                    currentBoxQuantity = card.quantity;
                                    int spaceLeft = card.boxQty - currentBoxQuantity;
                                    int fillQuantity = Math.Min(spaceLeft, remainingQuantity);
                                    string thisBarCode = card.barCode;

                                    for (int i = 0; i < fillQuantity && flowCardIndex < flowCards.Count; i++)
                                    {
                                        //存入订单号供生成领料单使用
                                        if (!MoDIdList.Contains(flowCards[flowCardIndex].MoDId))
                                        {
                                            MoDIdList.Add(flowCards[flowCardIndex].MoDId);
                                        }

                                        thisBarCode += "/" + flowCards[flowCardIndex].id;

                                        var detail = new PackageCardDetails
                                        {
                                            invID = flowCards[flowCardIndex].id,
                                            boxNumber = currentBoxNumber,
                                            invCode = invCode,
                                            invName = invName,
                                            flowCard = flowCards[flowCardIndex].flowCard,
                                            quantity = flowCards[flowCardIndex].quantity,
                                            createdBy = createName,
                                            createdTime = DateTime.Now,
                                            MoDId = flowCards[flowCardIndex].MoDId,
                                            PFId = flowCards[flowCardIndex].PFId,
                                            invAddCode = flowCards[flowCardIndex].pictureCode,
                                            isFlag = 0
                                        };
                                        db.Insertable(detail).ExecuteCommand();

                                        flowCardIndex++;
                                    }

                                    db.Updateable<PackageCard>()
                                        .SetColumns(pc => new PackageCard { barCode = thisBarCode, quantity = currentBoxQuantity + fillQuantity, modifiedTime = DateTime.Now, modifiedBy = createName })
                                        .Where(pc => pc.boxNumber == currentBoxNumber)
                                        .ExecuteCommand();

                                    remainingQuantity -= fillQuantity;
                                }
                            }
                        }
                    }
                    else
                    {
                        return $"产品{invCode}未设置合箱编码！";
                    }

                    // 步骤3：剩余产品按普通装箱规则处理
                    while (remainingQuantity > 0)
                    {
                        int packQuantity = Math.Min(MAX_PER_BOX, remainingQuantity);
                        currentBoxNumber = getSerialNumber();
                        string thisBarCode = pictureCode + "|" + currentBoxNumber + "|";

                        for (int i = 0; i < packQuantity && flowCardIndex < flowCards.Count; i++)
                        {
                            //存入订单号供生成领料单使用
                            if (!MoDIdList.Contains(flowCards[flowCardIndex].MoDId))
                            {
                                MoDIdList.Add(flowCards[flowCardIndex].MoDId);
                            }

                            if (i == 0)
                            {
                                thisBarCode += flowCards[flowCardIndex].id;
                            }
                            else
                            {
                                thisBarCode += "/" + flowCards[flowCardIndex].id;
                            }
                            var detail = new PackageCardDetails
                            {
                                invID = flowCards[flowCardIndex].id,
                                boxNumber = currentBoxNumber,
                                invCode = invCode,
                                invName = invName,
                                flowCard = flowCards[flowCardIndex].flowCard,
                                quantity = flowCards[flowCardIndex].quantity,
                                createdBy = createName,
                                createdTime = DateTime.Now,
                                MoDId = flowCards[flowCardIndex].MoDId,
                                PFId = flowCards[flowCardIndex].PFId,
                                invAddCode = flowCards[flowCardIndex].pictureCode,
                                isFlag = 0
                            };
                            db.Insertable(detail).ExecuteCommand();

                            flowCardIndex++;
                        }

                        var packageCard = new PackageCard
                        {
                            boxNumber = currentBoxNumber,
                            invCode = invCode,
                            invName = invName,
                            pictureCode = pictureCode,
                            customerName = customerName,
                            quantity = packQuantity,
                            createBy = createName,
                            createdTime = DateTime.Now,
                            boxQty = MAX_PER_BOX,
                            barCode = thisBarCode,
                        };
                        db.Insertable(packageCard).ExecuteCommand();

                        remainingQuantity -= packQuantity;
                    }
                }

                db.Ado.CommitTran();

                string message = materialappvouchs(MoDIdList, createName);

                if (message == "1")
                {
                    return "合箱装箱成功!领料成功!";
                }
                if (message == "2")
                {
                    return "合箱装箱成功!";
                }
                else
                {
                    db.Ado.RollbackTran();
                    return "合箱装箱失败:" + message;
                }
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                return $"合箱装箱异常: {ex.Message}";
            }
        }

        /// <summary>
        /// 特殊装箱方法，根据用户指定的箱号进行装箱，每箱只能装一个产品
        /// </summary>
        /// <param name="query">包含箱号的产品数据列表</param>
        /// <returns>操作结果信息</returns>
        public string createSpcPackage(PackageQuery1 query)
        {
            List<SimulatedDto> simulatedData = query.simulatedDatas;

            string createName = query.createName;

            if (simulatedData == null || simulatedData.Count == 0)
            {
                return "请选择装箱数据";
            }

            //检索simulatedData中是否有多个不同的invCode,存在时return
            var invCodes = simulatedData
                            .GroupBy(d => d.invCode)
                            .Select(d => d.Key)
                            .ToList();

            if (invCodes.Count > 1)
            {
                return "请选择相同产品进行装箱";
            }

            List<string> MoDIdList = new List<string>();

            try
            {
                db.Ado.BeginTran();

                // 按箱号分组
                var groupedByBox = simulatedData
                    .Where(d => d.isflag == 0) // 只处理未装箱的数据
                    .GroupBy(d => d.BoxNumber);

                if (!groupedByBox.Any())
                {
                    db.Ado.CommitTran();
                    return "您选择的数据已经完成装箱";
                }

                foreach (var boxGroup in groupedByBox)
                {
                    string boxNumber = boxGroup.Key;
                    if (string.IsNullOrEmpty(boxNumber))
                    {
                        throw new ArgumentException("请为需要装箱的产品输入箱号");
                    }

                    // 检查箱号是否已存在
                    var existingBox = db.Queryable<PackageCard>()
                        .Where(pc => pc.boxNumber == boxNumber)
                        .First();

                    // 不再按序号排序，直接获取分组数据
                    var items = boxGroup.ToList();

                    // 每箱只能装一个产品，检查分组数据是否超过1
                    if (items.Count > 1)
                    {
                        throw new ArgumentException($"输入箱号 {boxNumber} 重复");
                    }

                    var item = items.First(); // 只取第一条数据

                    if (existingBox == null)
                    {
                        // 新箱子，创建主表记录
                        var packageCard = new PackageCard
                        {
                            boxNumber = boxNumber,
                            invCode = item.invCode,
                            invName = item.invName,
                            pictureCode = item.pictureCode,
                            customerName = item.customerName,
                            quantity = item.quantity, // 每箱固定为1
                            createBy = createName,
                            createdTime = DateTime.Now,
                            boxQty = 1,
                            barCode = item.pictureCode + "|" + boxNumber + "|" + item.id
                        };
                        db.Insertable(packageCard).ExecuteCommand();
                    }
                    else
                    {
                        // 如果箱号已存在，不允许重复装箱
                        throw new ArgumentException($"箱号 {boxNumber}已存在");
                    }

                    // 处理子表详情
                    var existingDetail = db.Queryable<PackageCardDetails>()
                        .Where(d => d.boxNumber == boxNumber && d.flowCard == item.flowCard)
                        .Any();

                    if (!existingDetail)
                    {
                        //存入订单号供生成领料单使用
                        if (!MoDIdList.Contains(item.MoDId))
                        {
                            MoDIdList.Add(item.MoDId);
                        }

                        var detail = new PackageCardDetails
                        {
                            invID = item.id,
                            boxNumber = boxNumber,
                            invCode = item.invCode,
                            invName = item.invName,
                            flowCard = item.flowCard,
                            quantity = item.quantity, // 固定为1
                            createdBy = createName,
                            createdTime = DateTime.Now,
                            MoDId = item.MoDId,
                            PFId = item.PFId,
                            invAddCode = item.pictureCode,
                            isFlag = 0
                        };
                        db.Insertable(detail).ExecuteCommand();
                    }
                }

                db.Ado.CommitTran();

                string message = materialappvouchs(MoDIdList, createName);

                if (message == "1")
                {
                    return "装箱成功!领料成功!";
                }
                if (message == "2")
                {
                    return "装箱成功!";
                }
                else
                {
                    db.Ado.RollbackTran();
                    return "装箱失败:" + message;
                }
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                return $"装箱异常: {ex.Message}";
            }
        }

        /// <summary>
        /// 生成并获取最新流水号
        /// </summary>
        /// <returns></returns>
        public string getSerialNumber()
        {
            try
            {
                string currentDate = DateTime.Now.ToString("yyMMdd");
                int newSerialNumber;

                var oldSerial = db.Queryable<SerialNumbers>()
                                   .Where(sn => sn.serialDate == currentDate)
                                   .First();

                if (oldSerial == null)
                {
                    newSerialNumber = 1;
                    var newSerial = new SerialNumbers
                    {
                        serialDate = currentDate,
                        serialNumber = newSerialNumber,
                        fullSerial = $"{currentDate}-{newSerialNumber:D3}" // 流水号为3位
                    };
                    db.Insertable(newSerial).ExecuteCommand();
                }
                else
                {
                    newSerialNumber = oldSerial.serialNumber + 1;
                    oldSerial.serialNumber = newSerialNumber;
                    oldSerial.fullSerial = $"{currentDate}-{newSerialNumber:D3}";

                    db.Updateable(oldSerial).ExecuteCommand();
                }

                return $"{currentDate}-{newSerialNumber:D3}";
            }
            catch (Exception ex)
            {
                throw new Exception("流水号获取异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 获取领料申请单据号
        /// </summary>
        /// <returns></returns>
        public string getappcode()
        {
            try
            {
                #region 获取单据号
                string ccode = "0000000001";
                string sqlcnumber = "select right('00000000000'+convert(varchar(10),cNumber+1),10) as ccode from VoucherHistory with (UPDLOCK) Where CardNumber='0413' and cContent is null and cContentRule is null";
                DataSet dscnumber = U8db.Ado.GetDataSetAll(sqlcnumber);
                if (dscnumber != null && dscnumber.Tables[0].Rows.Count > 0 && dscnumber.Tables.Count > 0)//当月流水号是否存在
                {
                    ccode = dscnumber.Tables[0].Rows[0]["ccode"].ToString();
                    U8db.Ado.ExecuteCommand("update VoucherHistory set cNumber=cNumber+1 Where CardNumber='0413'  and cContent is null and cContentRule is null");
                }
                else
                {
                    string sqlin = @"insert into VoucherHistory(CardNumber,cContent,cContentRule,cSeed,cNumber,bEmpty) values('0413',null,null,null,'1','0')";
                    U8db.Ado.ExecuteCommand(sqlin);
                }
                return ccode;
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception("单据号获取异常:" + ex.Message);
            }

        }

        //生成领料申请单
        public string materialappvouchs(List<string> MoDIdList, string createNmae)
        {
            try
            {
                string username = createNmae;

                string ZTCode = config.GetSection("U8Configs").GetSection("ZTCode").Value;
                List<Materialappvouch> LinLiaoList = new List<Materialappvouch>();
                List<string> sqlList = new List<string>();

                foreach (var thisMoDid in MoDIdList)
                {
                    DataSet MomSet = U8db.Ado.GetDataSetAll(@"SELECT A.MoId,b.MoDId,A.MoCode,MDeptCode,B.SortSeq,b.InvCode,MolotCode,b.Qty,(c.Qty-c.RequisitionQty)cAppQty,c.AllocateId,c.SoType,c.SoDId,c.SoCode,c.SoSeq,c.InvCode as zInvCode,c.StartDemDate,c.OpSeq,c.Free1 FROM mom_order A (nolock) LEFT JOIN mom_orderdetail B (nolock)  ON A.MoId = B.MoId left join mom_moallocate c (nolock) on b.modid=c.modid where c.RequisitionFlag=1 and b.modid='" + thisMoDid + "' and (c.Qty-c.RequisitionQty)>0");

                    if (MomSet != null && MomSet.Tables.Count > 0 && MomSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow item in MomSet.Tables[0].Rows)
                        {
                            LinLiaoList.Add(new Materialappvouch
                            {
                                MDeptCode = item["MDeptCode"].ToString(),
                                MoDId = item["MoDId"].ToString(),
                                MoCode = item["MoCode"].ToString(),
                                SortSeq = item["SortSeq"].ToString(),
                                Qty = item["Qty"].ToString(),
                                cAppQty = item["cAppQty"].ToString(),
                                InvCode = item["InvCode"].ToString(),
                                AllocateId = item["AllocateId"].ToString(),
                                SoType = item["SoType"].ToString(),
                                SoDId = item["SoDId"].ToString(),
                                SoCode = item["SoCode"].ToString(),
                                SoSeq = item["SoSeq"].ToString(),
                                zInvCode = item["zInvCode"].ToString(),
                                dDueDate = item["StartDemDate"].ToString(),
                                iopseq = item["OpSeq"].ToString(),
                                Free1 = item["Free1"].ToString()
                            });
                        }

                    }
                }

                if (LinLiaoList.Count > 0)
                {

                    var grouprdappvouch = LinLiaoList.GroupBy(item => new
                    {
                        item.MDeptCode
                    });

                    foreach (var item in grouprdappvouch)
                    {
                        //获取单据号
                        string appcode = getappcode();

                        #region 获取领料申请单ID
                        //获取主表/子表ID
                        string sqlappID = string.Format(@"declare @p5 int
                                  set @p5=1000000002
                                  declare @p6 int
                                  set @p6=1000000002
                                  exec sp_GetId N'',N'" + ZTCode + "',N'mv'," + item.Count() + ",@p5 output,@p6 output,default select @p5, @p6");
                        //执行SQL
                        DataSet dssqappvouch = U8db.Ado.GetDataSetAll(sqlappID);
                        //主表ID
                        string appvouchid = dssqappvouch.Tables[0].Rows[0][0].ToString();
                        //子表id
                        int appvouchids = int.Parse(dssqappvouch.Tables[0].Rows[0][1].ToString()) - item.Count();
                        #endregion

                        //新增领料申请单
                        sqlList.Add(@"insert into materialappvouch ([ID],[dDate],[cCode],[cRdCode],[cDepCode],[cPersonCode],[cItem_class],[cItemCode],[cName],[cItemCName],[cHandler],[cMemo],[cCloser],[cMaker],[cDefine1],[cDefine2],[cDefine3],[cDefine4],[cDefine5],[cDefine6],[cDefine7],[cDefine8],[cDefine9],[cDefine10],[dVeriDate],[VT_ID],[cDefine11],[cDefine12],[cDefine13],[cDefine14],[cDefine15],[cDefine16],[ireturncount],[iverifystate],[iswfcontrolled],[cModifyPerson],[dModifyDate],[dnmaketime],[dnmodifytime],[dnverifytime],[iPrintCount],[cSource],[cvencode],[imquantity],[csysbarcode],[cCurrentAuditor],[cChanger]) values('" + appvouchid + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + appcode + "',Null," + isNull(item.Key.MDeptCode) + ",Null,Null,Null,Null,Null,'" + username + "',Null,Null,'" + username + "',Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,'" + DateTime.Now.ToString("yyyy-MM-dd") + "',30718,Null,Null,'装箱生成',Null,Null,Null,0,0,0,'',Null,getdate(),Null,getdate(),0,'生产订单',Null,Null,'||st64|" + appcode + "',Null,Null)");

                        int i = 1;
                        foreach (var itemapp in item)
                        {

                            appvouchids++;

                            //新增领料申请单子表
                            sqlList.Add(@"insert into materialappvouchs ([AutoID],[ID],[cInvCode],[iNum],[iQuantity],[cBatch],[cFree1],[cFree2],[dDueDate],[cBCloser],[fOutQuantity],[fOutNum],[dVDate],[cDefine22],[cDefine23],[cDefine24],[cDefine25],[cDefine26],[cDefine27],[cItem_class],[cItemCode],[cName],[cItemCName],[cFree3],[cFree4],[cFree5],[cFree6],[cFree7],[cFree8],[cFree9],[cFree10],[cAssUnit],[dMadeDate],[iMassDate],[cDefine28],[cDefine29],[cDefine30],[cDefine31],[cDefine32],[cDefine33],[cDefine34],[cDefine35],[cDefine36],[cDefine37],[cMassUnit],[cWhCode],[iinvexchrate],[iExpiratDateCalcu],[cExpirationdate],[dExpirationdate],[cBatchProperty1],[cBatchProperty2],[cBatchProperty3],[cBatchProperty4],[cBatchProperty5],[cBatchProperty6],[cBatchProperty7],[cBatchProperty8],[cBatchProperty9],[cBatchProperty10],[cbMemo],[irowno],[iMPoIds],[cMoLotCode],[cmworkcentercode],[cmocode],[imoseq],[iopseq],[copdesc],[iOMoDID],[iOMoMID],[comcode],[invcode],[cciqbookcode],[cservicecode],[iordertype],[iorderdid],[iordercode],[iorderseq],[isotype],[isodid],[csocode],[isoseq],[corufts],[crejectcode],[ipesodid],[ipesotype],[cpesocode],[ipesoseq],[cbsysbarcode],[ipickedquantity],[ipickednum],[cSourceMOCode],[iSourceMODetailsid],[cplanlotcode]) values('" + appvouchids + "','" + appvouchid + "','" + itemapp.zInvCode + "',Null," + itemapp.cAppQty + ",Null," + isNull(itemapp.Free1) + ",Null,'" + itemapp.dDueDate + "',Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,0,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,Null,'" + (i + 1) + "','" + itemapp.AllocateId + "',Null,NULL,'" + itemapp.MoCode + "'," + itemapp.SortSeq + ",'" + itemapp.iopseq + "',Null,Null,Null,Null,'" + itemapp.InvCode + "',Null,Null,0,0,Null,Null," + isNull(itemapp.SoType) + "," + isNull(itemapp.SoDId) + "," + isNull(itemapp.SoCode) + "," + isNull(itemapp.SoSeq) + ",convert(nvarchar,convert(money,@@DBTS),2),Null,'" + itemapp.AllocateId + "',7,'" + itemapp.MoCode + "'," + itemapp.SortSeq + ",'||st64|" + appcode + "|" + (i + 1) + "',Null,Null,Null,Null,Null)");


                            //修改申请数量
                            sqlList.Add("update mom_moallocate set RequisitionQty=RequisitionQty+" + itemapp.cAppQty + " where AllocateId='" + itemapp.AllocateId + "'");
                            i++;
                        }
                    }

                    try
                    {
                        U8db.Ado.BeginTran();

                        foreach (string sql in sqlList)
                        {
                            U8db.Ado.ExecuteCommand(sql);
                        }

                        U8db.Ado.CommitTran();
                        return "1";
                    }
                    catch (Exception ex)
                    {
                        U8db.Ado.RollbackTran();
                        return "领料申请失败" + ex.Message;
                    }
                }
                else
                {
                    return "2";
                }
            }
            catch (Exception ex)
            {
                return "领料申请异常：" + ex.Message;
            }
        }


        public string isNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Null";
            }
            else
            {
                return "'" + value + "'";
            }
        }

        /// <summary>
        /// 检查指定箱号中的产品是否全部已发货
        /// </summary>
        /// <param name="db">YYCAdmin数据库</param>
        /// <param name="boxNumber">箱号</param>
        /// <returns>如果所有产品已发货，返回 true；否则返回 false</returns>
        private bool IsBoxFullyShipped(dynamic db, string boxNumber)
        {
            string U8Code = config.GetSection("U8Configs").GetSection("U8Code").Value;
            // 查询箱子中是否存在未发货的产品
            string checkShippedSql = @"SELECT COUNT(*) FROM base_packageCardDetails PCD  WHERE PCD.boxNumber = @boxNumber  AND NOT EXISTS (SELECT 1  FROM [" + U8Code + @"].[dbo].DispatchLists B 
                INNER JOIN [" + U8Code + @"].[dbo].DispatchList A  ON A.DLID = B.DLID WHERE A.bReturnFlag = '0' AND B.cBatch = PCD.invID AND B.cInvCode = PCD.invCode)";

            int noShippedCount = db.Ado.GetInt(checkShippedSql, new { boxNumber });

            return noShippedCount == 0;
        }


    }
}
