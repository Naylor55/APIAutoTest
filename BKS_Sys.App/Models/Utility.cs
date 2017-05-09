using BKS_Sys.App.Models.Base;
using BKS_Sys.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Script.Serialization;

namespace BKS_Sys.App.Models
{
    public static class Utility
    {
        #region Md5
        public static String CreateMD5(int ID)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] userIDs = md5.ComputeHash(Encoding.Default.GetBytes(ID.ToString()), 0, ID.ToString().Length);
            StringBuilder sb = new StringBuilder();
            foreach (var item in userIDs)
            {
                sb.Append(item);
            }
            return sb.ToString();
        }

        public static String CreateMD5(string pwd)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] userIDs = md5.ComputeHash(Encoding.Default.GetBytes(pwd), 0, pwd.Length);
            StringBuilder sb = new StringBuilder();
            foreach (var item in userIDs)
            {
                sb.Append(item);
            }
            return sb.ToString();
        }

        public static string Md532(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 

                pwd = pwd + s[i].ToString("X");

            }
            return pwd;
        }

        #endregion

        //#region 对象转换为Json
        ///// <summary>
        ///// 对象转换为Json 
        ///// </summary>
        ///// <param name="jsonObject"></param>
        ///// <returns></returns>
        //public static String ToJson(object jsonObject)
        //{
        //    JavaScriptSerializer jss = new JavaScriptSerializer();
        //    return jss.Serialize(jsonObject);

        //}
        //#endregion

        #region 写入异常日志
        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <param name="msg">异常消息</param>
        public static void LogHelper(String request, String msg)
        {
            Entities ent = new Entities();
            request = request.Replace(" ", "");
            msg = msg.Replace(" ", "");
            ent.WriteExceptionLog(request, msg);
        }
        #endregion

        #region 插入手机串号
        /// <summary>
        /// 插入手机串号
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="IMSI1"></param>
        /// <param name="IMSI2"></param>
        /// <param name="IMEI1"></param>
        public static void UpdateIMIS(Guid UserID, String IMSI1, String IMSI2, String IMEI1)
        {
            Entities ent = new Entities();
            ent.UpdateIMIS(UserID, IMSI1, IMSI2, IMEI1);
        }
        #endregion

        /// <summary>
        /// 验证Token的合法性
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Boolean VerifyToken(String token)
        {
            Boolean bTemp = true;
            Entities ent = new Entities();
            UserInfo ui = ent.VerifyToken(token);
            if (ui != null)
            {
                bTemp = false;
            }
            return bTemp;
        }

        #region 获取用户信息
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="iTep"></param>
        /// <returns></returns>
        public static UserInfo GetUserInfo(String token, String userName, String password, Boolean iTep)
        {
            Entities ent = new Entities();
            UserInfo ui = ent.GetUserInfo(token, userName, password, iTep);
            return ui;
        }
        #endregion

        #region 写入操作日志
        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="LogUser"></param>
        /// <param name="LogDate"></param>
        /// <param name="LogIP"></param>
        /// <param name="LogMenu"></param>
        /// <param name="LogContent"></param>
        public static void WriteOperationLog(Guid UserID,String LogUser, DateTime LogDate, String LogIP, String LogMenu, String LogContent, int iType)
        {
            Entities ent = new Entities();
            ent.WriteOperationLog(UserID,LogUser, LogDate, LogIP, LogMenu, LogContent, iType);
        }
        #endregion

        public static String  GetDescriptionAttribute(string controller,string method)
        {
            String sDescription = "";
            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
          
            Type type = assembly.GetType(controller);     //命名空间名 + 类名
            MethodInfo[] mi = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (MethodInfo m in mi)
            {
                if (m.Name == method)
                {
                    if (m.ReturnType.Name == "IHttpActionResult")
                    {
                        object[] t1 = m.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                        if (t1 != null && t1.Length > 0)
                        {
                            System.ComponentModel.DescriptionAttribute d = (System.ComponentModel.DescriptionAttribute)t1[0];
                            sDescription = d.Description;
                        }
                    }
                    break;
                }
            }
            return sDescription;
        }


        #region 请求失败
        /**
        /// <summary>
        /// 请求失败
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static Result ErrorResult(String msg)
        {
            Result result = new Result();
            result.Code = "201";
            result.Msg = msg;
            return result;
        }
        #endregion

        #region 请求成功
        /// <summary>
        /// 请求成功
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static Result SuccessResult(String msg)
        {
            Result result = new Result();
            result.Code = "200";
            result.Msg = msg;
            return result;
        }
         * */
        #endregion

        #region 判断是否为Null，为True则返回Empty
        /// <summary>
        /// 判断是否为Null，为True则返回Empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String VerifyIsNull(String value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return "";
            }
            else
            {
                return value;
            }
        }
        #endregion

        #region GUID转换成字符串
        /// <summary>
        /// GUID转换成字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String Convert2TrimString(Object value)
        {
            if (value != null)
            {
                return value.ToString().Trim();
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region 对象转换成Boolean
        /// <summary>
        /// 对象转换成Boolean
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Boolean ConvertBoolean(Object value)
        {
            if (value != null)
            {
                return Convert.ToBoolean(value.ToString().Trim());
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}