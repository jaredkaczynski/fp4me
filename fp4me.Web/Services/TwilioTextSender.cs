using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace fp4me.Web.Services
{
    public class TwilioSettings
    {
        public string AuthToken { get; set; }
        public string AccountSID { get; set; }
        public string FromPhoneNumber { get; set; }
    }

    public class TwilioTextSender : ITextSender
    {

        public TwilioSettings Settings { get; set; }

        public TwilioTextSender(IOptions<TwilioSettings> settings)
        {
            Settings = settings.Value;
        }

        public Task SendAsync(string phoneNumber, string message)
        {
            TwilioClient.Init(Settings.AccountSID, Settings.AuthToken);
            
            var textMessage = MessageResource.Create(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(Settings.FromPhoneNumber),
                body: message);
            
            return Task.CompletedTask;
        }
    }
}
