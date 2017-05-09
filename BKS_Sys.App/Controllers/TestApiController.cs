using BKS_Sys.App.Filter;
using BKS_Sys.App.Models;
using BKS_Sys.App.Models.Base;
using BKS_Sys.Model;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace BKS_Sys.App.ApiControllers
{
    public class  TestApiController : COApiControllerBase
    {
        Entities ent = new Entities();

        #region 用户登录
        [Route("api/test/test")]
        [HttpPost]
        public IHttpActionResult test()
        {
            return Error("用户名或密码错误，请重新登录！");
        }
        #endregion

    }
}
