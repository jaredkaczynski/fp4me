using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models.StoredProcedures
{
    public class GetAttractionFastPassRequestsWithStatistics
    {
        public long UserID { get; set; }
        public long AttractionID { get; set; }
        public long AttractionFastPassRequestID { get; set; }
        public AttractionFastPassRequestStatusEnum Status { get; set; }
        public string ParkName { get; set; }
        public string AttractionName { get; set; }
        public DateTime Date { get; set; }
        public long NumberOfChecks { get; set; }
        public long NumberOfAvailableChecks { get; set; }
        public DateTime? LastAvailableCheck { get; set; }
        public DateTime? LastCheckDate { get; set; }
        public string SMSNotificationPhoneNumber { get; set; }
        public long NumberOfPeople { get; set; }
        public string LastCheckStatus { get; set; }
        public DateTime? LastCheckTimestamp { get; set; }
    }
}
