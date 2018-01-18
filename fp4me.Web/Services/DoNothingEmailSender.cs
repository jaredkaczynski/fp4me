using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Services
{
    public class DoNothingTextSender : ITextSender
    {
        public Task SendAsync(string phoneNumber, string message)
        {
            return Task.CompletedTask;
        }
    }
}
