using BKS_Sys.App.Models;
using BKS_Sys.App.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Script.Serialization;

namespace BKS_Sys.App.Filter
{
    //ConvertOperateControllerBase
    public class COApiControllerBase : SLSApiControllerBase
    {
        #region Hash转换String
        /// <summary>
        /// Hash转换String
        /// </summary>
        public String GetTrimString(String parameter)
        {
            return GetNoTrimString(parameter).Trim();
        }
        public String GetNoTrimString(String parameter)
        {
            if (String.IsNullOrEmpty(parameter) || Context[parameter]==null)
            {
                return "";
            }

            return Convert.ToString(Context[parameter]);
        }
        #endregion

        #region Hash转换Int32
        /// <summary>
        /// Hash转换Int32
        /// </summary>
        public Int32 GetInt32(String parameter)
        {
            if (String.IsNullOrEmpty(parameter) || Context[parameter] == null)
            {
                return Int16.MinValue;
            }
            return Convert.ToInt32(Context[parameter]);
        }
        #endregion


        #region 取最小页的数量
        public Int32 GetPageIndexMin1(String parameter)
        {
            Int32 Page = GetInt32(parameter);
            if (Page == Int16.MinValue)
            {
                return Int16.MinValue;
            }
            return Math.Max(1, Page);
        }
        public Int32 GetPageSizeMin10(String parameter)
        {
            Int32 Size = GetInt32(parameter);
            if (Size == Int16.MinValue)
            {
                return Int16.MinValue;
            }
            return Size == 0 ? 10 : Size;
        }
        public Int32 GetPageIndexMin1()
        {
            return GetPageIndexMin1("Page");
        }
        public Int32 GetPageSizeMin10()
        {
            return GetPageSizeMin10("Size");
        }
        #endregion

        #region 获取用户Token
        /// <summary>
        /// 获取用户Token
        /// </summary>
        /// <returns></returns>
        public String GetToken()
        {
            if (Context["RequestHead"] != null)
            {
                String Token = ((RequestHead)Context["RequestHead"]).Token;
                return Token;
            }
            return "";
        }
        #endregion

        #region 获取用户Token
        /// <summary>
        /// 获取用户Token
        /// </summary>
        /// <returns></returns>
        public String GetUserID()
        {
            if (Context["UserID"] != null)
            {
                String UserID = Context["UserID"].ToString().Trim();
                return UserID;
            }
            return "";
        }
        #endregion
    }
}