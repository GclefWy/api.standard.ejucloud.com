using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.standard.ejucloud.com
{
    public class Common
    {
        /// <summary>
        /// 添加response头,永许ajax跨域调用
        /// </summary>
        public static void AddResponseHeader()
        {
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
        }

        //加密算法
        public static string ParamEncrypt(int senceid)
        {
            int encNum = int.Parse(string.Format("{0}{1}", senceid, System.DateTime.Now.Millisecond.ToString("0000")));

            return encNum.ToString("x");
        }

        public static int ParamDecrypt(string senceid)
        {
            int encNum = Convert.ToInt32(senceid, 16);

            return encNum / 10000;
        }
    }
}