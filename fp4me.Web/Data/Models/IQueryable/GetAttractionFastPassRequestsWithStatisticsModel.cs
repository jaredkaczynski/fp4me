using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Data.Models.IQueryable
{
    public class GetAttractionFastPassRequestsWithStatisticsModel
    {
        public AttractionFastPassRequest AttractionFastPassRequest { get; set; }
        public DateTime? LastCheckTimestamp { get; set; }
        public DateTime? LastAvailableResultTimestamp { get; set; }
        public DateTime NextCheck { get; set; }

        public int NumberOfChecks { get; set; }
        public int NumberOfAvailableResults { get; set; }

        public GetAttractionFastPassRequestsWithStatisticsModel() { }

        public GetAttractionFastPassRequestsWithStatisticsModel(AttractionFastPassRequest attractionFastPassRequest,
            User user,
            Attraction attraction,
            Park park,
            DateTime? lastChecked,
            DateTime nextCheck,
            DateTime? lastAvailableResult,
            int numberOfChecks,
            int numberOfAvailableResults)
        {
            this.AttractionFastPassRequest = attractionFastPassRequest;
            this.AttractionFastPassRequest.User = user;
            this.AttractionFastPassRequest.Attraction = attraction;
            this.AttractionFastPassRequest.Attraction.Park = park;
            this.LastCheckTimestamp = lastChecked;
            this.NextCheck = nextCheck;
            this.LastAvailableResultTimestamp = lastAvailableResult;
            this.NumberOfChecks = numberOfChecks;
            this.NumberOfAvailableResults = numberOfAvailableResults;
        }
    }
}
