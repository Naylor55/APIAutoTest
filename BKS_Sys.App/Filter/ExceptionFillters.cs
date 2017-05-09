using BKS_Sys.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;

namespace BKS_Sys.App.Filter
{
    /// <summary>
    /// 自定义异常处理类
    /// </summary>
    public class ExceptionFillters : ExceptionFilterAttribute
    {
        /// <summary>
        /// 重写异常处理方法
        /// </summary>
        /// <param name="filterContext">上下文对象</param>
        public override async void OnException(HttpActionExecutedContext filterContext)
        {
            string bodyStr = "";
            bool bZip = false;
            if (filterContext.Request.Content.Headers.Contains("Content-Encoding"))
            {

                var ContentType = filterContext.Request.Content.Headers.GetValues("Content-Encoding").FirstOrDefault();
                if (!String.IsNullOrEmpty(ContentType) && ContentType.Trim() == "gzip")
                {
                    byte[] zip = await filterContext.Request.Content.ReadAsByteArrayAsync();
                    byte[] unZip = ZipWrapperUtils.Decompress(zip);
                    char[] _unZip = System.Text.Encoding.UTF8.GetChars(unZip);
                    bodyStr = new String(_unZip);
                    bZip = true;
                }
            }
            if (!bZip)
            {
                bodyStr = await filterContext.Request.Content.ReadAsStringAsync();
            }
            //string bodyStr = await filterContext.Request.Content.ReadAsStringAsync();
            String sMessage = filterContext.Exception.Message.ToString() + "，请求参数：" + bodyStr.Trim();
            Utility.LogHelper(filterContext.Request.RequestUri.ToString(), sMessage);
        }
    }

}