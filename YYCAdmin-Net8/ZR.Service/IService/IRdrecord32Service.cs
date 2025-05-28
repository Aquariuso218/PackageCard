using Infrastructure.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Business.Model;
using ZR.Model.Sunset.Model.Dto;

namespace ZR.Service.IService
{
    public interface IRdrecord32Service
    {
        public ApiResult stockOut(DispatchList dispatchList);
    }
}
