using Disney;
using fp4me.Web.Data;
using fp4me.Web.Data.Models;
using fp4me.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Services
{
    public class SMS
    {
        public long UserID { get; set; }
        public string AttractionName { get; set; }
        public DateTime Date { get; set; }
        public int NumberOfPeople { get; set; }
        public string SMSNotificationPhoneNumber { get; set; }
    }

    public class fp4meServices
    {
        IDisneyAPI _disneyAPI;
        DisneyAPISettings _disneySettings;
        FacadeContext _facade;
        AppSettings _appSettings;

        public fp4meServices(FacadeContext facade, IOptions<AppSettings> appSettings, IDisneyAPI disneyAPI, IOptions<DisneyAPISettings> disneySettings)
        {
            _disneyAPI = disneyAPI;
            _disneySettings = disneySettings.Value;
            _facade = facade;
            _appSettings = appSettings.Value;
        }

        public async Task<IList<AttractionFastPassRequestCheck>> ProcessAttractionFastPassRequestsThatAreDueAsync(long[] restrictToJustTheseAttractionFastPassRequestIDs, long? userID)
        {
            var processingTimestamp = DateTime.UtcNow;
            var allAttractionFastPassRequestChecks = new List<AttractionFastPassRequestCheck>();

            // get attractions, dates, and number of people combos that are due to be checked
            var attractionsThatAreDueToBeChecked = _facade.GetAttractionsThatAreDueToBeChecked(restrictToJustTheseAttractionFastPassRequestIDs, userID).ToList();
            var attractions = _facade.db.Attractions.Select(p => new { p.Name, p.AttractionID }).ToList();

            if (attractionsThatAreDueToBeChecked.Any())
            {
                // sign in to the disney API
                await _disneyAPI.LoginAsync(_disneySettings.Username, _disneySettings.Password);

                var uniqueDates = attractionsThatAreDueToBeChecked.GroupBy(p => p.Date).Select(p => p.Key).Distinct().OrderBy(p => p);

                foreach (var date in uniqueDates)
                {
                    // put the items in order:
                    //      first - put priority attractions, ordered by number of people ascending
                    //      then - put non priority attractions, ordered by number of people descending
                    // why?
                    //      Because we're going to process the priority ones by checked for availability
                    //      of the lowest number of people first. Since priority attractions are likely to 
                    //      to have no availability, we can assume that if it's not available for 1 person
                    //      then it's also not available for 4 people, so we can check the 1 first and 
                    //      possibly skip the check for 4. 
                    //      And vice versa for non priority attractions. Start those by checking the high
                    //      number first - i.e. check for 4 people. If it's available for 4 people, we 
                    //      will assume it's also available for 1, 2, and 3 people, and we'll skip those.
                    var attractionsThatAreDueToBeCheckedOrderedByPriorityAttractionsFirstAndNonPriorityAttractionsSecond = attractionsThatAreDueToBeChecked.Where(p => p.Date == date && p.AttractionPriority != null).OrderBy(p => p.AttractionID).ThenBy(p => p.NumberOfPeople).ToList();
                    attractionsThatAreDueToBeCheckedOrderedByPriorityAttractionsFirstAndNonPriorityAttractionsSecond.AddRange(attractionsThatAreDueToBeChecked.Where(p => p.Date == date && p.AttractionPriority == null).OrderBy(p => p.AttractionID).ThenByDescending(p => p.NumberOfPeople).ToList());
                    
                    var uniqueParkIDsForThisDate = attractionsThatAreDueToBeCheckedOrderedByPriorityAttractionsFirstAndNonPriorityAttractionsSecond.GroupBy(p => p.DisneyApiParkID).Select(p => p.Key).Distinct().OrderBy(p => p);

                    foreach (var parkID in uniqueParkIDsForThisDate)
                    {
                        // setup some lists that will be used to track things
                        var sms = new List<SMS>();
                        var attractionFastPassRequestChecks = new List<AttractionFastPassRequestCheck>();

                        // check each one
                        foreach (var item in attractionsThatAreDueToBeCheckedOrderedByPriorityAttractionsFirstAndNonPriorityAttractionsSecond.Where(p => p.DisneyApiParkID == parkID).ToList())
                        {
                            try
                            {
                                // if we already have some AVAILABLE results for the same attraction & date
                                // where the number of people is MORE THAN the one we're about to check, just assume
                                // the one we're about to check is available too.
                                // For example, if Barnstormer is available on 2/15 for 4 people then we can assume it's
                                // also available on 2/15 for 2 people.
                                if (attractionFastPassRequestChecks.Any(p =>
                                    p.AttractionFastPassStatus == "AVAILABLE"
                                    && p.Date == item.Date
                                    && p.AttractionID == item.AttractionID
                                    && p.NumberOfPeople >= item.NumberOfPeople))
                                    continue;

                                // if we already have some NOT AVAILABLE results for the same attraction & date
                                // where the number of people is LESS THAN the one we're about to check, just assume
                                // the one we're about to check is not available too.
                                // For example, if 7DMT is not available on 2/15 for 1 person then we can assume it's
                                // also not available on 2/15 for 3 people.
                                if (attractionFastPassRequestChecks.Any(p =>
                                    p.AttractionFastPassStatus != "AVAILABLE"
                                    && p.Date == item.Date
                                    && p.AttractionID == item.AttractionID
                                    && p.NumberOfPeople <= item.NumberOfPeople))
                                    continue;

                                // check the status on disney's API
                                var statuses = await _disneyAPI.GetFastpassStatus(item.Date, "07:00:00", "23:00:00", item.DisneyApiParkID, item.NumberOfPeople);

                                foreach (var status in statuses)
                                {
                                    var attraction = attractions.FirstOrDefault(p => p.Name == status.ExperienceName);

                                    if (attraction == null)
                                        continue;

                                    attractionFastPassRequestChecks.Add(new AttractionFastPassRequestCheck
                                    {
                                        Date = item.Date,
                                        AttractionID = attraction.AttractionID,
                                        NumberOfPeople = item.NumberOfPeople,
                                        AttractionFastPassStatus = status.Availability,
                                        Timestamp = status.Timestamp
                                    });
                                }

                            }
                            catch (Exception ex)
                            {
                                // TODO1: exception logging!
                            }
                        }
                        _facade.db.SaveChanges();

                        // For any attraction & date that we found an AVAILABLE fastpass, 
                        // update all requests for the same attraction & date that had 
                        // number of people LESS THAN OR EQUAL TO the one we found.
                        // Example - if we found availability for Barnstormer on 2/15 for 3 people, update all requests
                        // for Barnstormer on 2/15 that were looking for 3 people or less

                        // To do this, we need to find the highest number of people for which a Fastpass
                        // was found to be available for any date and attraction combo.
                        // For example, if we found availability for:
                        //      2/12/2018 Barnstormer, 2 people
                        //      2/12/2018 Barnstormer, 4 people
                        // ... then we can assume that on 2/12/2018, Barnstormer is available for 
                        // 1, 2, 3, or 4 people since it's showing as available for 4.
                        // So, extract those max people values where the fastpass is available:
                        var availables = from a in attractionFastPassRequestChecks
                                         where a.AttractionFastPassStatus == "AVAILABLE"
                                         group a by new { a.AttractionID, a.Date }
                                         into g
                                         select new { g.Key.AttractionID, g.Key.Date, NumberOfPeople = g.Max(p => p.NumberOfPeople), Timestamp = g.Max(p => p.Timestamp) };

                        foreach (var item in availables)
                        {
                            try
                            {
                                // get all the requests that relate to this AVAILABLE result
                                var attractionFastPassRequests = from r in _facade.db.AttractionFastPassRequests
                                                                 join u in _facade.db.Users on r.UserID equals u.UserID
                                                                 join a in _facade.db.Attractions on r.AttractionID equals a.AttractionID
                                                                 join up in _facade.db.UserPlans on u.UserPlanID equals up.UserPlanID
                                                                 where r.AttractionID == item.AttractionID
                                                                 && r.Date == item.Date
                                                                 && r.NumberOfPeople <= item.NumberOfPeople
                                                                 && r.Status == AttractionFastPassRequestStatusEnum.Active
                                                                 select new { AttractionFastPassRequest = r, User = u, Attraction = a, UserPlan = up };

                                // for each request that matches this AVAILABLE result, 
                                // 1) update the "last check timestamp" and "last check status", and
                                // 2) record that we need to send an SMS (we'll do the actual sending later)
                                foreach (var attractionFastPassRequest in attractionFastPassRequests)
                                {
                                    var persistedRequest = _facade.db.AttractionFastPassRequests.Single(p => p.AttractionFastPassRequestID == attractionFastPassRequest.AttractionFastPassRequest.AttractionFastPassRequestID);
                                    persistedRequest.LastCheckTimestamp = item.Timestamp;
                                    persistedRequest.LastCheckStatus = "AVAILABLE";
                                    var lastNotificationTimestamp = attractionFastPassRequest.AttractionFastPassRequest.LastNotificationTimestamp;
                                    // if the user has NOT been notified for this request yet (a.k.a. last notification timestamp is null or doesn't have a value)
                                    // OR if it's been more than PauseFrequencyInMinutes minutes since the last notification timestamp,
                                    // (1) update the last notification timestamp and 
                                    // (2) send the user an SMS
                                    if (!lastNotificationTimestamp.HasValue
                                        ||
                                        lastNotificationTimestamp.Value.AddMinutes(attractionFastPassRequest.UserPlan.PauseFrequencyInMinutes - 1) <= DateTime.UtcNow)
                                    {
                                        // (1) 
                                        persistedRequest.LastNotificationTimestamp = item.Timestamp;
                                        // (2) 
                                        sms.Add(new SMS
                                        {
                                            AttractionName = attractionFastPassRequest.Attraction.Name,
                                            UserID = attractionFastPassRequest.User.UserID,
                                            SMSNotificationPhoneNumber = attractionFastPassRequest.User.SMSNotificationPhoneNumber,
                                            Date = attractionFastPassRequest.AttractionFastPassRequest.Date,
                                            NumberOfPeople = attractionFastPassRequest.AttractionFastPassRequest.NumberOfPeople
                                        });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //TODO1: exception logging
                            }
                        }
                        _facade.db.SaveChanges();

                        // Send SMS
                        sms = (from s in sms
                               group s by new { s.AttractionName, s.Date, s.UserID, s.SMSNotificationPhoneNumber }
                               into grouped
                               select new SMS
                               {
                                   AttractionName = grouped.Key.AttractionName,
                                   Date = grouped.Key.Date,
                                   UserID = grouped.Key.UserID,
                                   SMSNotificationPhoneNumber = grouped.Key.SMSNotificationPhoneNumber,
                                   NumberOfPeople = grouped.Max(p => p.NumberOfPeople)
                               }).ToList();

                        foreach (var s in sms)
                        {
                            try
                            {
                                await _facade.SendTextMessageAsync(s.SMSNotificationPhoneNumber,
                                    Smart.Format(Messages.ATTRACTION_FASTPASS_REQUEST_IS_AVAILABLE, new
                                    {
                                        attractionName = s.AttractionName,
                                        date = s.Date.ToLocalTime().ToString("MM/dd/yyyy"),
                                        guests = s.NumberOfPeople == 1 ? "1 guest" : "up to " + s.NumberOfPeople + " guests"
                                    }));
                            }
                            catch (Exception ex)
                            {
                                //TODO1: exception logging!
                            }
                        }

                        // For any attraction & date that we found an UNAVAILABLE fastpass, 
                        // update all requests for the same attraction & date that had 
                        // number of people GREATER THAN OR EQUAL TO the one we found.
                        // Example - if we found "not available" for 7DMT on 2/15 for 2 people, update all requests
                        // for 7DMT on 2/15 that were looking for 2 people or more
                        var unavailables = from a in attractionFastPassRequestChecks
                                           where a.AttractionFastPassStatus != "AVAILABLE"
                                           group a by new { a.AttractionID, a.Date, a.AttractionFastPassStatus }
                                           into g
                                           select new { g.Key.AttractionID, g.Key.Date, NumberOfPeople = g.Min(p => p.NumberOfPeople), Timestamp = g.Max(p => p.Timestamp) };

                        foreach (var item in unavailables)
                        {
                            try
                            {
                                // get all the requests that relate to this UNAVAILABLE result
                                var attractionFastPassRequests = from r in _facade.db.AttractionFastPassRequests
                                                                 where r.AttractionID == item.AttractionID
                                                                 && r.Date == item.Date
                                                                 && r.NumberOfPeople >= item.NumberOfPeople
                                                                 && r.Status == AttractionFastPassRequestStatusEnum.Active
                                                                 select r;

                                // for each request that matches this UNAVAILABLE result, update its 
                                // "last check" attributes
                                foreach (var attractionFastPassRequest in attractionFastPassRequests)
                                {
                                    attractionFastPassRequest.LastCheckTimestamp = item.Timestamp;
                                    attractionFastPassRequest.LastCheckStatus = "UNAVAILABLE";
                                }
                            }
                            catch (Exception ex)
                            {
                                //TODO1: exception logging
                            }
                        }
                        _facade.db.SaveChanges();

                        // save all the results
                        foreach (var result in attractionFastPassRequestChecks)
                        {
                            try
                            {
                                // before saving AVAILABLE check results, see if there's a check being saved 
                                // for the same status/attraction/date but with a higher number of people; 
                                // if there is, ignore this one and just save the one that's AVAILABLE with the highest number of people
                                if (result.AttractionFastPassStatus.Equals("AVAILABLE") && attractionFastPassRequestChecks.Any(p => p.AttractionFastPassStatus == result.AttractionFastPassStatus && p.Date == result.Date && p.AttractionID == result.AttractionID && p.NumberOfPeople > result.NumberOfPeople))
                                    continue;

                                // before saving NOT AVAILABLE check results, see if there's a check being saved 
                                // for the same status/attraction/date but with a lower number of people; 
                                // if there is, ignore this one and just save the one that's NOT AVAILABLE with the lowest number of people
                                if (!result.AttractionFastPassStatus.Equals("AVAILABLE") && attractionFastPassRequestChecks.Any(p => p.AttractionFastPassStatus == result.AttractionFastPassStatus && p.Date == result.Date && p.AttractionID == result.AttractionID && p.NumberOfPeople < result.NumberOfPeople))
                                    continue;

                                _facade.db.AttractionFastPassRequestChecks.Add(result);
                            }
                            catch (Exception ex)
                            {
                                //TODO2: log errors!!!!
                            }
                        }
                        _facade.db.SaveChanges();

                        // TODO2: if we processed the first check for a user and that user did not
                        // get an AVAILABLE message, send them a message to say that we are checking for them

                        allAttractionFastPassRequestChecks = allAttractionFastPassRequestChecks.Union(attractionFastPassRequestChecks).ToList();
                    }
                }

            }
            return allAttractionFastPassRequestChecks;
        }


    }
}
