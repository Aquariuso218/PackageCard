using Microsoft.AspNetCore.Mvc;
using SqlSugar.IOC;
using SqlSugar;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Business.Model;
using ZR.Model.Business.Model.Dto;
using ZR.Model.Sunset.Model.Dto;
using ZR.Service;
using ZR.Service.IService;
using ZR.Service.Sunset;
using JinianNet.JNTemplate.Nodes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Cors;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using ZR.Model.Sunset.Model;

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
        public IRdrecord10Service Rdrecord10Service;
        public IRdrecord32Service Rdrecord32Service;

        public PackingLabelController(IPackagingLabelService packingLabelService, IRdrecord10Service rdrecord10Service, IRdrecord32Service rdrecord32Service)
        {
            PackagingLabelService = packingLabelService;
            Rdrecord10Service = rdrecord10Service;
            Rdrecord32Service = rdrecord32Service;
        }

        /// <summary>
        /// 删除装箱主表
        /// </summary>
        /// <param name="boxNumber"></param>
        /// <returns></returns>
        [HttpDelete("DeletePC/{boxNumber}")]
        //[Log(Title = "装箱管理", BusinessType = BusinessType.DELETE)]
        //[ActionPermissionFilter(Permission = "system:user:remove")]
        public IActionResult DeletePackageCard(string boxNumber = "")
        {
            if (boxNumber == "") { return ToResponse(ApiResult.Error(101, "请求参数错误")); };
            MyApiResult result = PackagingLabelService.DeletePackageCard(boxNumber);

            if (result.Code != 1)
            {
                return ToResponse(ResultCode.FAIL, result.Msg);
            }

            return ToResponse(ApiResult.Success(result.Msg));
        }

        /// <summary>
        /// 删除装箱子表明细
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeletePCD/{id}")]
        [Log(Title = "装箱管理", BusinessType = BusinessType.DELETE)]
        //[ActionPermissionFilter(Permission = "system:user:remove")]
        public IActionResult DeletePCDetails(int id = 0)
        {
            if (id <= 0) { return ToResponse(ApiResult.Error(101, "请求参数错误")); }

            MyApiResult result = PackagingLabelService.DeletePCDetails(id);
            if (result.Code != 1)
            {
                return ToResponse(ResultCode.FAIL, result.Msg);
            }

            return ToResponse(ApiResult.Success(result.Msg));
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("getInvList")]
        public IActionResult getInvList([FromQuery] SimulatedData simulatedData, PagerInfo pager)
        {
            return SUCCESS(PackagingLabelService.getInvList(simulatedData, pager));
        }

        /// <summary>
        /// 装箱列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("getPCList")]
        public IActionResult getPCList([FromQuery] PackageCard package, PagerInfo pager)
        {
            return SUCCESS(PackagingLabelService.getPCList(package, pager));
        }

        /// <summary>
        /// 普通装箱
        /// </summary>
        /// <returns></returns>
        [HttpPost("create")]
        public IActionResult createPackage([FromBody] PackageQuery Query)
        {
            return SUCCESS(PackagingLabelService.createPackage(Query));
        }

        /// <summary>
        /// 合箱装箱
        /// </summary>
        /// <returns></returns>
        [HttpPost("mergeCreate")]
        public IActionResult mergeCreatePackage([FromBody] PackageQuery Query)
        {
            return SUCCESS(PackagingLabelService.mergeCreatePackage(Query));
        }

        /// <summary>
        /// 特殊装箱
        /// </summary>
        /// <returns></returns>
        [HttpPost("createSpcPackage")]
        public IActionResult createSpcPackage([FromBody] PackageQuery1 query)
        {
            return SUCCESS(PackagingLabelService.createSpcPackage(query));
        }

        /// <summary>
        /// 扫描查询箱子详情列表
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        [HttpGet("stockInList")]
        [AllowAnonymous]
        public IActionResult getBoxDetails([FromQuery] string barCode)
        {
            return ToResponse(Rdrecord10Service.getBoxDetails(barCode));
        }

        /// <summary>
        /// 查询待入库产品列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("getStockInList")]
        [AllowAnonymous]
        public IActionResult getStockInList([FromQuery] string? startTime = null, string? endTime = null, string? invCode = null, string? keyWord = null)
        {
            return ToResponse(Rdrecord10Service.getStockInList(startTime, endTime, invCode, keyWord));
        }

        /// <summary>
        /// 查询待出库列表
        /// </summary>
        /// <param name="cDLCode"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        [HttpGet("stockOutList")]
        [AllowAnonymous]
        public IActionResult getStockOutList([FromQuery] string? cDLCode = null, string? startDate = null, string? endDate = null, string? keyWord = null)
        {
            return ToResponse(Rdrecord10Service.getStockOutList(cDLCode, startDate, endDate, keyWord));
        }

        /// <summary>
        /// 查询仓库列表
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        [HttpGet("wareHouseList")]
        [AllowAnonymous]
        //string? text = null：明确 text 是可选的，默认值为 null
        public IActionResult wareHouseList([FromQuery] PagerInfo pager, string? text = null)
        {
            return SUCCESS(Rdrecord10Service.wareHouseList(text, pager));
        }

        /// <summary>
        /// 查询客户列表
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        [HttpGet("customerList")]
        [AllowAnonymous]
        public IActionResult customerList([FromQuery] PagerInfo pager, string? text = null)
        {
            return SUCCESS(Rdrecord10Service.customerList(text, pager));
        }

        /// <summary>
        /// 产品入库
        /// </summary>
        /// <param name="salesInvoiceQuery"></param>
        /// <returns></returns>
        [HttpPost("stockIn")]
        [AllowAnonymous]
        public IActionResult stockIn([FromBody] SalesInvoiceQuery salesInvoiceQuery)
        {
            return ToResponse(Rdrecord10Service.stockIn(salesInvoiceQuery));
        }

        /// <summary>
        /// 销售出库
        /// </summary>
        /// <returns></returns>
        [HttpPost("salesInvoice")]
        [AllowAnonymous]
        public IActionResult salesInvoice([FromBody] DispatchList dispatchList)
        {
            return ToResponse(Rdrecord32Service.stockOut(dispatchList));
        }


        /// <summary>
        /// 修改装箱数据
        /// </summary>
        /// <returns></returns>
        [HttpPost("update")]
        [AllowAnonymous]
        public IActionResult UpdatePC([FromBody] PackageCard card)
        {
            return ToResponse(PackagingLabelService.UpdatePC(card));
        }

    }
}
