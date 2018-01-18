using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models
{
    public class AttractionFastPassRequestCheck
    {
        public long AttractionFastPassRequestCheckID { get; set; }

        public DateTime Date { get; set; }
        public long AttractionID { get; set; }
        public int NumberOfPeople { get; set; }
        
        public DateTime Timestamp { get; set; }
        public string AttractionFastPassStatus { get; set; }

        public Attraction Attraction { get; set; }
    }
}
