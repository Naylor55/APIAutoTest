using BKS_Sys.
App.Filter;
using BKS_Sys.Model;
using BKS_Sys.Model.SelfDefinedModel;
using BKS_Sys.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace BKS_Sys.App.Models
{
    public class Entities
    {
        private BookoesDBEntities db = new BookoesDBEntities();

        #region 用户登录
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="sLoginNumber"></param>
        /// <param name="sPassword"></param>
        /// <returns></returns>
        public UserInfo GetUserInfo(String sLoginNumber, String sPassword)
        {
            UserInfo ui = db.UserInfo.FirstOrDefault(D => D.LoginNumber == sLoginNumber && D.Password == sPassword
                && D.State == false && D.IsDelete == false);
            return ui;
        }


        public int getUserAttSetType(string userID)
        {
            Guid guser = Guid.Parse(userID);
            AttendanceSet asEntity = db.AttendanceSet.FirstOrDefault(x => x.UserID == guser && x.IsDelete == false);
            if (asEntity != null)
            {
                if (asEntity.UserTyep == true)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }

        }
        #endregion

        #region 验证Token合法性
        /// <summary>
        /// 验证Token合法性
        /// </summary>
        /// <param name="sLoginNumber"></param>
        /// <param name="sPassword"></param>
        /// <returns></returns>
        public UserInfo VerifyToken(String token)
        {
            Guid Token = new Guid(token);
            UserInfo ui = db.UserInfo.FirstOrDefault(D => D.Token == Token
                && D.State == false && D.IsDelete == false);
            return ui;
        }

        internal UserLocationInfo GetCurrentLocusDetail(string newid)
        {
            Guid detailId = Guid.Parse(newid);
            LocusDetail ldEntity = db.LocusDetail.Where(x => x.DetailID == detailId).FirstOrDefault();


            List<UserLocationInfo> ullList = (from ld in db.LocusDetail.Where(x => x.UserID == detailId)
                              join u in db.UserInfo.Where(x=>x.IsDelete==false) on ld.UserID equals u.UserID
                              select new UserLocationInfo
                              {
                                  UserID = ld.UserID,
                                  UserName = u.UserName,
                                  LocationDate = ((DateTime)ld.LocusDate).ToString("yyyy-MM-dd HH:mm:ss"),
                                  LocationJSON = ld.LocationJson,
                                  SpanSecond = ld.Interval,
                                  Type = ld.LocationSource
                              }).ToList();
            UserLocationInfo uliEntity = ullList.FirstOrDefault();

            TimeSpan ts = DateTime.Now - DateTime.Parse(uliEntity.LocationDate);

            uliEntity.SpanSecond = (int)ts.TotalSeconds;
            return uliEntity;
        
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
        public void UpdateIMIS(Guid UserID, String IMSI1, String IMSI2, String IMEI1)
        {
            String sql = string.Format(@"update UserInfo set IMSI1 = '{0}',IMSI2='{1}',IMEI1 = '{2}' " +
                         "where UserID = '{3}'", IMSI1, IMSI2, IMEI1, UserID);
            DBHelper.Instance().ExcuteSql(sql);
        }
        #endregion

        #region
        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="iTep"></param>
        /// <returns></returns>
        public UserInfo GetUserInfo(String token, String userName, String password, Boolean iTep)
        {
            UserInfo ui = new UserInfo();
            if (iTep)
            {
                Guid Token = new Guid(token);
                ui = db.UserInfo.FirstOrDefault(D => D.Token == Token
                    && D.State == false && D.IsDelete == false);
            }
            else
            {
                ui = db.UserInfo.FirstOrDefault(D => D.LoginNumber == userName && D.Password == password
                    && D.State == false && D.IsDelete == false);
            }
            return ui;
        }
        #endregion

        #region 写入异常日志
        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="request">请求消息</param>
        /// <param name="msg">异常消息</param>
        public void WriteExceptionLog(String request, String msg)
        {
            String sql = "INSERT INTO ExceptionLog(Request,ErrorContent,Platform) values('" + request + "',N'" + msg + "','1')";
            DBHelper.Instance().ExcuteSql(sql);
        }
        #endregion

        #region 写入操作日志
        /// <summary>
        /// 写入操作日志
        /// </summary>
        /// <param name="LogUser"></param>
        /// <param name="LogDate"></param>
        /// <param name="LogIP"></param>
        /// <param name="LogMenu"></param>
        /// <param name="LogContent"></param>
        public void WriteOperationLog(Guid UserID,String LogUser, DateTime LogDate, String LogIP, String LogMenu, String LogContent,int iType)
        {
            try
            {
                String sql = String.Format(@"INSERT INTO Logs(UserID,LogUser,LogDate,LogIP,LogMenu,LogContent,Platform,OperateType) values('{7}','{0}','{1}','{2}','{3}',N'{4}',{5},{6})"
                , LogUser, LogDate, LogIP, LogMenu, LogContent, 1, iType, UserID);
                DBHelper.Instance().ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                String sql = String.Format(@"INSERT INTO Logs(UserID,LogUser,LogDate,LogIP,LogMenu,LogContent,Platform,OperateType) values('{7}','{0}','{1}','{2}','{3}',N'{4}',{5},{6})"
                , LogUser, LogDate, LogIP, LogMenu, LogContent, 1, iType, UserID);
                WriteLog(ex, sql);
            }
            
        }


        public static void WriteLog(Exception ex, string sqltext)
        {
            //如果日志文件为空，则默认在Debug目录下新建 YYYY-mm-dd_Log.log文件
            string filePath = ConfigurationManager.AppSettings["picUrl"];
            //filePath = Server.MapPath(apiSiteUrl);
            if (!System.IO.File.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string newFileName = filePath + "/" +DateTime.Now.Ticks + ".txt"; ;
            
            //把异常信息输出到文件
            StreamWriter sw = new StreamWriter(newFileName, true);
            sw.WriteLine("当前时间：" + DateTime.Now.ToString());
            sw.WriteLine("异常信息：" + ex.Message);
            sw.WriteLine("异常对象：" + ex.Source);
            sw.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
            sw.WriteLine("触发方法：" + ex.TargetSite);
            sw.WriteLine("执行语句：" + sqltext);
            sw.WriteLine();
            sw.Close();
        }
        #endregion

        #region 修改密码
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="request">请求消息</param>
        /// <param name="msg">异常消息</param>
        public UserInfo UpdPwd(String sOldPwd, String sNewPwd, String token)
        {
            Guid Token = new Guid(token);
            UserInfo ui = db.UserInfo.FirstOrDefault(D => D.Password == sOldPwd && D.Token == Token);
            if (ui != null)
            {
                ui.Password = sNewPwd;
                db.SaveChanges();
            }
            return ui;
        }
        #endregion

        #region 用户签到
        /// <summary>
        /// 用户签到
        /// </summary>
        /// <param name="request">请求消息</param>
        /// <param name="msg">异常消息</param>
        public UserSignin UserSignIn(String userID)
        {
            Guid UserID = new Guid(userID);

            String sql = "select UserID,SignTime,AttSetType,TypeName from Attendance where IsDelete = 0 and UserID='"+userID+"' and CONVERT(varchar(12) , SignTime, 111 ) = CONVERT(varchar(12) , getdate(), 111 )";
            DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
            UserSignin us = new UserSignin();
            if (dt == null || dt.Rows.Count <= 0)
            {
                us.amSignInTime = "";
                us.amSignOutTime = "";
                us.pmSignInTime = "";
                us.pmSignOutTime = "";
                return us;
            }

            var signInList = dt.Select("AttSetType=0 and TypeName='签到'");
           
            if (signInList.Length != 0)
            {
                us.amSignInTime = ((DateTime)signInList[0].ItemArray[1]).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                us.amSignInTime = "";
            }
            var signOutList = dt.Select("AttSetType=0 and TypeName='签退'");
            if (signOutList.Length != 0)
            {
                us.amSignOutTime = ((DateTime)signOutList[0].ItemArray[1]).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                us.amSignOutTime = "";
            }
            var pmsignInList = dt.Select("AttSetType=1 and TypeName='签到'");
            if (pmsignInList.Length != 0)
            {
                us.pmSignInTime = ((DateTime)pmsignInList[0].ItemArray[1]).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                us.pmSignInTime = "";
            }
            var pmsignOutList = dt.Select("AttSetType=1 and TypeName='签退'");
            if (pmsignOutList.Length != 0)
            {
                us.pmSignOutTime = ((DateTime)pmsignOutList[0].ItemArray[1]).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                us.pmSignOutTime = "";
            }
            return us;

        }

        public UserSigninfo UserSignOuterInfo(string userID)
        {
            Guid UserID = new Guid(userID);

            String sql = "select UserID,SignTime,AttSetType,TypeName from Attendance where IsDelete = 0 and UserID='" + userID + "' and CONVERT(varchar(12) , SignTime, 111 ) = CONVERT(varchar(12) , getdate(), 111 )";
            UserSigninfo ui = new UserSigninfo();
            DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
            if (dt == null)
            {
                ui.amSignInTime = "";
                ui.pmSignOutTime = "";
                return ui;
            }

            var signInList = dt.Select("AttSetType=0 and TypeName='签到'");
            if (signInList.Length != 0)
            {
                ui.amSignInTime = ((DateTime)signInList[0].ItemArray[1]).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                ui.amSignInTime = "";
            }

            var pmsignOutList = dt.Select("AttSetType=1 and TypeName='签退'");
            if (pmsignOutList.Length != 0)
            {
                ui.pmSignOutTime = ((DateTime)pmsignOutList[0].ItemArray[1]).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                ui.pmSignOutTime = "";
            }
            return ui;
        }


        /// <summary>
        /// 获取内勤签到信息
        /// </summary>
        /// <param name="request">请求消息</param>
        /// <param name="msg">异常消息</param>
        internal UserSignin UserSignInByEF(string userID)
        {
            Guid usID=Guid.Parse(userID);
            DateTime nowDate=DateTime.Now.Date;
            DateTime tomorDate=DateTime.Now.AddDays(1).Date;
            List<Attendance> atList=db.Attendance.Where(x => x.UserID == usID && x.SignTime > nowDate && x.SignTime < tomorDate && x.IsDelete == false).ToList();

            UserSignin us = new UserSignin();
            if (atList.Count == 0)
            {
             
                us.amSignInTime = "";
                us.amSignOutTime = "";
                us.pmSignInTime = "";
                us.pmSignOutTime = "";
                return us;
            }

            //上午签到
            Attendance amLogintEntity=atList.Where(x => x.AttSetType == false && x.TypeName == "签到").FirstOrDefault();
            if (amLogintEntity != null)
            {
                us.amSignInTime = ((DateTime)amLogintEntity.SignTime).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                us.amSignInTime = "";
            }
            Attendance amLogoutEntity = atList.Where(x => x.AttSetType == false && x.TypeName == "签退").FirstOrDefault();
            if (amLogoutEntity != null)
            {
                us.amSignOutTime = ((DateTime)amLogoutEntity.SignTime).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                us.amSignInTime = "";
            }


            //下午签到
            Attendance pmLoginEntiy = atList.Where(x => x.AttSetType == true && x.TypeName == "签到").FirstOrDefault();
            if (pmLoginEntiy != null)
            {
                us.pmSignInTime =((DateTime)pmLoginEntiy.SignTime).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                us.pmSignInTime = "";
            }
            Attendance pmLogoutEntity = atList.Where(x => x.AttSetType == true && x.TypeName == "签退").FirstOrDefault();
            if (pmLogoutEntity != null)
            {
                us.pmSignOutTime = ((DateTime)pmLogoutEntity.SignTime).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                us.pmSignOutTime = "";
            }

            return us;
        }


        internal UserSigninfo UserSignOuterInfoByEF(string userID)
        {
            Guid UserID = new Guid(userID);
            DateTime nowDate = DateTime.Now.Date;
            DateTime tomorDate = DateTime.Now.AddDays(1).Date;
            List<Attendance> atList = db.Attendance.Where(x => x.UserID == UserID && x.SignTime > nowDate && x.SignTime < tomorDate && x.IsDelete == false).ToList();
            UserSigninfo ui = new UserSigninfo();
            if (atList.Count==0)
            {
                ui.amSignInTime = "";
                ui.pmSignOutTime = "";
                return ui;
            }

            Attendance amLoginEntity = atList.Where(x => x.AttSetType == false && x.TypeName == "签到").FirstOrDefault();
            if (amLoginEntity != null)
            {
                ui.amSignInTime = ((DateTime)amLoginEntity.SignTime).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                ui.amSignInTime = "";
            }

            Attendance pmLoginEntity = atList.Where(x => x.AttSetType == true && x.TypeName == "签退").FirstOrDefault();
            if (pmLoginEntity != null)
            {
                ui.pmSignOutTime = ((DateTime)pmLoginEntity.SignTime).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                ui.pmSignOutTime = "";
            }
            return ui;
        }
        #region 添加用户反馈信息
        internal bool AddFeedBack(string message, string token)
        {
            Guid Token = new Guid(token);
            UserInfo ui = db.UserInfo.FirstOrDefault(D => D.Token == Token
                && D.State == false && D.IsDelete == false);


            string sql = string.Format("insert into FeedBack values (newid(),'{0}','{1}','{2}',0)", ui.UserID, message, DateTime.Now);

            bool b = DBHelper.Instance().ExcuteSql(sql);

            return b;
        }

        #endregion



        #region 获取用户定位列表
        public UserLocationList GetUserLocationList(string token, string page, string size, string userID, string startTime, string endTime, string address, string minStayTime,string isShow)
        {
            UserLocationList ullEntity = new UserLocationList();
            ullEntity.List = new List<UserLocationInfo>();
            UserInfo ui = new UserInfo();
            Guid UserID = new Guid();

            List<UserInfo> uList = db.UserInfo.Where(x => x.IsDelete == false && x.State == false).ToList();
            //没有传入UserID,默认显示发送请求用户的定位列表信息
            if (string.IsNullOrWhiteSpace(userID))
            {
                Guid Token = new Guid(token);
                ui = uList.FirstOrDefault(D => D.Token == Token
                    && D.State == false && D.IsDelete == false);
                UserID = ui.UserID;
            }
            else
            {
                UserID = Guid.Parse(userID);
            }
            List<LocusDetail> edList = new List<LocusDetail>();
            //读取用户的所有定位信息集合
            List<LocusDetail> ldList = new List<LocusDetail>();
            IEnumerable<LocusDetail> leList = db.LocusDetail.Where(x => x.IsDelete == false && x.UserID == UserID);
            DateTime Start = DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                Start = DateTime.Parse(startTime);
            }

            leList = leList.Where(x => x.LocusDate >= Start);
             DateTime End=DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                End = DateTime.Parse(endTime);
            }

            leList = leList.Where(x => x.LocusDate < End.AddDays(1));
            if (!string.IsNullOrWhiteSpace(address))
            {
                leList = leList.Where(x => x.Location.Contains(address));
            }
            if (!string.IsNullOrWhiteSpace(minStayTime))
            {
                int min=int.Parse(minStayTime);
                leList = leList.Where(x => x.Interval > min);
            }
            //总记录数
            ldList = leList.ToList();
            int total = ldList.Count;
            //页码
            int pageSize = int.Parse(size);
            //页号
            int pageIndex = int.Parse(page);

            if (total > (pageIndex * pageSize))
            {
                ullEntity.Hasmore = true;
            }
            else
            {
                ullEntity.Hasmore = false;
            }
            //显示数据
            if (isShow == "0")
            {
                //如果是查看今天的轨迹
                if (End.Date == DateTime.Now.Date)
                {
                    if (DateTime.Now.Minute > 15)
                    {
                        //查询上个小时以前所有处理过的数据和当前小时的所有数据
                        edList.AddRange(ldList.Where(x => x.LocusDate.Value.Date == DateTime.Now.Date && x.IsDisplay>0 && x.LocusDate.Value.Hour < DateTime.Now.Hour));
                        List<LocusDetail> currentHourList=ldList.Where(x => x.LocusDate.Value.Date == DateTime.Now.Date && x.LocusDate.Value.Hour >= DateTime.Now.Hour).ToList();
                        List<LocusDetail> tempList = new List<LocusDetail>();
                        foreach (var item in currentHourList)
                        {
                            item.IsDisplay = 1;
                            tempList.Add(item);
                        }
                        edList.AddRange(tempList.Where(x => 1 == 1));
                        //edList.AddRange(ldList.Where(x => x.LocusDate.Value.Date > DateTime.Now.Date && x.LocusDate.Value.Hour >= DateTime.Now.Hour));

                    }
                    else
                    {
                        //查询上上个小时以前所有处理过的数据和之后的所有数据

                        edList.AddRange(ldList.Where(x => x.LocusDate.Value.Date > DateTime.Now.Date && x.IsDisplay>0 && x.LocusDate.Value.Hour < DateTime.Now.AddHours(-1).Hour));
                        List<LocusDetail> currentHourList = ldList.Where(x => x.LocusDate.Value.Date == DateTime.Now.Date && x.LocusDate.Value.Hour >= DateTime.Now.AddHours(-1).Hour).ToList();

                        List<LocusDetail> tempList = new List<LocusDetail>();
                        foreach (var item in currentHourList)
                        {
                            item.IsDisplay = 1;
                            tempList.Add(item);
                        }
                        edList.AddRange(tempList.Where(x => 1 == 1));

                    }
                }
                else
                {
                    edList.AddRange(ldList.Where(x=>x.IsDisplay>0));
                }
            }
            else
            {
                edList.AddRange(ldList.Where(x=>x.IsDisplay>=1));
            }

            ullEntity.List = (from ld in edList
                              join u in uList on ld.UserID equals u.UserID
                              select new UserLocationInfo
                              {
                                  UserID = ld.UserID,
                                  UserName = u.UserName,
                                  LocationDate = ((DateTime)ld.LocusDate).ToString("yyyy-MM-dd HH:mm:ss"),
                                  LocationJSON = ld.LocationJson,
                                  SpanSecond = ld.Interval,
                                  Type = ld.LocationSource,
                                  IsShow=ld.IsDisplay
                              }).OrderByDescending(x => x.LocationDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            UserLocationInfo uliEntity = ullEntity.List.FirstOrDefault();

            if (uliEntity != null)
            {
                TimeSpan ts = DateTime.Now - DateTime.Parse(uliEntity.LocationDate);

                ullEntity.List.FirstOrDefault().SpanSecond = (int)ts.TotalSeconds;
            }
            else
            {
                ullEntity.List = new List<UserLocationInfo>();
            }
            
            return ullEntity;
        }
        #endregion

        #endregion

        #region 获取部门全部信息
        /// <summary>
        /// 获取部门全部信息
        /// </summary>
        /// <returns></returns>
        public List<DetpList> GetDetpList()
        {
            List<Department> dept = db.Department.Where(D => D.IsDelete == false).OrderBy(D=>D.SortNo).ToList();
            List<DetpList> List = new List<DetpList>();
            if (dept != null && dept.Count > 0)
                foreach (Department d in dept)
                {
                    DetpList L = new DetpList();
                    L.DeptID = d.DeptID.ToString();
                    L.DeptName = d.DeptName;
                    if (d.ParentID == null)
                    {
                        L.ParentID = "";
                    }
                    else
                    {
                        L.ParentID = d.ParentID.Value.ToString();
                    }
                    if (d.SortNo == null)
                    {
                        L.SortNo = "";
                    }
                    else
                    {
                        L.SortNo = d.SortNo.Value.ToString();
                    }
                    List.Add(L);
                }
            return List;
        }
        #endregion

        #region 获取部门人员列表
        /// <summary>
        /// 获取部门人员列表
        /// </summary>
        /// <param name="sLoginNumber"></param>
        /// <param name="sPassword"></param>
        /// <returns></returns>
        public List<Users> DeptPerson(String sDeptID)
        {
            List<Users> list = new List<Users>();
            String sql = "select a.UserID,a.UserName,a.LoginNumber,a.UserImg from [UserInfo] as a " +
                        "inner join  [UserRelation] as b " +
                        "on a.UserID = b.UserID " +
                        "where b.DeptPostID =  '" + sDeptID + "' and b.[Type] =0 " +
                        "and b.IsDelete = 0 and a.IsDelete =0 and a.[State] =0";
            DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
            string websiteurl = ConfigurationManager.AppSettings["WebsiteUrl"];
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Users u = new Users();
                    u.UserID = Utility.Convert2TrimString(dt.Rows[i]["UserID"]);
                    u.UserName = Utility.Convert2TrimString(dt.Rows[i]["UserName"]);
                    u.LoginNumber = Utility.Convert2TrimString(dt.Rows[i]["LoginNumber"]);
                    u.UserImg = websiteurl + dt.Rows[i]["UserImg"].ToString();
                    list.Add(u);
                }
            }
            return list;
        }
        #endregion

        #region 人员列表
        /// <summary>
        /// 人员列表
        /// </summary>
        /// <param name="sLoginNumber"></param>
        /// <param name="sPassword"></param>
        /// <returns></returns>
        public List<Users> UserList(Int32 page, Int32 size)
        {
            List<Users> userList = new List<Users>();
            String sql = "SELECT TOP " + size + " UserID,Token,UserName,LoginNumber,UserImg,[State] FROM " +
                        "(SELECT ROW_NUMBER() OVER (ORDER BY SortNo) AS RowNumber,* FROM UserInfo WHERE [State]=0 AND IsDelete=0 " +
                        ") A WHERE RowNumber > " + size + "*(" + page + "-1)";
            DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
            string websiteurl = ConfigurationManager.AppSettings["WebsiteUrl"];
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Users u = new Users();
                    u.UserID = Utility.Convert2TrimString(dt.Rows[i]["UserID"]);
                    u.UserName = dt.Rows[i]["UserName"].ToString();
                    u.LoginNumber = Utility.Convert2TrimString(dt.Rows[i]["LoginNumber"]);
                    //u.Token = Utility.Convert2TrimString(dt.Rows[i]["LoginNumber"]);
                    u.UserImg =websiteurl+ dt.Rows[i]["UserImg"].ToString();
                    u.State = Utility.ConvertBoolean(dt.Rows[i]["State"]);
                    userList.Add(u);
                }
            }
            return userList;
        }
        #endregion

        #region 获取历史考勤信息

        /// <summary>
        /// 人员历史考勤信息
        /// </summary>
        /// <param name="sLoginNumber"></param>
        /// <param name="sPassword"></param>
        /// <returns></returns>
        /// page, size, uid, startTime, endTime
        public List<HistoryAttence> GetHistoryAttence(Int32 page, Int32 size, String uid, String startTime, String endTime)
        {
            List<HistoryAttence> htAttList = new List<HistoryAttence>();

            #region 20160519 刘凯明注释
            //String sql = string.Format(@"SELECT TOP {0} AttSetType,SignTime,Location,[State],[Message] FROM (select ROW_NUMBER() OVER (ORDER BY a.CreateDate) AS RowNumber, AttSetType,SignTime,Location,[State],[Message] from Attendance as a,AttendanceSet as ats where a.AttSetID=ats.AttSetID and a.IsDelete=0 and ats.IsDelete=0 and a.UserID='{1}' and SignTime>='{2}' and SignTime<='{3}') A WHERE RowNumber > 5*({4}-1)", size, uid, startTime, endTime, page);
            #endregion

            string checkSql = string.Format("select * from attendanceset where userid='{0}'", uid);

            DataTable checkTable = DBHelper.Instance().GetDataTableBySql(checkSql);

            if (checkTable != null && checkTable.Rows.Count > 0)
            {
                DateTime start = DateTime.Parse(startTime).Date;
                DateTime end = DateTime.Parse(endTime).Date;

                string sql = string.Format(@"
                        SELECT  ROW_NUMBER() over (order by SignTime) as RowNumber, AttSetType,SignTime,Location,
                        [State],[Message],TypeName FROM attendance as att where att.IsDelete=0 and att.UserID='{1}' and SignTime>='{2}' 
                        and SignTime<='{3}'", size, uid, startTime, end.AddDays(1), page);
                DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
                for (DateTime time = end; time >= start; )
                {
                    DataRow[] newdt = dt.Select(string.Format("SignTime<'{0}' and SignTime>='{1}'", time.AddDays(1), time));
                    HistoryAttence htAtt = new HistoryAttence();
                    htAtt.am1 = new a();
                    htAtt.am2 = new a();
                    htAtt.pm1 = new a();
                    htAtt.pm2 = new a();

                    for (int i = 0; i < newdt.Length; i++)
                    {

                        //上午签到
                        if (newdt[i]["AttSetType"].ToString().ToLower() == "false" && newdt[i]["TypeName"].ToString() == "签到")
                        {
                            htAtt.am1.time = ((DateTime)newdt[i]["SignTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                            htAtt.am1.position = newdt[i]["Location"].ToString();
                            htAtt.am1.state = newdt[i]["State"].ToString();
                            htAtt.am1.msg = newdt[i]["Message"].ToString();

                        }//上午签退
                        else if (newdt[i]["AttSetType"].ToString().ToLower() == "false" && newdt[i]["TypeName"].ToString() == "签退")
                        {
                            htAtt.am2.time = ((DateTime)newdt[i]["SignTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                            htAtt.am2.position = newdt[i]["Location"].ToString();
                            htAtt.am2.state = newdt[i]["State"].ToString();
                            htAtt.am2.msg = newdt[i]["Message"].ToString();
                        }
                        //下午签到
                        else if (newdt[i]["AttSetType"].ToString().ToLower() == "true" && newdt[i]["TypeName"].ToString() == "签到")
                        {
                            htAtt.pm1.time = ((DateTime)newdt[i]["SignTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                            htAtt.pm1.position = newdt[i]["Location"].ToString();
                            htAtt.pm1.state = newdt[i]["State"].ToString();
                            htAtt.pm1.msg = newdt[i]["Message"].ToString();
                        }
                        //下午签退
                        else
                        {
                            htAtt.pm2.time = ((DateTime)newdt[i]["SignTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                            htAtt.pm2.position = newdt[i]["Location"].ToString();
                            htAtt.pm2.state = newdt[i]["State"].ToString();
                            htAtt.pm2.msg = newdt[i]["Message"].ToString();
                        }
                    }
                    if (newdt.Length > 0)
                    {
                        htAttList.Add(htAtt);
                    }
                    time = time.AddDays(-1);
                }
            }
            else
            {
                DateTime start = DateTime.Parse(startTime).Date;
                DateTime end = DateTime.Parse(endTime).Date;

                string sql = string.Format(@"select ROW_NUMBER() over (order by SignTime) as RowNumber,SignTime,Location,[State],[Message] from attendance where UserID='{1}' and SignTime>='{2}' and SignTime<='{3}'", size, uid, startTime, end.AddDays(1), page);
                DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
                if (dt != null && dt.Rows.Count > 0)
                {

                    for (DateTime time = end; time >= start; )
                    {
                        DataRow[] newdt = dt.Select(string.Format("SignTime<'{0}' and SignTime>='{1}'", time.AddDays(1), time));
                        if (newdt.Length > 0)
                        {
                            if (newdt.Length == 1)
                            {
                                HistoryAttence htAtt = new HistoryAttence();
                                htAtt.am1 = new a();
                                htAtt.am1.time = ((DateTime)newdt[0]["SignTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                                htAtt.am1.position = newdt[0]["Location"].ToString();
                                htAtt.am1.state = "0";
                                htAtt.am1.msg = newdt[0]["Message"].ToString();
                                htAttList.Add(htAtt);
                            }
                            else
                            {
                                HistoryAttence htAtt = new HistoryAttence();
                                htAtt.am1 = new a();
                                htAtt.pm2 = new a();
                                htAtt.am1.time = ((DateTime)newdt[0]["SignTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                                htAtt.am1.position = newdt[0]["Location"].ToString();
                                htAtt.am1.state = "0";
                                htAtt.am1.msg = newdt[0]["Message"].ToString();
                                htAtt.pm2.time = ((DateTime)newdt[newdt.Length - 1]["SignTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                                htAtt.pm2.position = newdt[newdt.Length - 1]["Location"].ToString();
                                htAtt.pm2.state = "0";
                                htAtt.pm2.msg = newdt[newdt.Length - 1]["Message"].ToString();

                                htAttList.Add(htAtt);
                            }
                        }
                        time = time.AddDays(-1);
                    }
                }
            }
            return htAttList.Skip((page-1)*size).Take(size).ToList();
        }

        #region 20160519刘凯明注释
        //if (dt != null)
        //{
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        HistoryAttence htAtt = new HistoryAttence();
        //        htAtt.am1 = new a();
        //        htAtt.am2 = new a();
        //        htAtt.pm1 = new a();
        //        htAtt.pm2 = new a();

        //        //上午签到
        //        if (dt.Rows[i]["AttSetType"].ToString() == "0" && dt.Rows[i]["TypeName"].ToString() == "签到")
        //        {
        //            htAtt.am1.time = dt.Rows[i]["SignTime"].ToString();
        //            htAtt.am1.position = dt.Rows[i]["Location"].ToString();
        //            htAtt.am1.state = dt.Rows[i]["State"].ToString() == "-1" ? "迟到" : dt.Rows[i]["State"].ToString() == "0" ? "正常" : "早退";
        //            htAtt.am1.msg = dt.Rows[i]["Message"].ToString();

        //        }//上午签退
        //        else if (dt.Rows[i]["AttSetType"].ToString() == "0" && dt.Rows[i]["TypeName"].ToString() == "签退")
        //        {
        //            htAtt.am2.time = dt.Rows[i]["SignTime"].ToString();
        //            htAtt.am2.position = dt.Rows[i]["Location"].ToString();
        //            htAtt.am2.state = dt.Rows[i]["State"].ToString() == "-1" ? "迟到" : dt.Rows[i]["State"].ToString() == "0" ? "正常" : "早退";
        //            htAtt.am2.msg = dt.Rows[i]["Message"].ToString();
        //        }
        //        //下午签到
        //        else if (dt.Rows[i]["AttSetType"].ToString() == "1" && dt.Rows[i]["TypeName"].ToString() == "签到")
        //        {
        //            htAtt.pm1.time = dt.Rows[i]["SignTime"].ToString();
        //            htAtt.pm1.position = dt.Rows[i]["Location"].ToString();
        //            htAtt.pm1.state = dt.Rows[i]["State"].ToString() == "-1" ? "迟到" : dt.Rows[i]["State"].ToString() == "0" ? "正常" : "早退";
        //            htAtt.pm1.msg = dt.Rows[i]["Message"].ToString();
        //        }
        //        //下午签退
        //        else if (dt.Rows[i]["AttSetType"].ToString() == "1" && dt.Rows[i]["TypeName"].ToString() == "签退")
        //        {
        //            htAtt.pm2.time = dt.Rows[i]["SignTime"].ToString();
        //            htAtt.pm2.position = dt.Rows[i]["Location"].ToString();
        //            htAtt.pm2.state = dt.Rows[i]["State"].ToString() == "-1" ? "迟到" : dt.Rows[i]["State"].ToString() == "0" ? "正常" : "早退";
        //            htAtt.pm2.msg = dt.Rows[i]["Message"].ToString();
        //        }
        //        else
        //        {
        //            htAtt.pm2.time = dt.Rows[i]["SignTime"].ToString();
        //            htAtt.pm2.position = dt.Rows[i]["Location"].ToString();
        //            htAtt.pm2.state = dt.Rows[i]["State"].ToString() == "-1" ? "迟到" : dt.Rows[i]["State"].ToString() == "0" ? "正常" : "早退";
        //            htAtt.pm2.msg = dt.Rows[i]["Message"].ToString();
        //        }
        //        htAttList.Add(htAtt);
        //    }
        //}

        #endregion
        #endregion

        #region 提交定位
        public bool LocationSubmit(List<LocationModelDetail> model, string token, string JsonData)
        {
            Guid tempToken = Guid.Parse(token);
            UserInfo userEntity = db.UserInfo.Where(x => x.Token == tempToken && x.IsDelete == false).FirstOrDefault();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string sql = "";
            Citys proEntity;
            Citys cityEntity;
            Citys countryEntity;
            foreach (var item in model)
            {
            
                LocusDetailInfo tempEntity = jss.Deserialize<LocusDetailInfo>(item.LocationJSON);
                //DateTime dtime = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Local).AddMilliseconds(tempEntity.Time);
                DateTime dtime = DateTime.Parse(item.LocationDate);

                LocusDetail ldEntity = new LocusDetail();
                ldEntity.Latitude = tempEntity.Latitude;
                ldEntity.Location = tempEntity.Address;
                ldEntity.Longitude = tempEntity.Longitude;
                proEntity =LocalCitys.cityList.Where(x=>x.Name==tempEntity.Province&&x.FCityCode==null).FirstOrDefault();
                if (proEntity != null)
                {
                    ldEntity.Province = proEntity.Code;
                    cityEntity = LocalCitys.cityList.Where(x => x.Name == tempEntity.City && x.FCityCode == proEntity.Code).FirstOrDefault();
                    if (cityEntity != null)
                    {
                        ldEntity.City = cityEntity.Code;
                        countryEntity = LocalCitys.cityList.Where(x => x.Name == tempEntity.District && x.FCityCode == cityEntity.Code).FirstOrDefault();
                        if (countryEntity != null)
                        {
                            ldEntity.Country = countryEntity.Code;
                        }
                    }
                }
                if (tempEntity.Provider == "lbs")
                {
                    //定位方式，0--基站定位
                    ldEntity.LocationMethod = 0;
                }

                ldEntity.Token = token;
                //0--被动定位，1--上班考勤
                ldEntity.LocationSource = item.Type;
                DateTime nowdt = DateTime.Now.Date;
                var list = db.LocusDetail.Where(x => x.Token == ldEntity.Token && x.IsDelete == false).OrderByDescending(x => x.CreateDate).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(item.LocationDate))
                {
                    ldEntity.LocusDate = DateTime.Parse(item.LocationDate);
                }
                else
                {
                    ldEntity.LocusDate = DateTime.Now;
                }
                //前面存在定位信息
                if (list != null&&item.Type!=6)
                {
                    //判断是否是同一位置，如果是同意位置停留时间相加，修改数据

                    if (list.Latitude == tempEntity.Latitude && list.Longitude == tempEntity.Longitude)
                    {
                        TimeSpan ts = dtime.Subtract((DateTime)list.LocusDate);
                        ldEntity.Interval = 0;
                        list.Interval = (int)ts.TotalSeconds;


                        sql += string.Format("update LocusDetail set Interval='{0}' where DetailID='{1}' ", list.Interval, list.DetailID);
                        if (item.Type > 0 && item.Type < 5)
                        {
                            //获取个人考勤时间段集合
                            List<AttendanceSet> aList = db.AttendanceSet.Where(x => x.UserID == userEntity.UserID).ToList();

                            sql += InsertAttendance(item, aList, userEntity, ldEntity, 1);
                        }
                        if (item.Type == 5)
                        {
                            sql += InsertDeparture(ldEntity, userEntity);
                        }
                        continue;
                    }
                    else
                    {
                        TimeSpan ts = (DateTime)ldEntity.LocusDate - (DateTime)list.LocusDate;
                        int interval= (int)ts.TotalSeconds;
                        sql += string.Format("update LocusDetail set Interval='{0}' where DetailID='{1}' ", interval, list.DetailID);

                        ldEntity.Interval = 0;
                    }
                }
                else
                {
                    ldEntity.Interval = 0;
                }


                if (item.Type == 6)
                {
                    if (list != null)
                    {
                        TimeSpan ts = (DateTime)ldEntity.LocusDate - (DateTime)list.LocusDate;
                        int interval = (int)ts.TotalSeconds;
                        sql += string.Format("update LocusDetail set Interval='{0}' where DetailID='{1}' ", interval, list.DetailID);
                    }

                }

                ldEntity.LocationJson = item.LocationJSON;
                ldEntity.CreateDate = DateTime.Now;
                ldEntity.IsDelete = false;




                ldEntity.UserID = userEntity.UserID;
                ldEntity.Token = token;

                
                if (!string.IsNullOrWhiteSpace(item.ComLocusID))
                {
                    //强制定位，推送消息
                    ldEntity.DetailID = Guid.Parse(item.ComLocusID);
                    List<LocusDetail> newLdList = new List<LocusDetail>();
                    newLdList.Add(ldEntity);

                    if (item.MethodSource == 0)
                    {
                        PushPayload payload = new PushPayload();
                        payload.platform = Platform.all();
                        payload.audience = Audience.s_registrationId(item.ListenerPushID);


                        //

                        List<UserLocationInfo> ullList = (from ld in newLdList
                                                          join u in db.UserInfo.Where(x => x.IsDelete == false) on ld.UserID equals u.UserID
                                                          select new UserLocationInfo
                                                          {
                                                              UserID = ld.UserID,
                                                              UserName = u.UserName,
                                                              LocationDate = ((DateTime)ld.LocusDate).ToString("yyyy-MM-dd HH:mm:ss"),
                                                              LocationJSON = ld.LocationJson,
                                                              SpanSecond = ld.Interval,
                                                              Type = ld.LocationSource
                                                          }).ToList();
                        UserLocationInfo uliEntity = ullList.FirstOrDefault();

                        TimeSpan ts = DateTime.Now - DateTime.Parse(uliEntity.LocationDate);


                        PushClient client = new PushClient("b5325fd3f57bd8f160f3602e", "b4b89bce13e686bfc5da0ed0");
                        uliEntity.SpanSecond = (int)ts.TotalSeconds;
          
                        payload.message = Message.content("SendJPushToListener").AddExtras("UserID",uliEntity.UserID.ToString()).AddExtras("UserName",uliEntity.UserName).AddExtras("LocationDate",uliEntity.LocationDate).AddExtras("LocationJSON",uliEntity.LocationJSON).AddExtras("SpanSecond",uliEntity.SpanSecond.ToString()).AddExtras("Type",uliEntity.Type.ToString());

                        
                        MessageResult mr = client.sendPush(payload);

                        UserInfo LsUserEntity=db.UserInfo.Where(x => x.JPushID == item.ListenerPushID).FirstOrDefault();
                        string pushSql = string.Format("insert into AttendanceMsgAlert values(newid(),'{0}',getdate(),'定位信息，{1} {2}位于{3}',2,0) ", LsUserEntity.UserID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), uliEntity.UserName, ldEntity.Location);

                        DBHelper.Instance().ExcuteSql(pushSql);
                    }

                }
                else
                {
                    ldEntity.DetailID = Guid.NewGuid();
                }


                sql += string.Format("insert into LocusDetail values ('{0}',(select UserID from userinfo where token ='{1}'),'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',N'{10}',0,'{11}','{12}','{13}','{14}','{15}')  ", ldEntity.DetailID, ldEntity.Token, ldEntity.Latitude, ldEntity.Longitude, ldEntity.LocusDate, ldEntity.Province, ldEntity.City, ldEntity.Country, ldEntity.Location, ldEntity.LocationMethod, ldEntity.LocationSource, ldEntity.Interval, ldEntity.LocationJson, 0, ldEntity.CreateDate, ldEntity.IsDelete);

                if (item.Type > 0 && item.Type < 5)
                {
                    //获取个人考勤时间段集合
                    List<AttendanceSet> aList = db.AttendanceSet.Where(x => x.UserID == userEntity.UserID).ToList();

                    sql += InsertAttendance(item, aList, userEntity, ldEntity, 1);
                }
                if (item.Type == 5)
                {
                    sql += InsertDeparture(ldEntity, userEntity);
                }
            }

            bool b = DBHelper.Instance().ExcuteSql(sql);
            return b;

        }

        #region 插入偏离信息
        private string InsertDeparture(LocusDetail ldEntity, UserInfo userEntity)
        {
            string strsql = string.Format("insert into Departure(UserID,DepartureDate,DeparturePosition,Latitude,Longitude,IsDelete) values  ('{0}','{1}','{2}','{3}','{4}',0) ", userEntity.UserID, ldEntity.LocusDate, ldEntity.Location,ldEntity.Latitude,ldEntity.Longitude);
            return strsql;
        }
        #endregion

        #region 插入考勤记录
        /// <summary>
        /// 插入考勤记录
        /// </summary>
        /// <param name="aList">个人考勤时间段集合</param>
        /// <param name="userEntity">用户信息</param>
        /// <param name="ldEntity">用户定位信息</param>
        /// <param name="type">类型，1.上午签到，2.上午签退，3.下午签到，4.下午签退</param>
        private string InsertAttendance(LocationModelDetail item, List<AttendanceSet> aList, UserInfo userEntity, LocusDetail ldEntity, int type)
        {

            Attendance atEntity = new Attendance();
            atEntity.AttendanceID = Guid.NewGuid();
            atEntity.UserID = userEntity.UserID;
            atEntity.SignTime = DateTime.Now;


            //获取考勤设置详情
            AttendanceSet asEntity = new AttendanceSet();
            int error = 0;
            if (aList.Count > 0)
            {

                #region 获取考勤设置详情，添加考勤状态
                if (item.Type == 1)//上午上班签到
                {

                    asEntity = aList.Where(x => x.TypeName == "签到" && x.AttSetType == false && x.IsDelete == false).FirstOrDefault();


                    string sql = "select * from attendance where userid='" + userEntity.UserID + "' and CONVERT(varchar(12) , signTime, 111 ) = CONVERT(varchar(12) , getdate(), 111 ) and AttSetID='" + asEntity.AttSetID + "' and isdelete=0 order by signTime desc";
                    DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
                    if (dt.Rows.Count==0)
                    {
                        if (asEntity != null)
                        {
                            int dateDiff = (DateTime.Now.Date - ((DateTime)asEntity.StartTime).Date).Days;
                            asEntity.StartTime = asEntity.StartTime.Value.AddDays(dateDiff);
                            asEntity.Endtime = asEntity.Endtime.Value.AddDays(dateDiff);
                        }
                        else
                        {
                            asEntity.StartTime = DateTime.Now.Date;
                            asEntity.Endtime = DateTime.Now.AddDays(1).Date;
                        }


                        //判断考勤状态
                        if (atEntity.SignTime >= asEntity.StartTime && atEntity.SignTime <= asEntity.Endtime)
                        {
                            atEntity.State = 0;//正常
                        }
                        else if (atEntity.SignTime > asEntity.Endtime)
                        {
                            atEntity.State = -1;//迟到
                        }
                        else
                        {
                            atEntity.State = 2;//异常
                        }


                        asEntity.UserTyep = false;
                        asEntity.TypeName = "签到";
                    }
                    else
                    {
                        return "";
                    }
                }
                else if (item.Type == 2)//上午下班签到
                {
                    asEntity = aList.Where(x => x.TypeName == "签退" && x.AttSetType == false && x.IsDelete == false).FirstOrDefault();

                    string sql = "select * from attendance where userid='" + userEntity.UserID + "' and CONVERT(varchar(12) , signTime, 111 ) = CONVERT(varchar(12) , getdate(), 111 ) and AttSetID='" + asEntity.AttSetID + "' and isdelete=0  order by signTime desc";
                    DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
                    if (dt.Rows.Count == 0)
                    {
                        if (asEntity != null)
                        {
                            int dateDiff = (DateTime.Now.Date - ((DateTime)asEntity.StartTime).Date).Days;

                            asEntity.StartTime = asEntity.StartTime.Value.AddDays(dateDiff);
                            asEntity.Endtime = asEntity.Endtime.Value.AddDays(dateDiff);
                        }
                        else
                        {
                            asEntity.StartTime = DateTime.Now.Date;
                            asEntity.Endtime = DateTime.Now.AddDays(1).Date;
                        }

                        //判断考勤状态
                        if (atEntity.SignTime >= asEntity.StartTime && atEntity.SignTime <= asEntity.Endtime)
                        {
                            atEntity.State = 0;//正常
                        }
                        else if (atEntity.SignTime < asEntity.StartTime)
                        {
                            atEntity.State = 1;//早退
                        }
                        else
                        {
                            atEntity.State = 2;//异常
                        }


                        asEntity.UserTyep = false;
                        asEntity.TypeName = "签退";
                    }
                    else
                    {
                        return "";
                    }
                }
                else if (item.Type == 3)
                {
                    asEntity = aList.Where(x => x.TypeName == "签到" && x.AttSetType == true && x.IsDelete == false).FirstOrDefault();
                    string sql = "select * from attendance where userid='" + userEntity.UserID + "' and CONVERT(varchar(12) , signTime, 111 ) = CONVERT(varchar(12) , getdate(), 111 ) and AttSetID='" + asEntity.AttSetID + "'  and isdelete=0  order by signTime desc";
                    DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
                    if (dt.Rows.Count == 0)
                    {
                        if (asEntity != null)
                        {
                            int dateDiff = (DateTime.Now.Date - ((DateTime)asEntity.StartTime).Date).Days;

                            asEntity.StartTime = asEntity.StartTime.Value.AddDays(dateDiff);
                            asEntity.Endtime = asEntity.Endtime.Value.AddDays(dateDiff);
                        }
                        else
                        {
                            asEntity.StartTime = DateTime.Now.Date;
                            asEntity.Endtime = DateTime.Now.AddDays(1).Date;
                        }

                        //判断考勤状态
                        if (atEntity.SignTime >= asEntity.StartTime && atEntity.SignTime <= asEntity.Endtime)
                        {
                            atEntity.State = 0;//正常
                        }
                        else if (atEntity.SignTime > asEntity.Endtime)
                        {
                            atEntity.State = -1;//迟到
                        }
                        else
                        {
                            atEntity.State = 2;//异常
                        }

                        asEntity.UserTyep = true;
                        asEntity.TypeName = "签到";
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {

                    asEntity = aList.Where(x => x.TypeName == "签退" && x.AttSetType == true && x.IsDelete == false).FirstOrDefault();
                    string sql = "select * from attendance where userid='" + userEntity.UserID + "' and CONVERT(varchar(12) , signTime, 111 ) = CONVERT(varchar(12) , getdate(), 111 ) and AttSetID='" + asEntity.AttSetID + "' and isdelete=0  order by signTime desc";
                    DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
                    if (dt.Rows.Count == 0)
                    {
                        if (asEntity != null)
                        {
                            int dateDiff = (DateTime.Now.Date - ((DateTime)asEntity.StartTime).Date).Days;

                            asEntity.StartTime = asEntity.StartTime.Value.AddDays(dateDiff);
                            asEntity.Endtime = asEntity.Endtime.Value.AddDays(dateDiff);
                        }
                        else
                        {
                            asEntity.StartTime = DateTime.Now.Date;
                            asEntity.Endtime = DateTime.Now.AddDays(1).Date;
                        }


                        //判断考勤状态
                        if (atEntity.SignTime >= asEntity.StartTime && atEntity.SignTime <= asEntity.Endtime)
                        {
                            atEntity.State = 0;//正常
                        }
                        else if (atEntity.SignTime < asEntity.StartTime)
                        {
                            atEntity.State = 1;//早退
                        }
                        else
                        {
                            atEntity.State = 2;//异常
                        }

                        asEntity.UserTyep = true;
                        asEntity.TypeName = "签退";
                    }
                    else
                    {
                        return "";
                    }
                }
                #endregion

                atEntity.AttSetID = asEntity.AttSetID;
            }
            else
            {
                atEntity.State = 0;
            }

            atEntity.Message = item.Message;
            atEntity.Location = ldEntity.Location;
            atEntity.Latitude = ldEntity.Latitude.ToString();
            atEntity.Longitude = ldEntity.Longitude.ToString();
            atEntity.CreateDate = DateTime.Now;
            atEntity.IsDelete = false;
            if (atEntity.AttSetID == null)
            {
                string strSql = string.Format(" insert into Attendance values('{0}','{1}',null,'{2}','{3}','{4}','{5}','{6}','{7}','{10}','{11}','{8}','{9}') ", atEntity.AttendanceID, atEntity.UserID, atEntity.SignTime, atEntity.State, atEntity.Message, atEntity.Location, atEntity.Latitude, atEntity.Longitude, atEntity.CreateDate, atEntity.IsDelete,asEntity.AttSetType,asEntity.TypeName);

                return strSql;
            }
            else
            {
                string strSql = string.Format(" insert into Attendance values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{11}','{12}','{9}','{10}') ", atEntity.AttendanceID, atEntity.UserID, atEntity.AttSetID, atEntity.SignTime, atEntity.State, atEntity.Message, atEntity.Location, atEntity.Latitude, atEntity.Longitude, atEntity.CreateDate, atEntity.IsDelete, asEntity.AttSetType, asEntity.TypeName);


                return strSql;
            }


        }
        #endregion



        #endregion

        #region 上传图像
        /// <summary>
        /// 上传图像
        /// </summary>
        /// <param name="request">token</param>
        /// <param name="msg">headImg Base64</param>
        public string UplaodHeadImg(String token, String headImg)
        {
            Guid Token = new Guid(token);


            string result = "";
            string base64 = headImg;
            byte[] bytes = Convert.FromBase64String(base64);
            MemoryStream memStream = new MemoryStream(bytes);

            Bitmap bmp = new Bitmap(memStream);


            string fileName = DateTime.Now.Ticks.ToString();
            string path = "/UpLoadImg/";

            string dirpath = HttpContext.Current.Server.MapPath(path);
            if (!Directory.Exists(dirpath))
            {
                Directory.CreateDirectory(dirpath);
            }
            //bmp.Save(dirpath + "\\" + fileName, System.Drawing.Imaging.ImageFormat.Png);

            string extentName = SaveImg(dirpath + "\\" + fileName, bmp);
            memStream.Close();


            string sql = string.Format("select * from [UserInfo] where Token= '{0}'", token);
            DataTable userDt = DBHelper.Instance().GetDataTableBySql(sql);

            if (userDt != null && userDt.Rows.Count > 0)
            {
                sql = string.Format(@"update [UserInfo] set UserImg='{0}' where Token='{1}'", path + fileName + extentName, token);
                if (DBHelper.Instance().ExcuteSql(sql))
                {
                    string websiteurl = ConfigurationManager.AppSettings["WebsiteUrl"];
                    result = websiteurl + path + fileName + extentName;
                }
            }
            else
            {
                result = "Token无效！";
            }


            return result;
        }
        #endregion

        #region 获取配置信息
        internal UserRelationConfig LoadLocationConfig(string token)
        {
            Guid gToken = Guid.Parse(token);
            UserInfo uEntity = db.UserInfo.Where(x => x.Token == gToken && x.State == false && x.IsDelete == false).FirstOrDefault();
            if (uEntity != null)
            {
                string sql = string.Format("select MIN(Freqency) as fre,MIN(LocationStartTime) startTime,MAX(LocationEndTime) endTime,(select Raduis from MapInfo) raduis,stuff((SELECT  ','+CONVERT(varchar(10),Weeks) FROM LocationCycle lc WHERE lc.LocalSetID=(select LocalSetID from LocationSettingDetail  where UserID='{0}' and IsDelete=0) order by weeks FOR XML PATH('')),1,1,'') as weeks, (select Latitude from Station where StationID=(select top 1 stationID from StationPerson where userid='{0}' and IsDelete=0)) Latitude , (select Longitude from Station where StationID=(select top 1 stationID from StationPerson where userid='{0}'  and IsDelete=0)) Longitude from LocationSettingDetail as lsd left join LocationSetting as ls on lsd.LocalSetID=ls.LocalSetID and ls.IsDelete=0 where lsd.UserID='{0}'", uEntity.UserID);

                DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);

                List<AttendanceSet> asList=db.AttendanceSet.Where(x => x.UserID ==uEntity.UserID && x.IsDelete == false).ToList();

                if (dt != null)
                {
                    UserRelationConfig lcEntity = new UserRelationConfig();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(dt.Rows[i][0].ToString()))
                        {
                            lcEntity.Interval = int.Parse(dt.Rows[i][0].ToString()) * 1000;
                        }
                        else
                        {
                            lcEntity.Interval = 60 * 1000;
                        }
                        if (!string.IsNullOrWhiteSpace(dt.Rows[i][1].ToString()))
                        {
                            lcEntity.LocationStartTime = DateTime.Parse(dt.Rows[i][1].ToString()).ToString("HH:mm:ss");
                        }
                        else
                        {
                            lcEntity.LocationStartTime = "00:00:00";
                        }
                        if (!string.IsNullOrWhiteSpace(dt.Rows[i][2].ToString()))
                        {
                            lcEntity.LocationEndTime = DateTime.Parse(dt.Rows[i][2].ToString()).ToString("HH:mm:ss");
                        }
                        else
                        {
                            lcEntity.LocationEndTime = "24:00:00";
                        }
                        if (int.Parse(dt.Rows[i][3].ToString()) != 0)
                        {
                            lcEntity.Railings = int.Parse(dt.Rows[i][3].ToString());
                        }
                        else
                        {
                            lcEntity.Railings = 40000;
                        }
                        if (!string.IsNullOrWhiteSpace(dt.Rows[i][4].ToString()))
                        {
                            lcEntity.Weeks = dt.Rows[i][4].ToString();
                        }
                        else
                        {
                            lcEntity.Weeks = "";
                        }
                        if (!string.IsNullOrWhiteSpace(dt.Rows[i][5].ToString()))
                        {
                            lcEntity.Latitude = dt.Rows[i][5].ToString();
                        }
                        else
                        {
                            lcEntity.Latitude = "";
                        }
                        if (!string.IsNullOrWhiteSpace(dt.Rows[i][6].ToString()))
                        {
                            lcEntity.Longitude = dt.Rows[i][6].ToString();
                        }
                        else
                        {
                            lcEntity.Longitude = "";
                        }
                    }

                    if (asList != null)
                    {
                        if (asList.Count > 0)
                        {
                           AttendanceSet asamlisEntity=asList.Where(x => x.TypeName == "签到" && x.AttSetType == false).FirstOrDefault();
                           if (asamlisEntity != null)
                            {
                                lcEntity.amLoginInStartTime = ((DateTime)asamlisEntity.StartTime).ToString("HH:mm:ss");
                                lcEntity.amLoginInEndTime = ((DateTime)asamlisEntity.Endtime).ToString("HH:mm:ss");
                            }
                            AttendanceSet asamlieEntity = asList.Where(x => x.TypeName == "签退" && x.AttSetType == false).FirstOrDefault();
                            if (asamlieEntity != null&&asamlieEntity.UserTyep==false)
                            {
                                lcEntity.amLoginOutStartTime = ((DateTime)asamlieEntity.StartTime).ToString("HH:mm:ss");
                                lcEntity.amLoginOutEndTime = ((DateTime)asamlieEntity.Endtime).ToString("HH:mm:ss");
                            }
                            AttendanceSet aspmlisEntity = asList.Where(x => x.TypeName == "签到" && x.AttSetType == true).FirstOrDefault();
                            if (aspmlisEntity != null&&aspmlisEntity.UserTyep==false)
                            {
                                lcEntity.pmLoginInStartTime = ((DateTime)aspmlisEntity.StartTime).ToString("HH:mm:ss");
                                lcEntity.pmLoginInEndTime = ((DateTime)aspmlisEntity.Endtime).ToString("HH:mm:ss");
                            }
                            AttendanceSet aspmlieEntity = asList.Where(x => x.TypeName == "签退" && x.AttSetType == true).FirstOrDefault();
                            if (aspmlieEntity != null)
                            {
                                lcEntity.pmLoginOutStartTime = ((DateTime)aspmlieEntity.StartTime).ToString("HH:mm:ss");
                                lcEntity.pmLoginOutEndTime = ((DateTime)aspmlieEntity.Endtime).ToString("HH:mm:ss");
                            }

                        }
                    }
                    lcEntity.IsLeader = uEntity.IsLeader;
                    return lcEntity;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 更新极光ID到用户表
        internal bool UpdateJPushID2UserInfo(string jID, string token)
        {
            // Guid gToken = Guid.Parse(token);
            // UserInfo userEntity=db.UserInfo.Where(x => x.Token == gToken && x.State == false && x.IsDelete == false).FirstOrDefault();
            string sql = string.Format(" update userinfo set jpushid=null where userid in (select userid from userinfo where jpushid ='{0}')  ", jID);

            sql += string.Format(" update userinfo set JPushID='{0}' where Token='{1}' and State=0 and IsDelete=0", jID, token);
            return DBHelper.Instance().ExcuteSql(sql);
        }
        #endregion

        #region 极光推送消息
        public JpushMsg PushMsg()
        {
            
            string ApiKey = "b5325fd3f57bd8f160f3602e";
            string APIMasterSecret = "b4b89bce13e686bfc5da0ed0 ";
            string pushSql = "";
            try
            {
                Random ran = new Random();
                int sendno = ran.Next(1, 2100000000);//随机生成的一个编号
                string app_key = ApiKey;
                string masterSecret = APIMasterSecret;
                int receiver_type = 3;//接收者类型。2、指定的 tag。3、指定的 alias。4、广播：对 app_key 下的所有用户推送消息。5、根据 RegistrationID 进行推送。当前只是 Android SDK r1.6.0 版本支持

                #region 获取半小时内该签到的人数JPushID

                string sql = string.Format("select distinct ui.JPushID ,ui.UserID from AttendanceSet as atts left join UserInfo as ui on atts.UserID=ui.UserID and ui.State=0 and ui.IsDelete=0  where endtime<DATEADD(mi,30,getdate()) and endtime>dateadd(mi,-30,GETDATE())");
                #endregion
                string UserStr = "";
                DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(dt.Rows[i][0].ToString()))
                    {

                        UserStr += dt.Rows[i][0];
                        UserStr += ",";

                        pushSql += string.Format("insert into AttendanceMsgAlert values(newid(),'{0}',getdate(),'请注意考勤签到/退',2,0) ", dt.Rows[i][1]);
                    }
                }
                string receiver_value = "";
                if (UserStr.Length > 1)
                {
                    receiver_value = UserStr.Substring(0, UserStr.Length - 1);
                }
                int msg_type = 1;//1、通知2、自定义消息（只有 Android 支持）
                string msg_content = "{\"n_builder_id\":\"00\",\"n_title\":\"" + "签到提醒" + "\",\"n_content\":\"" + "请注意考勤签到/退" + "\"}";//消息内容
                string platform = "android";//目标用户终端手机的平台类型，如： android, ios 多个请使用逗号分隔。
                string verification_code = GetMD5Str(sendno.ToString(), receiver_type.ToString(), receiver_value, masterSecret);//验证串，用于校验发送的合法性。MD5
                string postData = "sendno=" + sendno;
                postData += ("&app_key=" + app_key);
                postData += ("&masterSecret=" + masterSecret);
                postData += ("&receiver_type=" + receiver_type);
                postData += ("&receiver_value=" + receiver_value);
                postData += ("&msg_type=" + msg_type);
                postData += ("&msg_content=" + msg_content);
                postData += ("&platform=" + platform);
                postData += ("&verification_code=" + verification_code);

                //byte[] data = encoding.GetBytes(postData);
                byte[] data = Encoding.UTF8.GetBytes(postData);
                string resCode = GetPostRequest(data);//调用极光的接口获取返回值
                JpushMsg msg = Newtonsoft.Json.JsonConvert.DeserializeObject<JpushMsg>(resCode);//定义一个JpushMsg类，包含返回值信息，将返回的json格式字符串转成JpushMsg对象 
                DBHelper.Instance().ExcuteSql(pushSql);
                //AttendanceMsgAlert
                // //遍历所有角色
                //List<RoleInfo> rList=db.RoleInfo.Where(x => x.IsDelete == false).ToList();
                //List<RoleUser> ruList = db.RoleUser.Where(x => x.IsDelete == false).ToList();
                //List<RoleUserDisplay> rudList = db.RoleUserDisplay.Where(x => x.IsDelete == false).ToList();
                //List<UserInfo> uiList = db.UserInfo.Where(x => x.IsDelete == false && x.State == false).ToList();
                // //所有用有角色的人
                //foreach (var item in rList)
                //{
                //    //
                //    ruList.Where(x => x.RoleID == item.RoleID).FirstOrDefault();
                //}
                // //所有角色有权限查看的人
                return msg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// MD5字符串
        /// </summary>
        /// <param name="paras">参数数组</param>
        /// <returns>MD5字符串</returns>
        public string GetMD5Str(params string[] paras)
        {
            string str = "";
            for (int i = 0; i < paras.Length; i++)
            {
                str += paras[i];
            }
            byte[] buffer = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str));
            string md5Str = string.Empty;
            for (int i = 0; i < buffer.Length; i++)
            {
                md5Str = md5Str + buffer[i].ToString("X2");
            }
            return md5Str;
        }



        /// <summary>
        /// Post方式请求获取返回值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetPostRequest(byte[] data)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://api.jpush.cn:8800/v2/push");

            myRequest.Method = "POST";//极光http请求方式为post
            myRequest.ContentType = "application/x-www-form-urlencoded";//按照极光的要求
            myRequest.ContentLength = data.Length;
            Stream newStream = myRequest.GetRequestStream();

            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            // Get response
            var response = (HttpWebResponse)myRequest.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
            {
                string result = reader.ReadToEnd();
                reader.Close();
                response.Close();
                return result;
            }
        }




        public class JpushMsg
        {
            private string sendno;//编号

            public string Sendno
            {
                get { return sendno; }
                set { sendno = value; }
            }
            private string msg_id;//信息编号

            public string Msg_id
            {
                get { return msg_id; }
                set { msg_id = value; }
            }
            private string errcode;//返回码

            public string Errcode
            {
                get { return errcode; }
                set { errcode = value; }
            }
            private string errmsg;//错误信息

            public string Errmsg
            {
                get { return errmsg; }
                set { errmsg = value; }
            }
        }
        #endregion

        #region 极光推送
        public void NewJpush()
        {

            PostMessageToEmployee();

            PostMessageToLeader();
        }

        private void PostMessageToLeader()
        {
            PushClient client = new PushClient("b5325fd3f57bd8f160f3602e", "b4b89bce13e686bfc5da0ed0");


            string[] users = new string[] { };
            string pushSql = "";

            List<RoleInfo> rList = db.RoleInfo.Where(x => x.IsDelete == false).ToList();
            List<RoleUser> ruList = db.RoleUser.Where(x => x.IsDelete == false).ToList();
            List<RoleUserDisplay> rudList = db.RoleUserDisplay.Where(x => x.IsDelete == false).ToList();
            List<UserInfo> uiList = db.UserInfo.Where(x => x.IsDelete == false && x.State == false).ToList();

            List<RolesLimit2Users> rl2uList = (from rud in rudList
                       join ui in uiList on rud.UserID equals ui.UserID
                       select new RolesLimit2Users
                       {
                           RoleID=(Guid)rud.RoleID,
                           UserID=(Guid)rud.UserID,
                           UserName=ui.UserName
                       }).ToList();
            List<RoleUser> newRuList = new List<RoleUser>();
            UserInfo uiEntity = new UserInfo();
            HashSet<string> usList = new HashSet<string>();//需要推送给的对象
            
            


            List<string> temUser = new List<string>();
            List<RolesLimit2Users> ll2uList = new List<RolesLimit2Users>();
            foreach (var item in rList)
            {
                string msg = "";//需要推送的内容
                string userids = "";
                //获取拥有当前角色的所有用户的JpushID
                 newRuList=ruList.Where(x => x.RoleID == item.RoleID).ToList();
                 foreach (var im in newRuList)
                 {
                     uiEntity=uiList.Where(x => x.UserID == im.UserID).FirstOrDefault();

                     if (uiEntity != null)
                     {
                         if (!string.IsNullOrWhiteSpace(uiEntity.JPushID))
                         {
                             usList.Add(uiEntity.JPushID);
                             temUser.Add(uiEntity.UserID.ToString());
                         }
                         
                     }
                 }

                 ll2uList=rl2uList.Where(x => x.RoleID == item.RoleID).DistinctBy(x=>x.UserID).ToList();


                 foreach (var tm in ll2uList)
                 {
                     if (msg.Length > 0)
                     {
                         userids += ",";
                     }
           
                     userids += tm.UserID;
                 }


                 string sql = string.Format("select distinct ui.JPushID ,ui.UserID,ui.UserName from AttendanceSet as atts left join UserInfo as ui on atts.UserID=ui.UserID and ui.State=0 and ui.IsDelete=0  where ui.UserID not in (select userid from Attendance where userid=ui.userid and CONVERT(varchar(12),signTime,23 )=CONVERT(varchar(12) ,getdate(),23 ) and isdelete=0) and atts.isdelete=0 and(CONVERT(varchar(12), atts.StartTime,108 ) between CONVERT(varchar(12) ,getdate(),108 )  and  CONVERT(varchar(12) ,dateadd(mi,30,getdate()),108 )  ) and atts.isdelete=0  and  ui.jpushid is not null and atts.UserID in ('{0}')",userids.Replace(",","','"));

                 DataTable testdt=DBHelper.Instance().GetDataTableBySql(sql);

                 if (testdt != null)
                 {
                     for (int i = 0; i < testdt.Rows.Count; i++)
                     {
                          if (i > 0)
                         {
                             msg += ",";
                         }
                          msg += testdt.Rows[i][2];
                     }
                     if (msg.Length > 0)
                     {
                         msg += "以上成员尚未打卡,请注意提醒！";

                         PushPayload payload = new PushPayload();
                         payload.platform = Platform.all();
                         payload.audience = Audience.s_registrationId(usList);

                         //payload.message = Message.content("测试");
                         payload.notification = new Notification().setAlert(msg);
                         MessageResult mr = client.sendPush(payload);
                         foreach (var tem in temUser)
                         {

                             pushSql += string.Format("insert into AttendanceMsgAlert values(newid(),'{0}',getdate(),'{1}',2,0) ", tem, msg);
                         }

                         DBHelper.Instance().ExcuteSql(pushSql);
                     }
                     

                  
                 }
                 

               
            }

          
        }

        private static void PostMessageToEmployee()
        {
            PushClient client = new PushClient("b5325fd3f57bd8f160f3602e", "b4b89bce13e686bfc5da0ed0");

            #region 获取半小时内该签到的人数JPushID

            string sql = string.Format("select distinct ui.JPushID ,ui.UserID from AttendanceSet as atts left join UserInfo as ui on atts.UserID=ui.UserID and ui.State=0 and ui.IsDelete=0  where ui.UserID not in (select userid from Attendance where userid=ui.userid and CONVERT(varchar(12),signTime,23 )=CONVERT(varchar(12) ,getdate(),23 ) and isdelete=0) and atts.isdelete=0 and(CONVERT(varchar(12), atts.StartTime,108 ) between CONVERT(varchar(12) ,getdate(),108 )  and  CONVERT(varchar(12) ,dateadd(mi,30,getdate()),108 )  ) and atts.isdelete=0  and  ui.jpushid is not null");
            #endregion
            string UserStr = "";
            string pushSql = "";
            DataTable dt = DBHelper.Instance().GetDataTableBySql(sql);
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(dt.Rows[i][0].ToString()))
                    {

                        UserStr += dt.Rows[i][0];
                        UserStr += ",";

                        pushSql += string.Format("insert into AttendanceMsgAlert values(newid(),'{0}',getdate(),'请注意考勤签到/退',2,0) ", dt.Rows[i][1]);
                    }
                }

                string receiver_value = "";
                if (UserStr.Length > 1)
                {
                    receiver_value = UserStr.Substring(0, UserStr.Length - 1);
                    string[] users = receiver_value.Split(',');

                    PushPayload payload = new PushPayload();
                    payload.platform = Platform.all();
                    payload.audience = Audience.s_registrationId(users);

                    //payload.message = Message.content("测试");
                    //payload.notification = new Notification().AndroidNotification
                    payload.notification = new Notification().setAlert("打卡时间到，请注意签到/签退");
                    MessageResult mr = client.sendPush(payload);
                    DBHelper.Instance().ExcuteSql(pushSql);
                }
            }
         
         
      
        }
        #endregion

        #region 图片转换
        private string SaveImg(string dirpath, Bitmap image)
        {
            ImageFormat format = image.RawFormat;
            if (format.Equals(ImageFormat.Jpeg))
            {
                image.Save(dirpath + ".jpeg", ImageFormat.Jpeg);
                return ".jpeg";
            }
            else if (format.Equals(ImageFormat.Bmp))
            {
                image.Save(dirpath + ".bmp", ImageFormat.Bmp);
                return ".bmp";
            }
            else if (format.Equals(ImageFormat.Gif))
            {
                image.Save(dirpath + ".gif", ImageFormat.Gif);
                return ".gif";
            }
            else if (format.Equals(ImageFormat.Icon))
            {
                image.Save(dirpath + ".icon", ImageFormat.Icon);
                return ".icon";
            }
            else
            {
                image.Save(dirpath + ".png", ImageFormat.Png);
                return ".png";
            }
        }
        #endregion

        #region 根据数据权限读取用户列表
        public List<LimitUser> getLimitUserInfoByUserID(string token, string type)
        {
            Guid userToken = Guid.Parse(token);
            string websiteurl = ConfigurationManager.AppSettings["WebsiteUrl"];
            UserInfo uEntity = db.UserInfo.Where(x => x.Token == userToken && x.State == false && x.IsDelete == false).FirstOrDefault();
            Guid UserID = uEntity.UserID;
            List<LimitUser> luList = (from ru in db.RoleUser.Where(x => x.UserID == UserID && x.IsDelete == false)
                                      join rud in db.RoleUserDisplay.Where(x => x.IsDelete == false) on ru.RoleID equals rud.RoleID
                                      join us in db.UserInfo.Where(x => x.IsDelete == false) on rud.UserID equals us.UserID
                                      select new LimitUser
                                      {
                                          UserID = rud.UserID,
                                          UserName = us.UserName,
                                          UserImg = websiteurl+us.UserImg,
                                          LoginNumber=us.LoginNumber
                                      }).ToList();

            if (type == "0")
            {
                LimitUser lu = new LimitUser();
                lu.UserID = uEntity.UserID;
                lu.UserName = uEntity.UserName;
                lu.UserImg =websiteurl+ uEntity.UserImg;
                lu.LoginNumber = uEntity.LoginNumber;
                if (!luList.Contains(lu))
                {
                    luList.Add(lu);
                }
            }


            luList = luList.DistinctBy(x => x.UserID).ToList();
            return luList;
        }
        #endregion


        #region 获取推送列表
        public List<PushMessage> getPushMessageList(string token, string type, string userID, string startTime, string endTime, int page, int size)
        {
            Guid uID = Guid.Parse(userID);

            DateTime start = DateTime.Parse(startTime);

            DateTime end = DateTime.Parse(endTime);

            List<PushMessage> pList = new List<PushMessage>();
            List<AttendanceMsgAlert> amaList = new List<AttendanceMsgAlert>();
            if (type == "0")
            {
                //.OrderByDescending(x=>x.AlertDate).Skip((page-1)*size).Take(size).ToList()
                amaList = db.AttendanceMsgAlert.Where(x => x.UserID == uID && x.AlertDate >= start && x.AlertDate < end && x.IsDelete == false).OrderByDescending(x => x.AlertDate).ToList().Skip((page - 1) * size).Take(size).ToList();
            }
            else
            {
                int t = int.Parse(type);
                amaList = db.AttendanceMsgAlert.Where(x => x.UserID == uID && x.AlertDate >= start && x.AlertDate < end && x.IsDelete == false).OrderByDescending(x => x.AlertDate).ToList().Skip((page - 1) * size).Take(size).ToList();
            }

            List<PushMessage> pushList = (from am in amaList
                                          join ui in db.UserInfo.Where(x => x.IsDelete == false) on am.UserID equals ui.UserID
                                          select new PushMessage
                                          {
                                              UserID = am.UserID,
                                              UserName = ui.UserName,
                                              AlertDate = am.AlertDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                              MsgID = am.MsgID,
                                              Message = am.Message,
                                              Type = am.AttType
                                          }).ToList();

            return pushList;
        }
        #endregion

        #region 强制发送定位信息
        internal void SendMessage4GetCurrentAddress(string userid, string newid,string token,int type)
        {
            string JpushID = GetJPushID(userid);
            string ListenerPushID = "";
            if (type == 0)
            {
                ListenerPushID = GetListenerPushIDByToken(token);//获取请求人PushＩＤ
            }
            
            if (!string.IsNullOrWhiteSpace(JpushID))
            {
                PushClient client = new PushClient("b5325fd3f57bd8f160f3602e", "b4b89bce13e686bfc5da0ed0");



                PushPayload payload = new PushPayload();
                payload.platform = Platform.all();
                payload.audience = Audience.s_registrationId(JpushID);

                payload.message = Message.content("CompellentLocus").AddExtras("ComLocusID", newid).AddExtras("ListenerPushID",ListenerPushID).AddExtras("MethodSource",type);
                //string pushSql = string.Format("insert into AttendanceMsgAlert values(newid(),'{0}',getdate(),'被请求强制定位',2,0) ", userid);
                MessageResult mr = client.sendPush(payload);
                //DBHelper.Instance().ExcuteSql(pushSql);
            }
        }

        private string GetListenerPushIDByToken(string token)
        {
            Guid NewToken=Guid.Parse(token);
            UserInfo user = db.UserInfo.Where(x => x.Token == NewToken).FirstOrDefault();
            return user.JPushID;
        }

        private string GetJPushID(string userid)
        {
            Guid CurrentUserID=Guid.Parse(userid);
            UserInfo user=db.UserInfo.Where(x => x.UserID == CurrentUserID).FirstOrDefault();
            if (user != null)
            {
                return user.JPushID;
            }
            else
            {
                return "";
            }
        }
        #endregion


      
    }

    public static class ExtensionInfo
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }   
}