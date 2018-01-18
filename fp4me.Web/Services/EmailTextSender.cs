using System.Threading.Tasks;
using fp4me.Web.Services;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using fp4me.Web.Models;
using Helpers.Services;

public class EmailTextSender : ITextSender
{
    private IEmailSender _emailSender;
    private EmailTextSenderSettings _settings;

    public EmailTextSender(IEmailSender emailSender, IOptions<EmailTextSenderSettings> settings)
    {
        _emailSender = emailSender;
        _settings = settings.Value;
    }

    public Task SendAsync(string phoneNumber, string message)
    {
        return _emailSender.SendEmailAsync(_settings.FromAddress, _settings.ToAddress, string.Format("Message from fp4me - {0}", phoneNumber), message);
    }
    
}