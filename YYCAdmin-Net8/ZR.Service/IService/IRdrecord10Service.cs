using Infrastructure.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Business.Model;
using ZR.Model.Sunset;
using ZR.Model.Sunset.Model;
using ZR.Model.Sunset.Model.Dto;

namespace ZR.Service.IService
{
    public interface IRdrecord10Service
    {
        /// <summary>
        /// 扫码查询入库列表
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        public ApiResult getBoxDetails(string barCode);

        /// <summary>
        /// 入库产品列表
        /// </summary>
        /// <returns></returns>
        public ApiResult getStockInList(string startTime, string endTime, string invCode, string keyWord);

        /// <summary>
        /// 查询待出库列表
        /// </summary>
        /// <returns></returns>
        public ApiResult getStockOutList(string cDLCode, string startDate, string endDate, string keyWord);

        /// <summary>
        /// 仓库列表
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public PagedInfo<Warehouse> wareHouseList(string text, PagerInfo pager);

        /// <summary>
        /// 客户列表
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public PagedInfo<Customer> customerList(string text, PagerInfo pager);

        /// <summary>
        /// 产品入库
        /// </summary>
        /// <param name="salesInvoiceQuery"></param>
        /// <returns></returns>
        public ApiResult stockIn(SalesInvoiceQuery salesInvoiceQuery);

        /// <summary>
        /// 获取入库单据号
        /// </summary>
        /// <returns></returns>
        public string Rd10code();
    }
}
