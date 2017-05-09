using BKS_Sys.App.Filter;
using BKS_Sys.App.Models;
using BKS_Sys.App.Models.Base;
using BKS_Sys.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace BKS_Sys.App.ApiControllers
{
    [Description("用户相关")]
    public class UsersApiController : COApiControllerBase
    {
        Entities ent = new Entities();

        #region 用户登录
        [Description("用户登录")]
        [Route("api/users/login")]
        [HttpPost]
        public IHttpActionResult Login()
        {
            string LoginNumber = GetTrimString("LoginNumber");
            string Password = GetTrimString("Password");

            if (String.IsNullOrEmpty(LoginNumber) || String.IsNullOrEmpty(Password))
            {
                return Error("用户名和密码不能为空！");
            }
            else
            {
                UserInfo ui = ent.GetUserInfo(LoginNumber, Utility.Md532(Password));
                if (ui != null)
                {
                    string websiteurl = ConfigurationManager.AppSettings["WebsiteUrl"];
                    Users user = new Users();
                    user.Token = Utility.Convert2TrimString(ui.Token);
                    user.UserName = ui.UserName;
                    user.LoginNumber = ui.LoginNumber;


                    user.UserImg = websiteurl + ui.UserImg;
                    user.State = ui.State;
                    user.UserID = Utility.Convert2TrimString(ui.UserID);
                    user.AttSetType = ent.getUserAttSetType(user.UserID);



                    return Ok("登录成功", user);
                }
                else
                {
                    return Error("用户名或密码错误，请重新登录！");
                }
            }
        }
        #endregion

        #region 修改密码
        [Description("修改密码")]
        [Route("api/users/updpwd")]
        [HttpPost]
        public IHttpActionResult UpdPwd()
        {
            string sOldPwd = GetTrimString("OldPwd");
            string sNewPwd = GetTrimString("NewPwd");
            string token = GetToken();
            if (String.IsNullOrEmpty(sOldPwd) || String.IsNullOrEmpty(sNewPwd))
            {
                return Error("密码修改失败，请重试！");
            }
            else
            {
                UserInfo ui = ent.UpdPwd(Utility.Md532(sOldPwd), Utility.Md532(sNewPwd), token);
                if (ui != null)
                {
                    Users user = new Users();
                    user.Token = Utility.Convert2TrimString(ui.Token);
                    user.UserName = ui.UserName;
                    user.LoginNumber = ui.LoginNumber;
                    user.UserImg = ui.UserImg;
                    user.State = ui.State;
                    user.UserID = Utility.Convert2TrimString(ui.UserID);
                    user.AttSetType = ent.getUserAttSetType(user.UserID);
                    return Ok("修改成功", user);
                }
                else
                {
                    return Error("密码修改失败，请重试！");
                }
            }
        }
        #endregion

        #region 获取用户签到信息

        public IHttpActionResult UserSignInByEF()
        {
            string userID = GetUserID();
            if (String.IsNullOrEmpty(userID))
            {
                return Error("用户ID不能为空");
            }
            UserSignin us = null;
            UserSigninfo ui = null;
            for (int i = 0; i < 1000; i++)
            {
                int type = 0;

                type = ent.getUserAttSetType(userID);

                if (type == 0)
                {
                    us = ent.UserSignInByEF(userID);

                }
                else
                {
                    ui = ent.UserSignOuterInfoByEF(userID);

                }
            }
            if (us != null)
            {
                return Ok(us);
            }
            else
            {
                return Ok(ui);
            }



        }

        #region OldGetSignInfo
        [Description("获取了签到签退信息")]
        [Route("api/users/usersignin")]
        [HttpPost]
        public IHttpActionResult UserSignIn()
        {
            UserSignin us = null;
            UserSigninfo ui = null;

            string userID = GetUserID();
            if (String.IsNullOrEmpty(userID))
            {
                return Error("用户ID不能为空");
            }
            int type = 0;

            type = ent.getUserAttSetType(userID);


            if (type == 0)
            {
                us = ent.UserSignIn(userID);
                return Ok(us);
            }
            else
            {
                ui = ent.UserSignOuterInfo(userID);
                return Ok(ui);
            }
        }
        #endregion
        #endregion

        #region 获取部门列表
        [Description("获取部门列表")]
        [Route("api/users/deptlist")]
        [HttpPost]
        public IHttpActionResult DeptList()
        {
            return Ok(ent.GetDetpList());
        }
        #endregion

        #region 获取用户列表
        [Description("获取用户列表")]
        [Route("api/users/userlist")]
        [HttpPost]
        public IHttpActionResult UserList()
        {
            Int32 Page = GetPageIndexMin1();
            Int32 Size = GetPageSizeMin10();

            if (Page == Int32.MinValue || Size == Int32.MinValue)
            {
                return Error("请输入页码和页数！");
            }
            else
            {
                List<Users> List = ent.UserList(Page, Size);
                return Ok(List);
            }
        }
        #endregion

        #region 获取部门用户
        [Description("获取部门用户")]
        [Route("api/users/deptperson")]
        [HttpPost]
        public IHttpActionResult DeptPerson()
        {
            string DeptID = GetTrimString("DeptID");
            if (String.IsNullOrEmpty(DeptID))
            {
                return Error("请选择部门！");
            }
            else
            {
                List<Users> List = ent.DeptPerson(DeptID);
                return Ok(List);
            }
        }
        #endregion

        #region 上传用户图像
        [Description("上传用户图像")]
        [Route("api/users/uploadheadimg")]
        [HttpPost]
        public IHttpActionResult UploadHeadImg()
        {
            string token = GetToken();
            string headImg = GetTrimString("HeadImg");

            if (String.IsNullOrEmpty(token))
            {
                return Error("用户ID不能为空");
            }
            string us = ent.UplaodHeadImg(token, headImg);
            return Ok("", new { headImgPath = us });
        }
        #endregion

        #region 提交极光ID
        [Description("提交推送ID")]
        [Route("api/users/setjpushid")]
        [HttpPost]
        public IHttpActionResult SetJPushID()
        {
            string jID = GetTrimString("JPushID");
            string token = GetToken();
            bool b = ent.UpdateJPushID2UserInfo(jID, token);
            if (b)
            {
                return Ok("");
            }
            else
            {
                return Error("");
            }

        }
        #endregion

        #region 推送消息
        [Route("api/users/pushmsg")]
        [HttpGet]
        public IHttpActionResult PushMsg()
        {
            var entity = "";
            ent.NewJpush();
            if (entity != null)
            {
                return Ok("", entity);
            }
            else
            {
                return Error("");
            }

        }
        #endregion

        #region 读取用户有权限查看的人员列表
        [Description("读取用户有权限查看的人员列表")]
        [Route("api/users/loaduserlimitlist")]
        [HttpPost]
        public IHttpActionResult LoadUserLimitList()
        {
            string token = GetToken();
            string type = GetTrimString("Type");
            if (string.IsNullOrWhiteSpace(type))
            {
                type = "0";
            }


            List<LimitUser> luList = ent.getLimitUserInfoByUserID(token, type);
            return Ok("", luList);
        }
        #endregion

        #region 读取推送消息列表
        [Description("读取推送消息列表")]
        [Route("api/users/loadpushlist")]
        [HttpPost]
        public IHttpActionResult LoadPushList()
        {
            string token = GetToken();
            string userID = GetTrimString("UserID");
            string type = GetTrimString("Type");
            string startTime = GetTrimString("StartTime");
            string endTime = GetTrimString("EndTime");

            string pageIndex = GetTrimString("Page");

            string pageSize = GetTrimString("Size");

            int page = 1;
            int size = 10;

            if (!string.IsNullOrWhiteSpace(pageIndex))
            {
                page = int.Parse(pageIndex);
            }
            if (!string.IsNullOrWhiteSpace(pageSize))
            {
                size = int.Parse(pageSize);
            }



            if (string.IsNullOrWhiteSpace(type))
            {
                type = "0";
            }
            List<PushMessage> puList = ent.getPushMessageList(token, type, userID, startTime, endTime, page, size);
            return Ok("", puList);
        }
        #endregion

        #region 添加用户反馈信息
        [Description("添加用户反馈信息")]
        [Route("api/users/addfeedbackinfo")]
        [HttpPost]
        public IHttpActionResult AddFeedBackInfo()
        {
            string message = GetTrimString("Message");
            string token = GetToken();
            bool b = ent.AddFeedBack(message, token);
            if (b)
            {
                return Ok("");
            }
            else
            {
                return Error("");
            }
        }
        #endregion

        #region 获取用户定位列表
        [Description("获取用户定位列表")]
        [Route("api/users/getlocationlist")]
        [HttpPost]
        public IHttpActionResult GetLocationList()
        {
            string token = GetToken();
            string page = GetTrimString("Page");
            string size = GetTrimString("Size");
            string userID = GetTrimString("UserID");
            string startTime = GetTrimString("StartTime");
            string endTime = GetTrimString("EndTime");
            string address = GetTrimString("Address");
            string minStayTime = GetTrimString("MinStayTime");
            string isShow = GetTrimString("IsShow");
            if (string.IsNullOrWhiteSpace(isShow))
            {
                isShow ="0"; //显示全部
            }

            UserLocationList ulList = ent.GetUserLocationList(token, page, size, userID, startTime, endTime, address,minStayTime,isShow);
            return Ok("", ulList);
        }

        #endregion

        #region 注销登录
        [Description("注销登录")]
        [Route("api/users/exitsystem")]
        [HttpPost]
        public IHttpActionResult ExitSystem()
        {
            return Ok("");
        }
        #endregion


        #region 二维码登录
        [Description("二维码登录")]
        [Route("api/users/piclogin")]
        [HttpPost]
        public IHttpActionResult PicLogin()
        {
            string PicID = GetTrimString("PicID");
            string State = GetTrimString("State");
            string token = GetToken();

            //if (String.IsNullOrEmpty(DeptID))
            //{
            //    return Error("请选择部门！");
            //}
            //else
            //{
            //    List<Users> List = ent.DeptPerson(DeptID);
            //    return Ok(List);
            //}
            String WebUrlInfo = ConfigurationManager.AppSettings["apiUrl"] + "/?token=" + token + "&picid=" + PicID+"&state="+State;
            String Path = ConfigurationManager.AppSettings["website"];
            //Get请求
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Path);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.GetAsync(WebUrlInfo).ContinueWith((t) =>
            {
            }, TaskScheduler.FromCurrentSynchronizationContext());
            return Ok("");
        }
        #endregion
    }
}
