using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Models.UserViewModels
{
    public class SignInLinkSentViewModel
    {
        public string SMSNotificationPhoneNumber { get; set; }
        public string AccessToken { get; set; }
        public bool IsNewUser { get; set; }
        public bool IsActivatedUser { get; set; }
        public bool NewAttractionFastPassRequestWasSaved { get; set; }

        public SignInLinkSentViewModel()
        {
            IsNewUser = false;
            NewAttractionFastPassRequestWasSaved = false;
            IsActivatedUser = false;
        }

        public SignInLinkSentViewModel(string accessToken, bool isNewUser, bool isActivatedUser, bool newAttractionFastPassRequestWasSaved)
        {
            SMSNotificationPhoneNumber = accessToken;
            IsNewUser = isNewUser;
            IsActivatedUser = isActivatedUser;
            NewAttractionFastPassRequestWasSaved = newAttractionFastPassRequestWasSaved;
        }
    }
}
