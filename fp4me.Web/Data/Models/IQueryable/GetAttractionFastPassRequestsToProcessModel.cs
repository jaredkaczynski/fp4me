using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models.IQueryable
{
    public class GetAttractionsThatAreDueToBeCheckedModel
    {
        public long AttractionID { get; set; }
        public int? AttractionPriority { get; set; }
        public DateTime Date { get; set; }
        public int NumberOfPeople { get; set; }
        public string DisneyApiParkID { get; set; }
        public string AttractionName { get; set; }
    }
}
