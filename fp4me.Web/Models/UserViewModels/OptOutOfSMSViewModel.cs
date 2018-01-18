using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models.UserViewModels
{
    public class OptOutOfSMSViewModel
    {
        public string PhoneNumber { get; set; }
        public string OptOutToken { get; set; }
    }
}
