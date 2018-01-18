using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models.StoredProcedures
{
    public class GetMostRecentChecksForAttractionFastRequests
    {
        public long AttractionFastPassRequestID { get; set; }
        public long AttractionFastPassRequestCheckID { get; set; }

        public DateTime Date { get; set; }
        public long AttractionID { get; set; }
        public int NumberOfPeople { get; set; }

        public DateTime Timestamp { get; set; }
        public string AttractionFastPassStatus { get; set; }

        public Attraction Attraction { get; set; }
    }
}
