using fp4me.Web;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        private const string TIMEZONE_OFFSET_ITEM_KEY = "timezoneOffset";
        private const string NEW_FP_CHECK_PAGE_KEY = "IsAPageForCreatingNewFastPassChecks";

        public static void ThisPageIsUsedForCreatingNewFastPassChecks(this HttpContext httpContext, bool value)
        {
            if (httpContext.Items.Keys.Contains(NEW_FP_CHECK_PAGE_KEY))
                httpContext.Items.Remove(NEW_FP_CHECK_PAGE_KEY);

            httpContext.Items.Add(NEW_FP_CHECK_PAGE_KEY, value);
        }
        
        public static bool ThisPageIsUsedForCreatingNewFastPassChecks(this HttpContext httpContext)
        {
            if (!httpContext.Items.Keys.Contains(NEW_FP_CHECK_PAGE_KEY))
                return false;

            return (bool)httpContext.Items[NEW_FP_CHECK_PAGE_KEY];
        }

        public static void SetTimezoneOffset(this HttpContext httpContext, long offsetInMinutes)
        {
            httpContext.Items.Add(TIMEZONE_OFFSET_ITEM_KEY, offsetInMinutes);
        }

        public static long TimezoneOffset(this HttpContext httpContext)
        {
            var offset = httpContext.Items[TIMEZONE_OFFSET_ITEM_KEY];
            if (offset == null)
                return Convert.ToInt64(TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
            else
                return Convert.ToInt64(offset);
        }

        public static AppUser AppUser(this HttpContext httpContext)
        {
            AppUser appUser = null;

            if (httpContext.Items.Keys.Contains("AppUser"))
                appUser = (AppUser)httpContext.Items["AppUser"];

            if (appUser == null) {
                appUser = new AppUser();
                httpContext.Items.Add("AppUser", appUser);
            }

            return appUser;
        }
    }
}
