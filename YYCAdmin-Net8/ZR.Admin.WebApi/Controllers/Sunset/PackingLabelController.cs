using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Business.Model;
using ZR.Service;
using ZR.Service.IService;

namespace ZR.Admin.WebApi.Controllers.Business
{
    /// <summary>
    /// 包装标识卡
    /// </summary>
    [Verify]
    [Route("v1/packinglable")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class PackingLabelController : BaseController
    {
        public IPackagingLabelService PackagingLabelService;

        public PackingLabelController(IPackagingLabelService packingLabelService) {
            PackagingLabelService = packingLabelService;
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("getInvList")]
        public IActionResult getInvList([FromQuery] PagerInfo pager) {
           return SUCCESS(PackagingLabelService.getInvList(pager));
        }

        /// <summary>
        /// 装箱
        /// </summary>
        /// <returns></returns>
        [HttpPost("create")]        
        public IActionResult createPackage([FromBody] List<SimulatedData> simulatedData) {
            return SUCCESS(PackagingLabelService.createPackage(simulatedData));
        }

        /// <summary>
        /// 装箱列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("getPCList")]
        public IActionResult getPCList([FromQuery] PackageCard package, PagerInfo pager)
        {
            return SUCCESS(PackagingLabelService.getPCList(package,pager));
        }

    }
}
