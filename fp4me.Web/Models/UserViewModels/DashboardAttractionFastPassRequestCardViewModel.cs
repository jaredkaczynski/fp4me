using fp4me.Web.Data.Models;
using fp4me.Web.Data.Models.StoredProcedures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models.UserViewModels
{
    public class DashboardAttractionFastPassRequestCardViewModel
    {
        public GetAttractionFastPassRequestsWithStatistics GetAttractionFastPassRequestsWithStatistics { get; set; }
        public UserPlan UserPlan { get; set; }
        public IEnumerable<GetMostRecentChecksForAttractionFastRequests> RecentChecksForAttractionFastPassRequests { get; set; }
        public long? HowOftenTheSystemChecksAndProcessesAttractionFastPassRequestsInMinutes { get; set; }
    }
}
