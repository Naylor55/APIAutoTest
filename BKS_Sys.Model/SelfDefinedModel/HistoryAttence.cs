using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKS_Sys.Model.SelfDefinedModel
{
    /// <summary>
    /// 历史考勤信息
    /// </summary>
    public class HistoryAttence
    {

        public a am1 { get; set; }
        public a am2 { get; set; }
        public a pm1 { get; set; }
        public a pm2 { get; set; }
    }
   public class a
    {
        public string time { get; set; }
        public string position { get; set; }
        public string state { get; set; }
        public string msg { get; set; }
    }

   
}
