using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models
{
    public class UserPlan
    {
        public long UserPlanID { get; set; }

        public int CheckFrequencyInMinutes { get; set; }
        public int PauseFrequencyInMinutes { get; set; }
        public string Name { get; set; }
        public int MaxAllowedActiveAttractionFastPassRequests { get; set; }
        public bool IsPublic { get; set; }
    }
}
