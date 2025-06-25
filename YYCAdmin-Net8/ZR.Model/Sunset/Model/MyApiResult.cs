using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model
{
    public class MyApiResult
    {
        public int Code { get; set; } // 状态码（1成功，0失败，-1异常）
        public string Msg { get; set; } // 详细信息
    }
}
