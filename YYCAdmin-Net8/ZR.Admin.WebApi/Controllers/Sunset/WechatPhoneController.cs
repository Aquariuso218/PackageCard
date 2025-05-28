using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ZR.Admin.WebApi.Controllers.Sunset
{
    [Route("v1/WeChat")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class WechatPhoneController : BaseController
    {
        [HttpPost("decryptPhone")]
        public IActionResult DecryptPhone([FromBody] DecryptRequest request)
        {
            if (string.IsNullOrEmpty(request.EncryptedData) || string.IsNullOrEmpty(request.Iv) || string.IsNullOrEmpty(request.code))
            {
                return SUCCESS(new { success = false, message = "缺少参数" });
            }

            try
            {
                string appId = "wx6fe21ba5f97f7bff";
                string secret = "d3147e22c01141f37f120d95aa065f0a";

                // 修正 URL 拼接问题
                string url = $"https://api.weixin.qq.com/sns/jscode2session?appid={appId}&secret={secret}&js_code={request.code}&grant_type=authorization_code";
                string jsonResponse = HttpGet(url);

                // 解析 JSON 获取 session_key
                JObject jsonObject = JObject.Parse(jsonResponse);
                string sessionKey = jsonObject["session_key"]?.ToString();
                string errMsg = jsonObject["errmsg"]?.ToString();

                Console.WriteLine($"获取到的 SessionKey: {sessionKey}");
                Console.WriteLine($"WeChat API 错误信息: {errMsg}");

                // sessionKey 为空，返回错误
                if (string.IsNullOrEmpty(sessionKey))
                {
                    return SUCCESS(new { success = false, message = "获取 SessionKey 失败", error = jsonResponse });
                }

                // 修正 Base64 解码错误
                string decryptedJson = DecryptWeChatData(request.EncryptedData, sessionKey, request.Iv);
                var jsonData = JObject.Parse(decryptedJson);

                return SUCCESS(
                    new
                    {
                        success = true,
                        phoneNumber = jsonData["phoneNumber"]?.ToString()
                    });
            }
            catch (Exception ex)
            {
                return SUCCESS(new { success = false, message = "解密失败", error = ex.Message });
            }
        }

        // 修正 Base64 解析失败的情况
        private static string DecryptWeChatData(string encryptedData, string sessionKey, string iv)
        {
            try
            {
                byte[] encryptedDataBytes = Convert.FromBase64String(encryptedData);
                byte[] sessionKeyBytes = Convert.FromBase64String(sessionKey);
                byte[] ivBytes = Convert.FromBase64String(iv);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = sessionKeyBytes;
                    aes.IV = ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    using (MemoryStream ms = new MemoryStream(encryptedDataBytes))
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Base64 解码错误: " + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("解密失败: " + ex.Message);
                return null;
            }
        }


        public static string HttpGet(string url)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            try
            {
                //获取内容
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                stream.Close();
            }
            return result;
        }
    }

    public class DecryptRequest
    {
        public string EncryptedData { get; set; }
        public string code { get; set; }
        public string Iv { get; set; }
    }
}
