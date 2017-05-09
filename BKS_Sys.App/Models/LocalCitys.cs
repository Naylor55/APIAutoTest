using BKS_Sys.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BKS_Sys.App.Models
{
    public static class LocalCitys
    {

        public static List<Citys> cityList;
        static LocalCitys()
        {
        
            if (cityList == null)
            {
                BookoesDBEntities db = new BookoesDBEntities();
                cityList = db.Citys.Where(x => 1 == 1).ToList();
            }            
        }
    }

}