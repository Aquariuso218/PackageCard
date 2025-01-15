using Infrastructure.Attribute;
using SqlSugar.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Business.Model;
using ZR.Model.Business.Model.Dto;
using ZR.Service.IService;
using SqlSugar;
using ZR.Model.System;
using ZR.Repository;

namespace ZR.Service
{
    [AppService(ServiceType = typeof(IPackagingLabelService), ServiceLifetime = LifeTime.Transient)]
    public class PackingLabelService : BaseService<PackageCard>, IPackagingLabelService, IDynamicApi
    {

        private readonly ISqlSugarClient db;

        public PackingLabelService()
        {
            db = DbScoped.SugarScope.GetConnectionScope(0);
        }

        /// <summary>
        /// 产品装箱
        /// </summary>
        /// <param name="simulatedData">产品列表</param>
        /// <returns>信息</returns>
        public string createPackage(List<SimulatedData> simulatedData)
        {
            if (simulatedData == null || simulatedData.Count == 0)
            {
                return "传入数据为空,请传入有效数据。";
            }
            try
            {
                // 查询未装满的最后一张包装卡
                var lastUnfilledCard = db.Queryable<PackageCard>()
                                .Where(pc => SqlFunc.Subqueryable<PackageCardDetails>()
                                                     .Where(d => d.boxNumber == pc.boxNumber)
                                                     .Count() < 4)
                                .OrderBy(pc => pc.createdTime, OrderByType.Desc)
                                .First();

                int countPerBox = 4; // 每个包装卡的数量上限
                string currentboxNumber = lastUnfilledCard?.boxNumber ?? null;
                int currentBoxCount = lastUnfilledCard?.quantity ?? 0;

                List<PackageCardDetails> detailsList = new List<PackageCardDetails>();

                db.Ado.BeginTran();
                foreach (var item in simulatedData)
                {
                    if (currentboxNumber == null || currentBoxCount >= countPerBox)
                    {
                        // 创建主表
                        currentboxNumber = Guid.NewGuid().ToString("N"); // 使用GUID生成箱号
                        currentBoxCount = 0;

                        var packageCard = new PackageCard
                        {
                            boxNumber = currentboxNumber,
                            invCode = item.invCode,
                            invName = "产品名称", // 假设由其他方法获取
                            pictureCode = "图号", // 假设由其他方法获取
                            quantity = 0, // 初始化为0，稍后更新
                            createBy = "当前用户", // 从上下文获取创建人
                            createdTime = DateTime.Now
                        };

                        db.Insertable(packageCard).ExecuteCommand();
                    }

                    var packageCardDetails = new PackageCardDetails
                    {
                        boxNumber = currentboxNumber,
                        flowCard = item.flowCard,
                        quantity = item.quantity,
                        createdBy = "当前用户",
                        createdTime = DateTime.Now
                    };
                    //添加待写入子表数据
                    detailsList.Add(packageCardDetails);
                    currentBoxCount++;
                    //更新PackageCard表的quantity
                    db.Updateable<PackageCard>()
                        .SetColumns(pc => new PackageCard { quantity = currentBoxCount })
                        .Where(pc => pc.boxNumber == currentboxNumber)
                        .ExecuteCommand();
                }

                // 添加子表明细
                db.Insertable(detailsList).ExecuteCommand();
                // 提交事务
                db.Ado.CommitTran();
                return "生成成功";
            }
            catch (Exception ex)
            {
                // 出现异常时回滚事务
                db.Ado.RollbackTran();
                return $"发生错误: {ex.Message}";
            }
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        public PagedInfo<SimulatedData> getInvList(PagerInfo pager)
        {
            // 查询所有数据
            var query = db.Queryable<SimulatedData>();

            // 直接进行分页处理并返回
            return query.ToPage(pager);
        }

        /// <summary>
        /// 装箱列表
        /// </summary>
        /// <param name="package"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public PagedInfo<PackageCard> getPCList(PackageCard package, PagerInfo pager)
        {

            // 创建表达式构建器，根据 boxNumber 和 invCode 进行筛选
            var exp = Expressionable.Create<PackageCard>();
            // 动态添加条件
            if (!string.IsNullOrEmpty(package.boxNumber))
            {
                exp.And(p => p.boxNumber == package.boxNumber);
            }
            if (!string.IsNullOrEmpty(package.invCode))
            {
                exp.And(p => p.invCode == package.invCode);
            }

            // 查询主表数据（PackageCard）并应用筛选条件
            var query = db.Queryable<PackageCard>()
                .Where(exp.ToExpression());

            // 获取分页数据
            var packageCards = query.ToPage(pager);

            // 遍历主表数据，查询并填充子表数据
            foreach (var card in packageCards.Result)
            {
                // 查询对应的子表数据
                var packageDetails = db.Queryable<PackageCardDetails>()
                    .Where(d => d.boxNumber == card.boxNumber) // 通过boxNumber与主表关联
                    .ToList();

                // 将查询到的子表数据添加到主表数据中
                card.Details = packageDetails; // 这里不需要进行ToArray()，直接赋值为List即可
            }

            return packageCards;
        }

    }
}
