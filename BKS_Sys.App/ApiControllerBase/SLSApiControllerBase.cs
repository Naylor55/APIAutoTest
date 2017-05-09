using BKS_Sys.App.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace BKS_Sys.App.Filter
{
    public class Context
    {
        public object this[object key]
        {
            get
            {
                return HttpContext.Current.Items[key];
            }
        }
    }
    public class SLSApiControllerBase : ApiController
    {
        public Context Context { get; set; }
        public SLSApiControllerBase()
        {
            Context = new Context();
        }

        public IHttpActionResult Error(String errMsg)
        {
            ResponseBody<Object> body = new ResponseBody<Object>();
            body.Result = new Result();
            body.Result.Code = "201";
            body.Result.Msg = errMsg;
            return base.Ok(body);
        }
        public IHttpActionResult Ok(String okMsg, Object t)
        {
            ResponseBody<Object> body = new ResponseBody<Object>();
            body.Result = new Result();
            body.Result.Code = "200";
            body.Result.Msg = okMsg;
            body.Body = t;
            return base.Ok(body);
        }
        public IHttpActionResult Ok(Object t)
        {
            return Ok("成功",t);
        }
    }
}
