using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Disney
{
    public class DisneyAPITester : IDisneyAPI
    {
        private List<ExperienceFastpassAvailability> _sampleResults = null;
        
        private async Task<string> GetSampleDataAsync()
        {
            var resourceStream = typeof(Disney.DisneyAPITester).GetTypeInfo().Assembly.GetManifestResourceStream("Disney.sample.json");
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task<List<ExperienceFastpassAvailability>> GetFastpassStatus(DateTime date, string startTime, string endTime, string experienceParkID, int numberOfGuests)
        {
            if (_sampleResults == null)
            {
                _sampleResults = (new Helpers()).ExtractFastpassStatus(await GetSampleDataAsync());
                _sampleResults.ForEach(p => p.Timestamp = DateTime.UtcNow);
                _sampleResults.ForEach(p => p.NumberOfGuests = numberOfGuests);
                _sampleResults.ForEach(p => p.ParkID = experienceParkID);
                _sampleResults.ForEach(p => p.Date = date);
            }
            return _sampleResults;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            return username;
        }
    }
}
