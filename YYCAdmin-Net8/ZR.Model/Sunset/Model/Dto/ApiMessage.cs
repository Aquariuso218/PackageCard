using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sunset.Model.Dto
{
    /// <summary>
    /// 通用的API消息类，用于封装接口返回的数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ApiMessage<T>
    {
        /// <summary>
        /// 返回成功标识 true:成功 false:失败
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// 返回编码 1:成功 其他:失败
        /// </summary>
        public int statecode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 返回结果
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]//如果为NULL则不转JSON
        public T subdata { get; set; }
        /// <summary>
        /// 总条数
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]//如果为NULL则不转JSON
        public int? total { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]//如果为NULL则不转JSON
        public int? page { get; set; }
        /// <summary>
        /// 当前页条数
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]//如果为NULL则不转JSON
        public int? pagesize { get; set; }
    }
}