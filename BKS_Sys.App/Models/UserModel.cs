using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BKS_Sys.App.Models
{  

    #region 用户登录
    public class UserLogin
    {
        /// <summary>
        /// 用户登录名/手机号
        /// </summary>
        public String LoginNumber { get; set; }
        /// <summary>
        /// 密码(非加密)
        /// </summary>
        public String Password { get; set; }
    }
    #endregion

    #region 用户信息
    public class Users
    {
        /// <summary>
        /// 用户ID，GUID
        /// </summary>
        public String UserID { get; set; }

        /// <summary>
        /// 令牌，GUID
        /// </summary>
        public String Token { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// 登录账户，手机号
        /// </summary>
        public String LoginNumber { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public String UserImg { get; set; }

        /// <summary>
        /// 用户状态,0-启用，1-禁用
        /// </summary>
        public Boolean State { get; set; }

        /// <summary>
        /// 用户考勤类型
        /// </summary>
        public int AttSetType { get; set; }
    }
    #endregion

    #region 签到信息
    public class UserSignin
    {
        public String amSignInTime { get; set; }
        public String amSignOutTime { get; set; }


        public String pmSignInTime { get; set; }
        public String pmSignOutTime { get; set; }
    }

    public class UserSigninfo
    {
        public String amSignInTime { get; set; }
        public String pmSignOutTime { get; set; }
    }
    #endregion

    #region 部门列表
    public class DetpList
    {
        public String DeptID { get; set; }
        public String DeptName { get; set; }
        public String ParentID { get; set; }
        public String SortNo { get; set; }


    }
    #endregion

    #region 权限用户
    public class LimitUser
    {
        public Guid? UserID { get; set; }

        public string UserName { get; set; }
        public string UserImg { get; set; }

        public string LoginNumber { get; set; }
    }
    #endregion


    #region 推送消息内容
        public class PushMessage
        {
            public Guid MsgID { get; set; }

            public Guid UserID { get; set; }

            public string UserName { get; set; }

            public string AlertDate { get; set; }

            public string Message { get; set; }

            public int Type { get; set; }
        }

        #endregion


    #region 用户定位信息
    public class UserLocationInfo
    {
        public Guid? UserID { get; set; }

        public string UserName { get; set; }

        public string LocationDate { get; set; }

        public string LocationJSON { get; set; }

        public int? Type { get; set; }

        public int? SpanSecond { get; set; }

        public int IsShow { get; set; }
    }



    public class UserLocationList
    {
        public bool Hasmore { get; set; }

        public List<UserLocationInfo> List { get; set; }
    }
    #endregion


    #region 
    public class RolesLimit2Users
    {
        public Guid RoleID { get; set; }

        public Guid UserID { get; set; }

        public string UserName { get; set; }
    }
    #endregion


    #region 和用户相关的配置信息
    public class UserRelationConfig
    {
        public int Interval { get; set; }

        public string LocationStartTime { get; set; }

        public string LocationEndTime { get; set; }


        public int Railings { get; set; }

        public string Weeks { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }
        
        /// <summary>
        /// 是否是领导
        /// </summary>
        public bool IsLeader { get; set; }
        /// <summary>
        /// 上午签到开始时间
        /// </summary>
        public string amLoginInStartTime { get; set; }
        /// <summary>
        /// 上午签到结束时间
        /// </summary>
        public string amLoginInEndTime { get; set; }

        /// <summary>
        /// 上午签退开始时间
        /// </summary>
        public string amLoginOutStartTime { get; set; }
        /// <summary>
        /// 上午签退结束时间
        /// </summary>
        public string amLoginOutEndTime { get; set; }
        /// <summary>
        /// 下午签到开始时间
        /// </summary>
        public string pmLoginInStartTime { get; set; }

        /// <summary>
        /// 下午签到结束时间
        /// </summary>
        public string pmLoginInEndTime { get; set; }

        /// <summary>
        /// 下午签退开始时间
        /// </summary>
        public string pmLoginOutStartTime { get; set; }

        /// <summary>
        /// 下午签退结束世家你
        /// </summary>
        public string pmLoginOutEndTime { get; set; }
    }
    #endregion
}