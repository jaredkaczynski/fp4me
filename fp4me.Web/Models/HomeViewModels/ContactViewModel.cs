using fp4me.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models.HomeViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Message is required.")]
        public string Message { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email Address is required.")]
        public string EmailAddress { get; set; }

        public string Focus { get; set; }
    }
}
