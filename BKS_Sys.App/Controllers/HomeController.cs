using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Web.Mvc;

namespace BKS_Sys.App.Controllers
{

    public  class AttrMode
    {
        //action
        public string Template { get; set; }
        //描述
        public string Description { get; set; }
        public bool isEmpty(){
            return String.IsNullOrEmpty(Template)&&String.IsNullOrEmpty(Description);
        }
    }
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<AttrMode> ls = GetClasses("BKS_Sys.App.ApiControllers");
            ViewData["LIST"] = ls;
            ViewData["Type"] = "Index";
            return View();
        }

        public ActionResult DetailList(string ctr)
        {
            ViewData["Type"] = "DetailList";
            List<AttrMode> ls = GetAllActionByController("BKS_Sys.App.ApiControllers." + ctr);
            ViewData["LIST"] = ls;
            return View("Index");
        }

        public ActionResult Detail(string detail)
        {
            //"/api/users/login";
            ViewData["URL"] = "/"+detail;
            String viewName = detail.Substring(detail.LastIndexOf("/") + 1);
            return View(viewName);
        }

        //根据命名空间来取所有的controller
        static List<AttrMode> GetClasses(string nameSpace)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            List<AttrMode> namespacelist = new List<AttrMode>();
            foreach (Type type in asm.GetTypes())
            {
                if (type.Namespace == nameSpace) {
                    var attr = new AttrMode();
                    attr.Template = type.Name;
                    var v = type.GetCustomAttribute(typeof(DescriptionAttribute), false);
                    if (v!=null)
                    {
                        DescriptionAttribute vDes = (DescriptionAttribute)v;
                        attr.Description = vDes.Description;
                    }
                    if (!attr.isEmpty())
                    {
                        namespacelist.Add(attr);
                    }
                }
            }
            return namespacelist;
        }

        //根据controller来取Action
        static List<AttrMode> GetAllActionByController(string controller)
        {
            List<AttrMode> list = new List<AttrMode>();
            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Type type = assembly.GetType(controller);     //命名空间名 + 类名
            MethodInfo[] mi = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (MethodInfo m in mi)
            {
                if (m.ReturnType.Name == "IHttpActionResult")
                {
                    var attr = new AttrMode();
                    object[] t = m.GetCustomAttributes(typeof(System.Web.Http.RouteAttribute),false);
                    if (t != null && t.Length>0)
                    {
                        System.Web.Http.RouteAttribute r = (System.Web.Http.RouteAttribute)t[0];
                        attr.Template = r.Template;
                    }
                    object[] v = m.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (v != null && v.Length > 0)
                    {
                        DescriptionAttribute vDes = (DescriptionAttribute)v[0];
                        attr.Description = vDes.Description;
                    }
                    if (!attr.isEmpty()){
                        list.Add(attr);
                    }
                } 
            }
            return list;
        }


    }
}
