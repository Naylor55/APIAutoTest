using BKS_Sys.App.Filter;
using BKS_Sys.App.Models;
using BKS_Sys.App.Models.Base;
using BKS_Sys.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace BKS_Sys.App.ApiControllers
{
    [Description("定位信息")]
    public class LocationApiController : COApiControllerBase
    {
        Entities ent = new Entities();


        #region 提交定位
        [Description("提交定位信息")]
        [Route("api/location/locationsubmit")]
        [HttpPost]
        public IHttpActionResult LocationSubmit()
        {
            //string userID = GetUserID();
            string token = GetToken();
            string data = GetTrimString("Data");
            //反序列化
            JavaScriptSerializer jss = new JavaScriptSerializer();
            List<LocationModelDetail> model = jss.Deserialize<List<LocationModelDetail>>(data);

            bool result = ent.LocationSubmit(model, token, data);

            if (result)
            {
                return Ok("");
            }
            else
            {
                return Error("");
            }
        }
        #endregion

        #region 获取定位配置信息
        [Description("获取定位配置信息")]
        [Route("api/location/locationconfig")]
        [HttpPost]
        public IHttpActionResult LocationConfig()
        {
            string token = GetToken();
            UserRelationConfig lcEntity=ent.LoadLocationConfig(token);
            if (lcEntity != null)
            {
                return Ok(lcEntity);
            }
            else
            {
                return Error("");
            }
            
        }
        #endregion

        #region 发送定位请求
        [Description("发送定位请求")]
        [Route("api/location/getcurrentaddress")]
        [HttpPost]
        public IHttpActionResult GetCurrentAddress()
        {
            string userid=GetTrimString("UserID");
            string newid = GetTrimString("NewID");
            string token= GetToken();
            int type = 0; //移动端请求


            ent.SendMessage4GetCurrentAddress(userid, newid, token,type);
            return Ok("");
        }
        #endregion


        #region 发送定位请求
        [Description("发送定位请求")]
        [Route("api/location/getcurrentaddressforpc")]
        [HttpGet]
        public IHttpActionResult GetCurrentAddressForPc()
        {
            string userid = GetTrimString("userid");
            string newid = GetTrimString("newid");
            string token ="";
            int type = 1;//PC端请求

            ent.SendMessage4GetCurrentAddress(userid, newid,token,type);
            return Ok("");
        }
        #endregion


        #region 获取指定的定位信息
        [Description("获取指定的定位信息")]
        [Route("api/location/getfocusaddressinfo")]
        [HttpPost]
        public IHttpActionResult GetFocusAddressInfo()
        {
            string newid = GetTrimString("LocusID");

            UserLocationInfo ldEntity = ent.GetCurrentLocusDetail(newid);

            return Ok("",ldEntity);
        }
        #endregion
    }
}