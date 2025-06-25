using Infrastructure.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Business.Model;
using ZR.Model.Business.Model.Dto;
using ZR.Model.Sunset.Model;
using ZR.Model.Sunset.Model.Dto;

namespace ZR.Service.IService
{
    /// <summary>
    /// 包装标识卡
    /// </summary>
    public interface IPackagingLabelService : IBaseService<PackageCard>  
    {
        /// <summary>
        /// 产品列表
        /// </summary>
        /// <returns></returns>
        PagedInfo<SimulatedData> getInvList(SimulatedData simulatedData,PagerInfo pager);

        /// <summary>
        /// 装箱列表
        /// </summary>
        /// <param name="package"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        PagedInfo<PackageCard> getPCList(PackageCard package, PagerInfo pager);

        /// <summary>
        /// 普通装箱
        /// </summary>
        /// <param name="PackageQuery"></param>
        /// <returns></returns>
        string createPackage(PackageQuery query);

        /// <summary>
        /// 合箱装箱
        /// </summary>
        /// <param name="PackageQuery"></param>
        /// <returns></returns>
        string mergeCreatePackage(PackageQuery query);

        /// <summary>
        /// 特殊装箱
        /// </summary>
        /// <param name="PackageQuery1"></param>
        /// <returns></returns>
        string createSpcPackage(PackageQuery1 query);

        /// <summary>
        /// UPDATE
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public int UpdatePC(PackageCard card);

        public MyApiResult DeletePackageCard(string boxNumber);

        public MyApiResult DeletePCDetails(int id);

    }
}
