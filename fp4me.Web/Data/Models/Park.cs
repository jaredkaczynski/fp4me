using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models
{
    public class Park
    {
        public long ParkID { get; set; }

        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string DisneyApiParkID { get; set; }

        public IList<Attraction> Attractions { get; set; }
    }
}
