using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace fp4me.Web
{
    /*
        This is probably a terrible way to handle string constants but it works for now.
    */

    public static class Messages
    {
        public const string DELETE_SUCCESS = "Delete success.";
        public const string INVALID_AUTH_LINK = "Invalid link. Please try signing in manually.";

        public const string USER_BY_PHONE_NUMBER_DOESNT_EXIST = "The user for phone number {0} doesn't exist.";
        public const string USER_BY_PHONE_NUMBER_ALREADY_EXISTS = "{0} already exists. Sign in as that user to create and manage your checks, or go back and use a different phone number.";

        public const string SAVED = "Saved.";
        public const string CHECK_SAVED_BUT_SIGN_IN_REQUIRED_TO_MANAGE_IT = "Check saved but you need to sign in to manage it.";

        public const string ATTRACTION_FASTPASS_REQUEST_IS_AVAILABLE = "FP AVAILABLE! {attractionName} - {date}, {guests}";

        public const string SIGN_IN_LINK_MESSAGE_FOR_ACTIVE_USER = "{code} is your fp4me sign in code. Click here to sign in from mobile: {url}";
        public const string SIGN_IN_LINK_MESSAGE_FOR_INACTIVE_USER = "{code} is your fp4me activation code. Click here to activate and let's find some fastpasses! {url}";
        public const string SIGN_IN_LINK_WELCOME_NEW_USER = "Welcome! {code} is your fp4me activation code. Click here to activate from mobile: {url}";
        public const string SIGN_IN_LINK_WELCOME_NEW_USER_AND_CHECK_CREATED = "Welcome! {code} is your fp4me activation code. Click here to activate from mobile: {url}";
        
        public const string OPT_OUT_MESSAGE = "If you aren't expecting these messages from fp4me, click here to opt-out and we'll stop sending them: {0}";

        public const string SMS_OPT_OUT_SUCCESSFUL = "Opt-out successful for {0}.";

        public const string ACTIVATION_SUCCESSFUL = "Congrats! Your account has been activated.";
        
    }
}
