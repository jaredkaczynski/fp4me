using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models
{
    public class SignInLink
    {
        public Uri Url { get; set; }
        public string AccessToken { get; set; }
    }
}
