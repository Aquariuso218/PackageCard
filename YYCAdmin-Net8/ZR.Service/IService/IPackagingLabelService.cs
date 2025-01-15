using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Business.Model;
using ZR.Model.Business.Model.Dto;

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
        PagedInfo<SimulatedData> getInvList(PagerInfo pager);


        /// <summary>
        /// 产品装箱
        /// </summary>
        /// <param name="simulatedData"></param>
        /// <returns></returns>
        string createPackage(List<SimulatedData> simulatedData);

        PagedInfo<PackageCard> getPCList(PackageCard package, PagerInfo pager);


    }
}
