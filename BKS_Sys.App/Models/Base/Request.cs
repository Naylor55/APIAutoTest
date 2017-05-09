using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BKS_Sys.App.Models.Base
{

        #region Request

        public class RequestBody<T>
        {
            public RequestHead RequestHead { get; set; }
            public T body { get; set; }
        }
        public class RequestHead
        {
            /// <summary>
            /// 移动版本号
            /// </summary>
            public string Version { get; set; }
            /// <summary>
            /// 令牌
            /// </summary>
            public string Token { get; set; }
            /// <summary>
            /// 宽
            /// </summary>
            public string Width { get; set; }
            /// <summary>
            /// 高
            /// </summary>
            public string Height { get; set; }
            /// <summary>
            /// 平台，android，IOS
            /// </summary>
            public string Platform { get; set; }
            /// <summary>
            /// 卡标识1
            /// </summary>
            public String Imsi1 { get; set; }
            /// <summary>
            /// 卡标识2
            /// </summary>
            public String Imsi2 { get; set; }
            /// <summary>
            /// 手机串号
            /// </summary>
            public String Imei { get; set; }
        }
        #endregion
}