using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models
{
    public class Attraction
    {
        public long AttractionID { get; set; }

        public string Name { get; set; }
        public int? Priority { get; set; }

        public Park Park { get; set; }
        public long ParkID { get; set; }

    }
}
