//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace BKS_Sys.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class WorkFlowAnnex
    {
        public int AnnexID { get; set; }
        public System.Guid RunID { get; set; }
        public string AnnexName { get; set; }
        public string AnnexUrl { get; set; }
        public string AnnexSize { get; set; }
        public bool IsDelete { get; set; }
    
        public virtual WorkFlowRun WorkFlowRun { get; set; }
    }
}
