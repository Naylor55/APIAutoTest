using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BKS_Sys.App.Models
{
    public class BaseHttpClient
    {
        private const String CHARSET = "UTF-8";
        private const String RATE_LIMIT_QUOTA = "X-Rate-Limit-Limit";
        private const String RATE_LIMIT_Remaining = "X-Rate-Limit-Remaining";
        private const String RATE_LIMIT_Reset = "X-Rate-Limit-Reset";

        protected const int RESPONSE_OK = 200;

        //设置连接超时时间
        private const int DEFAULT_CONNECTION_TIMEOUT = (20 * 1000); // milliseconds
        //设置读取超时时间
        private const int DEFAULT_SOCKET_TIMEOUT = (30 * 1000); // milliseconds

        public ResponseWrapper sendPost(String url, String auth, String reqParams)
        {
            return this.sendRequest("POST", url, auth, reqParams);
        }
        public ResponseWrapper sendDelete(String url, String auth, String reqParams)
        {
            return this.sendRequest("DELETE", url, auth, reqParams);
        }
        public ResponseWrapper sendGet(String url, String auth, String reqParams)
        {
            return this.sendRequest("GET", url, auth, reqParams);
        }
        /**
         *
         * method "POST" or "GET"
         * url
         * auth   可选
         */
        public ResponseWrapper sendRequest(String method, String url, String auth, String reqParams)
        {
            Console.WriteLine("Send request - " + method.ToString() + " " + url + " " + DateTime.Now);
            if (null != reqParams)
            {
                Console.WriteLine("Request Content - " + reqParams + " " + DateTime.Now);
            }
            ResponseWrapper result = new ResponseWrapper();
            HttpWebRequest myReq = null;
            HttpWebResponse response = null;
            try
            {
                myReq = (HttpWebRequest)WebRequest.Create(url);
                myReq.Method = method;
                myReq.ContentType = "application/json";
                if (!String.IsNullOrEmpty(auth))
                {
                    myReq.Headers.Add("Authorization", "Basic " + auth);
                }
                if (method == "POST")
                {
                    byte[] bs = UTF8Encoding.UTF8.GetBytes(reqParams);
                    myReq.ContentLength = bs.Length;
                    using (Stream reqStream = myReq.GetRequestStream())
                    {
                        reqStream.Write(bs, 0, bs.Length);
                        reqStream.Close();
                    }
                }
                response = (HttpWebResponse)myReq.GetResponse();
                HttpStatusCode statusCode = response.StatusCode;
                result.responseCode = statusCode;
                if (Equals(response.StatusCode, HttpStatusCode.OK))
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        result.responseContent = reader.ReadToEnd();
                    }
                    String limitQuota = response.GetResponseHeader(RATE_LIMIT_QUOTA);
                    String limitRemaining = response.GetResponseHeader(RATE_LIMIT_Remaining);
                    String limitReset = response.GetResponseHeader(RATE_LIMIT_Reset);
                    result.setRateLimit(limitQuota, limitRemaining, limitReset);
                    Console.WriteLine("Succeed to get response - 200 OK" + " " + DateTime.Now);
                    Console.WriteLine("Response Content - {0}", result.responseContent + " " + DateTime.Now);
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpStatusCode errorCode = ((HttpWebResponse)e.Response).StatusCode;
                    string statusDescription = ((HttpWebResponse)e.Response).StatusDescription;
                    using (StreamReader sr = new StreamReader(((HttpWebResponse)e.Response).GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        result.responseContent = sr.ReadToEnd();
                    }
                    result.responseCode = errorCode;
                    result.exceptionString = e.Message;
                    String limitQuota = ((HttpWebResponse)e.Response).GetResponseHeader(RATE_LIMIT_QUOTA);
                    String limitRemaining = ((HttpWebResponse)e.Response).GetResponseHeader(RATE_LIMIT_Remaining);
                    String limitReset = ((HttpWebResponse)e.Response).GetResponseHeader(RATE_LIMIT_Reset);
                    result.setRateLimit(limitQuota, limitRemaining, limitReset);
                    Debug.Print(e.Message);
                    result.setErrorObject();
                    Console.WriteLine(string.Format("fail  to get response - {0}", errorCode) + " " + DateTime.Now);
                    Console.WriteLine(string.Format("Response Content - {0}", result.responseContent) + " " + DateTime.Now);

                    throw new APIRequestException(result);
                }
                else
                {//
                    throw new APIConnectionException(e.Message);
                }

            }
            //这里不再抓取非http的异常，如果异常抛出交给开发者自行处理
            //catch (System.Exception ex)
            //{
            //     String errorMsg = ex.Message;
            //     Debug.Print(errorMsg);
            //}
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
                if (myReq != null)
                {
                    myReq.Abort();
                }
            }
            return result;
        }
    }



    public class APIRequestException : Exception
    {

        private ResponseWrapper responseRequest;
        public APIRequestException(ResponseWrapper responseRequest)
            : base(responseRequest.exceptionString)
        {
            this.responseRequest = responseRequest;
        }
        public HttpStatusCode Status
        {
            get
            {
                return this.responseRequest.responseCode;
            }
        }
        public long MsgId
        {
            get
            {
                return responseRequest.jpushError.msg_id;
            }
        }
        public int ErrorCode
        {
            get
            {
                return responseRequest.jpushError.error.code;
            }
        }

        public String ErrorMessage
        {
            get
            {
                return responseRequest.jpushError.error.message;
            }
        }
        private JpushError ErrorObject()
        {
            return responseRequest.jpushError;
        }
        public int RateLimitQuota()
        {
            return responseRequest.rateLimitQuota;
        }
        public int RateLimitRemaining()
        {
            return responseRequest.rateLimitRemaining;
        }
        public int RateLimitReset()
        {
            return responseRequest.rateLimitReset;
        }
    }


    public class APIConnectionException : Exception
    {
        public APIConnectionException(String message)
            : base(message)
        {

        }
    }

    public class ResponseWrapper
    {
        private const int RESPONSE_CODE_NONE = -1;

        //private static Gson _gson = new Gson();
        public JpushError jpushError;

        public HttpStatusCode responseCode = HttpStatusCode.BadRequest;
        private String _responseContent;
        public String responseContent
        {
            get
            {
                return _responseContent;
            }
            set
            {
                _responseContent = value;
            }
        }
        public void setErrorObject()
        {
            if (!string.IsNullOrEmpty(_responseContent))
            {
                jpushError = JsonConvert.DeserializeObject<JpushError>(_responseContent);
            }
        }

        public int rateLimitQuota;
        public int rateLimitRemaining;
        public int rateLimitReset;

        public bool isServerResponse()
        {
            return responseCode == HttpStatusCode.OK;
        }
        public String exceptionString;

        public ResponseWrapper()
        {
        }
        public void setRateLimit(String quota, String remaining, String reset)
        {
            if (null == quota) return;
            try
            {
                if (quota != "" && StringUtil.IsInt(quota))
                {
                    rateLimitQuota = int.Parse(quota);
                }
                if (remaining != "" && StringUtil.IsInt(remaining))
                {
                    rateLimitRemaining = int.Parse(remaining);
                }
                if (reset != "" && StringUtil.IsInt(reset))
                {
                    rateLimitReset = int.Parse(reset);
                }
                Console.WriteLine(string.Format("JPush API Rate Limiting params - quota:{0}, remaining:{1}, reset:{2} ", quota, remaining, reset) + " " + DateTime.Now);
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

    }

    public class JpushSuccess
    {
        public string sendno;
        public string msg_id;
    }
    public class JpushError
    {
        public JpushErrorObject error;
        public long msg_id;
    }
    public class JpushErrorObject
    {
        public int code;
        public String message;
    }


    class StringUtil
    {

        public bool IsNumber(String strNumber)
        {
            Regex objNotNumberPattern = new Regex("[^0-9.-]");
            Regex objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
            Regex objTwoMinusPattern = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
            String strValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
            String strValidIntegerPattern = "^([-]|[0-9])[0-9]*$";
            Regex objNumberPattern = new Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");

            return !objNotNumberPattern.IsMatch(strNumber) &&
                   !objTwoDotPattern.IsMatch(strNumber) &&
                   !objTwoMinusPattern.IsMatch(strNumber) &&
                   objNumberPattern.IsMatch(strNumber);
        }

        public static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
        public static bool IsInt(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }
        public static bool IsUnsign(string value)
        {
            return Regex.IsMatch(value, @"^\d*[.]?\d*$");
        }
        public static String arrayToString(String[] values)
        {
            if (null == values) return "";

            StringBuilder buffer = new StringBuilder(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                buffer.Append(values[i]).Append(",");
            }
            if (buffer.Length > 0)
            {
                return buffer.ToString().Substring(0, buffer.Length - 1);
            }
            return "";
        }

    }
}