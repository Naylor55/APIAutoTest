using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BKS_Sys.App.Models
{

    #region 提交定位
    public class LocationModel
    {
        public List<LocationModelDetail> Data { get; set; }
    }

    public class LocationModelDetail
    {
        public string LocationDate { get; set; }

        public string LocationJSON { get; set; }

        public int Type { get; set; }

        public string Message { get; set; }

        /// <summary>
        /// 强制定位ＩＤ
        /// </summary>
        public string ComLocusID { get; set; }

        /// <summary>
        /// 定位来源：0-移动端，1-PC端
        /// </summary>
        public int MethodSource { get; set; }


        public string ListenerPushID { get; set; }
    }


    public class LocusDetailInfo
    {
        public Decimal Latitude { get; set; }

        public Decimal Longitude { get; set; }

        public string Address { get; set; }

        public string Provider { get; set; }

        public long Time { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 县
        /// </summary>
        public string District { get; set; }
    }
    #endregion


    #region 定位配置信息
    public class LocationConfig
    {
        public int Interval { get; set; }

        public string LocationStartTime { get; set; }

        public string LocationEndTime { get; set; }


        public int Railings { get; set; }

        public string Weeks { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }
    }
    #endregion
}