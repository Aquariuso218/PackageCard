using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZR.Admin.WebApi.Filters;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Cors;

namespace ZR.Admin.WebApi.Controllers.Sunset
{

    [Route("v1/dingding")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class DingDingController : BaseController
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _corpId = "dingrbwj01hzc0obyoag";
        private readonly string _corpSecret = "CQtzXgqL1Sg92JNGqznBAi0nNawpd4DteB8VTTMFEM7iAF5VY_SgSryIP8hreE7c"; 

        [HttpPost("getUserInfo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserInfo([FromBody] CodeRequest request)
        {
            if (string.IsNullOrEmpty(request?.Code))
            {
                return Ok(new { mescode = -1, errmsg = "code 不能为空" });
            }

            try
            {
                // 第一步：获取 access_token
                var tokenUrl = $"https://oapi.dingtalk.com/gettoken?appkey={_corpId}&appsecret={_corpSecret}";
                var tokenResponse = await _httpClient.GetAsync(tokenUrl);
                tokenResponse.EnsureSuccessStatusCode();

                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenResult = JsonConvert.DeserializeObject<DingTalkResponse>(tokenContent);

                if (tokenResult.Errcode != 0)
                {
                    return Ok(new { mescode = tokenResult.Errcode, errmsg = tokenResult.Errmsg });
                }

                string accessToken = tokenResult.AccessToken;

                // 第二步：使用 access_token 和 code 获取用户信息
                var userInfoUrl = $"https://oapi.dingtalk.com/user/getuserinfo?access_token={accessToken}&code={request.Code}";
                var userInfoResponse = await _httpClient.GetAsync(userInfoUrl);
                userInfoResponse.EnsureSuccessStatusCode();

                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                var userInfoResult = JsonConvert.DeserializeObject<UserInfoResponse>(userInfoContent);

                if (userInfoResult.Errcode != 0)
                {
                    return Ok(new { mescode = userInfoResult.Errcode, errmsg = userInfoResult.Errmsg });
                }

                return Ok(new
                {
                    mescode = 0,
                    mesdata = new
                    {
                        userId = userInfoResult.Userid,
                        name = userInfoResult.Name,
                        avatar = userInfoResult.Avatar ?? ""
                    }
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { errcode = 500, errmsg = "网络请求失败: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errcode = 500, errmsg = "服务器错误: " + ex.Message });
            }
        }
    }

    public class CodeRequest
    {
        public string Code { get; set; }
    }

    public class DingTalkResponse
    {
        [JsonProperty("errcode")]
        public int Errcode { get; set; }

        [JsonProperty("errmsg")]
        public string Errmsg { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

    public class UserInfoResponse
    {
        [JsonProperty("errcode")]
        public int Errcode { get; set; }

        [JsonProperty("errmsg")]
        public string Errmsg { get; set; }

        [JsonProperty("userid")]
        public string Userid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }
}