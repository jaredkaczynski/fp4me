using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models
{
    public class WaitlistCode
    {
        [Key]
        public string Code { get; set; }
        public bool Used { get; set; }
    }
}
