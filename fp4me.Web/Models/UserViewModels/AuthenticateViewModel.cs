using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models.UserViewModels
{
    public class AuthenticateViewModel
    {
        [Required]
        public string AccessToken { get; set; }

        public bool? IsForActivation { get; set; }

        public AuthenticateViewModel() { }

        public AuthenticateViewModel(string accessToken)
        {
            AccessToken = accessToken;
        }
    }
}
