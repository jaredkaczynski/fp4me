using fp4me.Web.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using fp4me.Web.Data.Models.IQueryable;
using fp4me.Web.Data.Models;

namespace fp4me.Web
{
    public class UserTimezoneOffset
    {
        private readonly RequestDelegate _next;

        public UserTimezoneOffset(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                var timezoneOffset = context.Request.Cookies["timezoneOffset"];
                context.SetTimezoneOffset(long.Parse(timezoneOffset));
            } catch { }
            await _next(context);
        }
    }

    public static class UserTimezoneOffsetExtensions
    {
        public static IApplicationBuilder UseUserTimezoneOffset(this IApplicationBuilder app)
        {
            return app.UseMiddleware<UserTimezoneOffset>();
        }
    }
}
