using fp4me.Web.Controllers;
using fp4me.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web
{
    public abstract class ModelStateTransfer : ActionFilterAttribute
    {
        protected const string Key = nameof(ModelStateTransfer);
    }

    public class ThisPageIsUsedForCreatingNewFastPassChecksAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            context.HttpContext.ThisPageIsUsedForCreatingNewFastPassChecks(true);
        }

    }

    public class SetTempDataModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //Only export when ModelState is not valid
            if (!filterContext.ModelState.IsValid)
            {
                //Export if we are redirecting
                if (filterContext.Result is RedirectResult
                    || filterContext.Result is RedirectToRouteResult
                    || filterContext.Result is RedirectToActionResult)
                {
                    var controller = filterContext.Controller as Controller;
                    if (controller != null && filterContext.ModelState != null)
                    {
                        var modelState = Helpers.SerialiseModelState(filterContext.ModelState);
                        controller.TempData[nameof(ModelStateTransfer)] = modelState;
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }

    public class RestoreModelStateFromTempDataAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            var serialisedModelState = controller?.TempData[nameof(ModelStateTransfer)] as string;

            if (serialisedModelState != null)
            {
                //Only Import if we are viewing
                if (filterContext.Result is ViewResult)
                {
                    var modelState = Helpers.DeserialiseModelState(serialisedModelState);
                    filterContext.ModelState.Merge(modelState);
                }
                controller.TempData.Remove(nameof(ModelStateTransfer));
            }

            base.OnActionExecuted(filterContext);
        }
    }

    public class SimpleAuthorizeAttribute : ActionFilterAttribute
    {
        public bool UserMustBeAdmin { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (
                (context.HttpContext.AppUser() == null) 
                || !context.HttpContext.AppUser().IsAuthenticated 
                || (UserMustBeAdmin && !context.HttpContext.AppUser().IsAdmin))
                context.Result = new RedirectToActionResult(nameof(UserController.SignIn), nameof(UserController).Replace("Controller", ""), null);
            
        }
    }
    
}
