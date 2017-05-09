using BKS_Sys.App.Filter;
using BKS_Sys.App.Models;
using BKS_Sys.Model.SelfDefinedModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BKS_Sys.App.ApiControllers
{
    [Description("考勤相关")]
    public class AttendanceApiController : COApiControllerBase
    {
        Entities ent = new Entities();
        #region 历史考勤信息
        [Description("历史考勤列表")]
        [Route("api/attendance/getattendancelist")]
        [HttpPost]
        public IHttpActionResult GetAttendanceList()
        {
            Int32 page = this.GetPageIndexMin1();
            Int32 size = this.GetPageSizeMin10();
            String uid = this.GetTrimString("Uid");
            String startTime = this.GetTrimString("StartTime");
            String endTime = this.GetTrimString("EndTime");

            if (string.IsNullOrEmpty(uid))
            {
                return Error("请输入UID！");
            }
            if (page == Int32.MinValue || size == Int32.MinValue)
            {
                return Error("请输入页码和页数！");
            }
            if (string.IsNullOrEmpty(startTime)||string.IsNullOrEmpty(endTime))
            {
                return Error("请输入完整的时间段！");
            }

            List<HistoryAttence> List = ent.GetHistoryAttence(page, size, uid, startTime, endTime);
            return Ok(List);
        }
        #endregion
    }
}
