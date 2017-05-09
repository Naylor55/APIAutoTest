using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BKS_Sys.App.Models.Base
{
    #region Response
    public class ResponseBody<T>
    {
        public Result Result { get; set; }
        public T Body { get; set; }
    }
    public class Result
    {
        /// <summary>
        /// 信息编号
        /// </summary>
        public String Code { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public String Msg { get; set; }
    }
    #endregion
}