//=====================================================================================
// All Rights Reserved , Copyright © Learun 2013
//=====================================================================================
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Utilities.Base.Security
{
    /// <summary>
    /// MD5加密帮助类
    /// 版本：2.0
    /// <author>
    ///		<name>lvqiao</name>
    ///		<date>2021-07-12</date>
    /// </author>
    /// </summary>
    public class Md5Helper
    {
        #region "MD5加密"
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str">加密字符</param>
        /// <param name="code">加密位数16/32</param>
        /// <returns></returns>
        public static string Md5Hash(string str, int code)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(str));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            if (code == 16)
            {
                return sBuilder.ToString().Substring(8, 16);

            }
            return sBuilder.ToString();
        }
        #endregion
    }
}
