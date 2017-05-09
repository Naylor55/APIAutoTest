using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using BKS_Sys.App.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using BKS_Sys.App.Models.Base;
using Newtonsoft.Json;
using BKS_Sys.Model;
using System.IO.Compression;

namespace BKS_Sys.App.Filter
{
    public class RequestDataParseFilter : ActionFilterAttribute
    {
        UserInfo user = new UserInfo();
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            base.OnActionExecuting(actionContext);
            try
            {
                var pars = actionContext.Request.GetQueryNameValuePairs();
                foreach (var par in pars)
                {
                    HttpContext.Current.Items.Add(par.Key, par.Value);
                }
                if (actionContext.Request.Method == HttpMethod.Post)
                {
                    //是否是zip-json
                    string bodyStr = "";
                    bool bZip = false;
                    if (actionContext.Request.Content.Headers.Contains("Content-Encoding"))
                    {

                        var ContentType = actionContext.Request.Content.Headers.GetValues("Content-Encoding").FirstOrDefault();
                        if (!String.IsNullOrEmpty(ContentType) && ContentType.Trim() == "gzip")
                        {
                            byte[] zip = await actionContext.Request.Content.ReadAsByteArrayAsync();
                            byte[] unZip = ZipWrapperUtils.Decompress(zip);
                            char[] _unZip = System.Text.Encoding.UTF8.GetChars(unZip);
                            bodyStr = new String(_unZip);
                            bZip = true;
                        }
                    }
                    if (!bZip)
                    {
                        bodyStr = await actionContext.Request.Content.ReadAsStringAsync();
                    }
                    if (string.IsNullOrEmpty(bodyStr))
                    {
                        HandleBadRequest(actionContext);
                        return;
                    }
                    JObject jObj = JObject.Parse(bodyStr);
                    if (jObj["Request"] == null)
                    {
                        HandleBadRequest(actionContext);
                        return;
                    }
                    RequestHead rh = new RequestHead()
                    {
                        Version = Convert.ToString(jObj["Request"]["Version"]),
                        Token = Convert.ToString(jObj["Request"]["Token"]),
                        Width = Convert.ToString(jObj["Request"]["Width"]),
                        Height = Convert.ToString(jObj["Request"]["Height"]),
                        Platform = Convert.ToString(jObj["Request"]["Platform"]),
                        Imsi1 = Convert.ToString(jObj["Request"]["Imsi1"]),
                        Imsi2 = Convert.ToString(jObj["Request"]["Imsi2"]),
                        Imei = Convert.ToString(jObj["Request"]["Imei"]),
                    };
                    HttpContext.Current.Items.Add("RequestHead", rh);
                    HttpContext.Current.Items.Add("RequestBody", jObj["Body"]);
                    foreach (JProperty item in jObj["Body"].Children<JProperty>())
                    {
                        HttpContext.Current.Items.Add(item.Name, item.Value);
                    }
                    HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
                    //判断是否要验证token过期
                    var actionName = actionContext.ActionDescriptor.ActionName;

                    if ("login" != actionName.ToLower())
                    {
                        string token = jObj["Request"]["Token"].ToString().Trim();
                        user = Utility.GetUserInfo(token, "", "", true);
                        if (user == null)
                        {
                            //验证token过期 
                            TokenExpire(actionContext);
                        }
                    }
                    else
                    {
                        string LoginNumber = HttpContext.Current.Items["LoginNumber"].ToString();
                        string Password = HttpContext.Current.Items["Password"].ToString();

                        user = Utility.GetUserInfo("", LoginNumber, Utility.Md532(Password.Trim()), false);
                    }
                    if (user != null)
                    {
                        String Imsi1 = Convert.ToString(jObj["Request"]["Imsi1"]).Trim().ToLower();
                        String Imsi2 = Convert.ToString(jObj["Request"]["Imsi2"]).Trim().ToLower();
                        String Imei = Convert.ToString(jObj["Request"]["Imei"]).Trim().ToLower();
                        if (user.IMSI1Bind)
                        {
                            if (Imsi1 != user.IMSI1)
                            {
                                IMSExpire(actionContext);
                            }
                        }
                        else if (user.IMSI2Bind)
                        {
                            if (Imsi2 != user.IMSI2)
                            {
                                IMSExpire(actionContext);
                            }
                        }
                        else if (user.IMEI1Bind)
                        {
                            if (Imei != user.IMEI1)
                            {
                                IMSExpire(actionContext);
                            }
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(user.IMSI1) && String.IsNullOrEmpty(user.IMSI2) && String.IsNullOrEmpty(user.IMEI1))
                            {
                                Utility.UpdateIMIS(user.UserID, Imsi1, Imsi2, Imei);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ////LogHelper.Instance.Error(ErrorCode.BadRequest, ex.Message, ex);
                HandleBadRequest(actionContext);
            }
        }

        /// <summary>
        /// 在请求执行完后 记录请求的数据以及返回数据
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            HttpRequest request = HttpContext.Current.Request;
            string bodyStr = "";
            if (actionExecutedContext.Request.Content.Headers.Contains("Content-Encoding"))
           {
               //取返回值
               var ContentType = actionExecutedContext.Request.Content.Headers.GetValues("Content-Encoding").FirstOrDefault();
               //是否是zip-json
               if (!String.IsNullOrEmpty(ContentType) && ContentType.Trim() == "gzip")
               {
                   //读取返回值
                   string resultStr = await actionExecutedContext.Response.Content.ReadAsStringAsync();
                   byte[] bUnZip = System.Text.Encoding.UTF8.GetBytes(resultStr);
                   byte[] bZip = ZipWrapperUtils.Compress(bUnZip);
                   var response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK);
                   response.Content = new ByteArrayContent(bZip);
                   actionExecutedContext.Response = response;
                   //读取请求参数
                   byte[] zip = await actionExecutedContext.Request.Content.ReadAsByteArrayAsync();
                   byte[] unZip = ZipWrapperUtils.Decompress(zip);
                   char[] _unZip = System.Text.Encoding.UTF8.GetChars(unZip);
                   bodyStr = new String(_unZip);
               }
               else
               {
                   bodyStr = await actionExecutedContext.Request.Content.ReadAsStringAsync();
               }
           }
            

            ///获取action名称
            string actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
            string controllerName = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string FilePath = request.FilePath;
            string sNamespace = "BKS_Sys.App.ApiControllers.";
            sNamespace += controllerName.Trim() + "Controller";
            //string bodyStr = await actionExecutedContext.Request.Content.ReadAsStringAsync();
            string sDescription = Utility.GetDescriptionAttribute(sNamespace, actionName)+"，请求参数："+bodyStr.Trim();
            string ip = request.UserHostAddress;
            int iType = 0;
            if ("login" == actionName.ToLower())
            {
                iType = 1;
            }
            else if ("exitsystem" == actionName.ToLower())
            {
                iType = 2;
            }
            Utility.WriteOperationLog(user.UserID,user.UserName, DateTime.Now, ip, FilePath, sDescription, iType);
            //return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        private void HandleBadRequest(HttpActionContext actionContext)
        {
            //var response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
            //response.Content = new StringContent("Request is invalid");
            //actionContext.Response = response;
            RequestError(actionContext);
        }

        private void IMSExpire(HttpActionContext actionContext)
        {
            ResponseJson(actionContext, "401", "手机绑定了其他的账号，请联系管理员");
        }

        //请求参数错误
        private void RequestError(HttpActionContext actionContext)
        {
            ResponseJson(actionContext, "201", "请求参数错误");
        }

        private void RequestError(HttpActionContext actionContext,String s)
        {
            ResponseJson(actionContext, "201", s);
        }
        //token过期
        private void TokenExpire(HttpActionContext actionContext)
        {
            ResponseJson(actionContext, "401", "token错误");
        }
        private void ResponseJson(HttpActionContext actionContext, String Code, String Msg)
        {
            ResponseBody<Object> body = new ResponseBody<Object>();
            body.Result = new Result();
            body.Result.Code = Code;
            body.Result.Msg = Msg;
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            jsetting.NullValueHandling = NullValueHandling.Ignore;
            string s = JsonConvert.SerializeObject(body, Formatting.Indented, jsetting);
            var response = actionContext.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(s);
            actionContext.Response = response;
        }
    }
}