using fp4me.Web.Data;
using fp4me.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Controllers
{
    public class DataController : BaseController
    {
        public DataController(FacadeContext facade, IOptions<AppSettings> appSettings) : base(facade, appSettings) { }

        public IActionResult GetAttractionFastPassRequestsToProcessAsJson()
        {
            var list = facade.GetAttractionsThatAreDueToBeChecked(null, null)
                .OrderBy(p => p.Date)
                .ThenBy(p => p.DisneyApiParkID)
                .ThenBy(p => p.AttractionID)
                .ThenBy(p => p.NumberOfPeople)
                .ToList();
            return new ObjectResult(list);
        }

        public IActionResult GetAttractionFastPassRequestsWithStatisticsAsJson()
        {
            var list = facade.GetAttractionFastPassRequestsWithStatistics(this.HttpContext.AppUser().UserID).ToList();
            facade.db.SaveChanges();
            return new ObjectResult(list);
        }

    }
}
