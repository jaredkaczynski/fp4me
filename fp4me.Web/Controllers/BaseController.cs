using fp4me.Web.Data;
using fp4me.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Controllers
{
    [SetTempDataModelState]
    [RestoreModelStateFromTempData]
    public class BaseController : Controller
    {
        protected readonly FacadeContext facade;
        protected readonly AppSettings settings;

        public BaseController(FacadeContext facadeContext, IOptions<AppSettings> appSettings)
        {
            facade = facadeContext;
            settings = appSettings.Value;
        }
    }
}
