using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models
{
    public class User
    {
        public long UserID { get; set; }

        public string SMSNotificationPhoneNumber { get; set; }
        public string AccessToken { get; set; }
        public long UserPlanID { get; set; }
        public UserPlan UserPlan { get; set; }
        public short NumberOfSuccessiveSignInLinkAttempts { get; set; }
        public bool HasOptedOutOfSMS { get; set; }
        public bool IsActivated { get; set; }
        public bool IsAdmin { get; set; }
    }
}
