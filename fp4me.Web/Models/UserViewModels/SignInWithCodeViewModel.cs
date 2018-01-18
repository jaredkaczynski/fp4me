using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models.UserViewModels
{
    public class SignInWithCodeViewModel
    {
        public string PhoneNumber { get; set; }
        public string AccessToken { get; set; }
        public bool IsActivated { get; set; }
    }
}
