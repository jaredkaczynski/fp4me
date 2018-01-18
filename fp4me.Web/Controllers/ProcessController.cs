using Disney;
using fp4me.Web.Data;
using fp4me.Web.Data.Models;
using fp4me.Web.Models;
using fp4me.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Controllers
{
    public class ProcessController : BaseController
    {

        public ProcessController(FacadeContext facade, IOptions<AppSettings> appSettings, IDisneyAPI disneyAPI, IOptions<DisneyAPISettings> disneySettings) : base(facade, appSettings)
        {
        }

        /// <summary>
        /// This must be called with an access token that matches a valid user that's an admin.
        /// </summary>
        /// <param name="accessToken">An admin's access token</param>
        /// <returns></returns>
        public async Task<IActionResult> AttractionFastPassRequests(string accessToken)
        {
            try
            {
                var user = facade.db.Users.FirstOrDefault(p => p.AccessToken == accessToken && p.IsAdmin);

                if (user == null)
                    return Unauthorized();

                var services = new fp4meServices(this.facade, (IOptions<AppSettings>)this.HttpContext.RequestServices.GetService(typeof(IOptions<AppSettings>)), (IDisneyAPI)this.HttpContext.RequestServices.GetService(typeof(IDisneyAPI)), (IOptions<DisneyAPISettings>)this.HttpContext.RequestServices.GetService(typeof(IOptions<DisneyAPISettings>)));
                var results = await services.ProcessAttractionFastPassRequestsThatAreDueAsync(null, null);
                return Ok(results);
            }
            catch
            {
                //TODO1: exception handling
                return Ok();
            }
        }
    }
}
