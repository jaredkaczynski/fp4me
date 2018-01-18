using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models
{
    public class AppSettings
    {
        public string DefaultUserPlanName { get; set; }
        public long HowOftenTheSystemChecksAndProcessesAttractionFastPassRequestsInMinutes { get; set; }
        public bool AcceptingNewUsers { get; set; }
        public string ContactEmailFromAddress { get; set; }
        public string ContactEmailToAddress { get; set; }
        public string SupportEmailAddress { get; set; }
    }
}
