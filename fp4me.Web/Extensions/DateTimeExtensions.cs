using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToContextTimezone(this DateTime dateTime, HttpContext httpContext)
        {
            return dateTime.AddMinutes(httpContext.TimezoneOffset());
        }

        public static long ToUnixEpochInMilliseconds(this DateTime dateTime)
        {
            return (long)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }
    }
}
