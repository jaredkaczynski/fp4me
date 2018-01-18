using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models.IQueryable
{
    public class GetUsersWithStatisticsModel
    {
        public Data.Models.User User { get; set; }
        public int NumberOfAttractionFastPassRequests { get; set; }
        public int NumberOfActiveAttractionFastPassRequests { get; set; }
        public int NumberOfDeletedAttractionFastPassRequests { get; set; }
    }
}
