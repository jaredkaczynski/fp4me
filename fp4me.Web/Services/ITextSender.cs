using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Services
{
    public interface ITextSender
    {
        Task SendAsync(string phoneNumber, string message);
    }
}
