using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models.UserViewModels
{
    public class SignInViewModel
    {
        [Required(ErrorMessage = "Phone Number is required.")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number", Prompt = "(888) 555-1212")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Must be a valid U.S. Phone Number")]
        public string PhoneNumber { get; set; }

        public string AccessToken { get; set; }

        public string WaitlistCode { get; set; }
    }
}
