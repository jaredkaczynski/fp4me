using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models.HomeViewModels
{
    public class HomeViewModel
    {
        public string Park { get; set; }

        public IDictionary<long, string> Attractions { get; set; }
        public IEnumerable<Tuple<long, long, string>> PriorityAttractions { get; set; }

        [Required(ErrorMessage = "Attraction is required.")]
        public long AttractionID { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = @"{0: M/d/yyyy}")]
        [Required]
        public DateTime Date { get; set; }

        [Range(1, 4)]
        [Required]
        public int NumberOfPeople { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number", Prompt = "your phone number (U.S. only)")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid Phone number")]
        public string SMSNotificationPhoneNumber { get; set; }

        public HomeViewModel()
        {
            NumberOfPeople = 2;
            Date = System.DateTime.Now.AddDays(1);
        }
    }
}
