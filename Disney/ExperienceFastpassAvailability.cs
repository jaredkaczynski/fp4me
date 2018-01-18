using System;
using System.Collections.Generic;
using System.Text;

namespace Disney
{
    public class ExperienceFastpassAvailability
    {
        public string ExperienceName { get; set; }
        public string ParkID { get; set; }
        public string ExperienceID { get; set; }
        public string Availability { get; set; }
        public DateTime Date { get; set; }
        public int NumberOfGuests { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
