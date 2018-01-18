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
    public class AppUser
    {
        public bool IsAuthenticated
        {
            get
            {
                return this.UserID > 0;
            }
        }

        public long UserID { get; set; }
        public string SMSNotificationPhoneNumber { get; set; }
        public bool IsActivated { get; set; }
        public bool IsAdmin { get; set; }
        public string AccessToken { get; set; }
        public GetUsersWithStatisticsModel UserStatistics { get; internal set; }
        public UserPlan UserPlan { get; internal set; }
    }

    public class SimpleUserIdentity
    {
        private readonly RequestDelegate _next;

        public SimpleUserIdentity(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var facade = (FacadeContext)context.RequestServices.GetService(typeof(FacadeContext));

            var accessToken = context.Request.Cookies["bearer"];
            if (accessToken != null)
            {
                var user = facade.db.Users.Include(nameof(UserPlan)).FirstOrDefault(p => p.AccessToken == accessToken);
                if (user != null)
                {
                    var userStats = facade.GetUserWithStatistics(user.UserID);
                    context.AppUser().UserID = user.UserID;
                    context.AppUser().IsActivated = user.IsActivated;
                    context.AppUser().SMSNotificationPhoneNumber = user.SMSNotificationPhoneNumber;
                    context.AppUser().IsAdmin = user.IsAdmin;
                    context.AppUser().AccessToken = user.AccessToken;
                    context.AppUser().UserStatistics = userStats;
                    context.AppUser().UserPlan = user.UserPlan;
                }
            }
            await _next(context);
        }
    }

    public static class SimpleUserIdentityExtensions
    {
        public static IApplicationBuilder UseSimpleUserIdentity(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SimpleUserIdentity>();
        }
    }
}
