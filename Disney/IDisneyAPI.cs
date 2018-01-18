using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Disney
{
    public interface IDisneyAPI
    {
        Task<List<ExperienceFastpassAvailability>> GetFastpassStatus(DateTime date, string startTime, string endTime, string experienceParkID, int numberOfGuests);
        Task<string> LoginAsync(string username, string password);
    }
}