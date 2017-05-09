using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using BKS_Sys.App.Filter;
using Newtonsoft.Json;

namespace BKS_Sys.App
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务
            // 将 Web API 配置为仅使用不记名令牌身份验证。
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            ///注册webapi的Attribute
            config.Filters.Add(new RequestDataParseFilter());
            //注册APILOG的
            config.Filters.Add(new ExceptionFillters());
            
            //这个设置就可以将结构中的null序列化成""
            config.Formatters.JsonFormatter.SerializerSettings =  new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
