using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models
{
    public enum AttractionFastPassRequestStatusEnum { Active, Deleted, Paused }

    public class AttractionFastPassRequest
    {
        public long AttractionFastPassRequestID { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime Date { get; set; }
        public int NumberOfPeople { get; set; }
        public long AttractionID { get; set; }

        public long UserID { get; set; }
        public string LastCheckStatus { get; set; }
        public DateTime? LastCheckTimestamp { get; set; }
        public DateTime? LastNotificationTimestamp { get; set; }

        public User User { get; set; }
        public Attraction Attraction { get; set; }

        public AttractionFastPassRequestStatusEnum Status { get; set; }

        public AttractionFastPassRequest()
        {
            Status = AttractionFastPassRequestStatusEnum.Active;
        }

    }
}
