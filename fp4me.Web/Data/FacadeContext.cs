using fp4me.Web.Data.Models;
using fp4me.Web.Data.Models.IQueryable;
using fp4me.Web.Data.Models.StoredProcedures;
using fp4me.Web.Models;
using fp4me.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace fp4me.Web.Data
{
    public class FacadeContext
    {
        public DataContext db { get; set; }
        public ITextSender _textSender { get; set; }
        public AppSettings settings { get; set; }
        //TODO1:filter checks to return only checks since the request was first saved
        public FacadeContext(DataContext dataContext, ITextSender textSender, IOptions<AppSettings> appSettings)
        {
            db = dataContext;
            _textSender = textSender;
            settings = appSettings.Value;
        }

        public void SaveChanges()
        {
            db.SaveChanges();
        }

        public async Task<bool> SendTextMessageAsync(string phoneNumber, string message)
        {
            var user = GetUserBySMSNotificationPhoneNumber(phoneNumber);
            if (user != null && user.HasOptedOutOfSMS)
            {
                return false;
            }
            else
            {
                await _textSender.SendAsync(phoneNumber, message);
                return true;
            }
        }

        public void SignalOptOut(string SMSNotificationPhoneNumber)
        {
            var user = db.Users.Single(p => p.SMSNotificationPhoneNumber == Helpers.CleanPhoneNumber(SMSNotificationPhoneNumber));
            user.HasOptedOutOfSMS = true;
            user.IsActivated = false;
            db.SaveChanges();
        }

        public void SignalActivation(User user)
        {
            user.IsActivated = true;
            user.HasOptedOutOfSMS = false;
        }

        public void SignalSignIn(long userID)
        {
            var user = db.Users.Single(p => p.UserID == userID);
            user.NumberOfSuccessiveSignInLinkAttempts = 0;
            user.HasOptedOutOfSMS = false;
            if (!user.IsActivated)
            {
                SignalActivation(user);
            }
            db.SaveChanges();
        }

        public void SignalSignInLinkSent(string SMSNotificationPhoneNumber)
        {
            var user = GetUserBySMSNotificationPhoneNumber(SMSNotificationPhoneNumber);
            user.NumberOfSuccessiveSignInLinkAttempts++;
            db.SaveChanges();
        }

        public User AddUser(User user)
        {
            user.SMSNotificationPhoneNumber = Helpers.CleanPhoneNumber(user.SMSNotificationPhoneNumber);
            if (user.UserPlan == null)
            {
                user.UserPlan = db.UserPlans.Single(p => p.Name == settings.DefaultUserPlanName);
            }
            user.HasOptedOutOfSMS = false;
            user.IsActivated = false;
            user.NumberOfSuccessiveSignInLinkAttempts = 0;
            db.Users.Add(user);
            return user;
        }

        public GetUsersWithStatisticsModel GetUserWithStatistics(long userID)
        {
            return GetUsersWithStatistics().Single(p => p.User.UserID == userID);
        }

        public GetUsersWithStatisticsModel GetUserWithStatisticsBySMSNotificationPhoneNumber(string SMSNotificationPhoneNumber)
        {
            SMSNotificationPhoneNumber = Helpers.CleanPhoneNumber(SMSNotificationPhoneNumber);
            return GetUsersWithStatistics().FirstOrDefault(p => p != null && p.User != null && p.User.SMSNotificationPhoneNumber == SMSNotificationPhoneNumber);
        }

        public User GetUserBySMSNotificationPhoneNumber(string SMSNotificationPhoneNumber)
        {
            SMSNotificationPhoneNumber = Helpers.CleanPhoneNumber(SMSNotificationPhoneNumber);
            return db.Users.FirstOrDefault(p => p.SMSNotificationPhoneNumber == SMSNotificationPhoneNumber);
        }

        public void DeleteAttractionFastPassRequest(long AttractionFastPassRequestID, long userID)
        {
            AttractionFastPassRequest a = db.AttractionFastPassRequests.Include(nameof(User)).First(p => p.AttractionFastPassRequestID == AttractionFastPassRequestID);
            if (userID == a.User.UserID)
            {
                a.Status = AttractionFastPassRequestStatusEnum.Deleted;
                db.SaveChanges();
            }
        }

        public IQueryable<GetUsersWithStatisticsModel> GetUsersWithStatistics()
        {
            return from u in db.Users
                   join a in db.AttractionFastPassRequests on u.UserID equals a.User.UserID
                   into joined
                   from a in joined.DefaultIfEmpty()
                   group a by new
                   {
                       User = u
                   } into grouped
                   select new GetUsersWithStatisticsModel
                   {
                       User = grouped.Key.User,
                       NumberOfActiveAttractionFastPassRequests = grouped.Count(p => p != null && p.Status == AttractionFastPassRequestStatusEnum.Active && p.Date >= DateTime.UtcNow.Date),
                       NumberOfDeletedAttractionFastPassRequests = grouped.Count(p => p != null && p.Status == AttractionFastPassRequestStatusEnum.Deleted),
                       NumberOfAttractionFastPassRequests = grouped.Count()
                   };
        }

        public List<GetMostRecentChecksForAttractionFastRequests> GetMostRecentChecksForAttractionFastRequests(long userID, int checksToReturnPerAttractionFastRequest)
        {
            var connection = this.db.Database.GetDbConnection();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "GetMostRecentChecksForAttractionFastRequests";
            command.Parameters.Add(new MySqlParameter("userID", userID));
            command.Parameters.Add(new MySqlParameter("attractionFastPassRequestStatus", AttractionFastPassRequestStatusEnum.Active));
            command.Parameters.Add(new MySqlParameter("checksToReturnPerAttractionFastRequest", checksToReturnPerAttractionFastRequest));
            var results = new List<GetMostRecentChecksForAttractionFastRequests>();
            var r = command.ExecuteReader();
            results = ToList<GetMostRecentChecksForAttractionFastRequests>(r);
            r.Close();
            connection.Close();
            return results;
        }

        public List<GetAttractionFastPassRequestsWithStatistics> GetAttractionFastPassRequestsWithStatistics(long userID)
        {
            //TODO1: add options to get only the ACTIVE fastpasses so that we aren't wasting cycles retrieving data for deleted requests
            var connection = this.db.Database.GetDbConnection();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "GetAttractionFastPassRequestsWithStatistics";
            command.Parameters.Add(new MySqlParameter("userID", userID));
            var results = new List<GetAttractionFastPassRequestsWithStatistics>();
            var r = command.ExecuteReader();
            results = ToList<GetAttractionFastPassRequestsWithStatistics>(r);
            //while (r.Read())
            //{
            //    results.Add(new GetAttractionFastPassRequestsWithStatistics
            //    {
            //        AttractionFastPassRequestID = (long)r["AttractionFastPassRequestID"],
            //        UserID = (long)r["UserID"],
            //        AttractionID = (long)r["AttractionID"],
            //        Status = (AttractionFastPassRequestStatusEnum)(int)r["Status"],
            //        ParkName = (string)r["ParkName"],
            //        AttractionName = (string)r["AttractionName"],
            //        Date = (DateTime)r["Date"],
            //        NumberOfChecks = r["NumberOfChecks"] is System.DBNull ? 0 : (long)r["NumberOfChecks"],
            //        NumberOfAvailableChecks = r["NumberOfAvailableChecks"] is System.DBNull ? 0 : (long)r["NumberOfAvailableChecks"],
            //        LastAvailableCheck = r["LastAvailableCheck"] is System.DBNull ? null : new DateTime?((DateTime)r["LastAvailableCheck"]),
            //        LastCheckDate = r["LastCheckDate"] is System.DBNull ? null : new DateTime?((DateTime)r["LastCheckDate"]),
            //        SMSNotificationPhoneNumber = (string)r["SMSNotificationPhoneNumber"],
            //        NumberOfPeople = (int)r["NumberOfPeople"],
            //        LastCheckStatus = r["LastCheckStatus"] is System.DBNull ? null : (string)r["LastCheckStatus"],
            //        LastCheckTimestamp = r["LastCheckTimestamp"] is System.DBNull ? null : new DateTime?((DateTime)r["LastCheckTimestamp"]),
            //    });
            //}
            r.Close();
            connection.Close();
            return results;
        }

        private List<T> ToList<T>(DbDataReader reader) where T : new()
        {
            List<T> list = new List<T>();
            while (reader.Read())
            {
                T t = new T();
                Type destinationObjectType = t.GetType();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var readerFieldValue = reader.GetValue(i);

                    PropertyInfo destinationObjectField = destinationObjectType.GetProperty(reader.GetName(i));

                    if (destinationObjectField == null)
                        continue;

                    if (readerFieldValue is System.DBNull)
                        readerFieldValue = null;
                    else
                    {
                        if (reader.GetFieldType(i) != destinationObjectField.PropertyType)
                        {
                            if ((destinationObjectField.PropertyType == typeof(DateTimeOffset)) || (destinationObjectField.PropertyType == typeof(DateTimeOffset?)))
                                readerFieldValue = new DateTimeOffset((DateTime)readerFieldValue);
                        }
                    }
                    destinationObjectField.SetValue(t, readerFieldValue, null);
                }
                list.Add(t);
            }
            return list;
        }

        public IQueryable<GetAttractionsThatAreDueToBeCheckedModel> GetAttractionsThatAreDueToBeChecked(long[] restrictToJustTheseAttractionFastPassRequestIDs, long? userID)
        {
            // get FP requests that are due to process
            //
            //  the request must meet ALL OF THESE:
            //
            //      request status = active
            //      user is activated
            //      request date is now or in the future
            //
            //  and also...
            //
            //      MUST MEET EITHER BOTH OF THESE:
            //
            //          request has never been checked (a.k.a. LastCheckTimestamp is null) OR it's been more than CheckFrequencyInMinutes since the LastCheckTimestamp
            //          user has never been notified (a.k.a. LastNotificationTimestamp is null) OR it's been more than PauseFrequencyInMinutes since the LastNotificationTimestamp
            //
            //      OR ALL OF THESE:
            //         
            //          LastNotificationTimestamp has a value
            //          we are passed the "pause" window (a.k.a. it's been more than PauseFrequencyInMinutes since the LastNotificationTimestamp)
            //          LastCheckTimestamp is prior to the end of the pause window (a.k.a. LastCheckTimestamp <= LastNotificationTimestamp + PauseFrequency)
            //

            var list = from a in db.AttractionFastPassRequests
                       join b in db.Attractions on a.AttractionID equals b.AttractionID
                       join c in db.Parks on b.ParkID equals c.ParkID
                       join u in db.Users on a.UserID equals u.UserID
                       join up in db.UserPlans on u.UserPlanID equals up.UserPlanID
                       where 
                          a.UserID == userID.GetValueOrDefault(a.UserID)
                          && a.Status == AttractionFastPassRequestStatusEnum.Active
                          && a.User.IsActivated
                          && a.Date >= DateTime.UtcNow.Date
                          && (restrictToJustTheseAttractionFastPassRequestIDs == null || restrictToJustTheseAttractionFastPassRequestIDs.Contains(a.AttractionFastPassRequestID))
                          && (
                                (
                                    (a.LastCheckTimestamp.GetValueOrDefault(new DateTime(1980,1,1)).AddMinutes(up.CheckFrequencyInMinutes - 1) <= DateTime.UtcNow) 
                                    && (a.LastNotificationTimestamp.GetValueOrDefault(new DateTime(1980, 1, 1)).AddMinutes(up.PauseFrequencyInMinutes - 1) <= DateTime.UtcNow)
                                )
                                ||
                                (
                                    a.LastNotificationTimestamp.HasValue
                                    && (a.LastNotificationTimestamp.GetValueOrDefault(new DateTime(1980, 1, 1)).AddMinutes(up.PauseFrequencyInMinutes - 1) <= DateTime.UtcNow)
                                    && (a.LastCheckTimestamp.GetValueOrDefault(DateTime.UtcNow) <= a.LastNotificationTimestamp.GetValueOrDefault(new DateTime(1980, 1, 1)).AddMinutes(up.PauseFrequencyInMinutes - 1))
                                )
                             )
                       select new GetAttractionsThatAreDueToBeCheckedModel { AttractionID = a.AttractionID, AttractionName = b.Name, AttractionPriority = b.Priority, DisneyApiParkID = c.DisneyApiParkID, Date = a.Date, NumberOfPeople = a.NumberOfPeople };

            // de-dupe to return only unique combinations of:
            // attraction, date, and number of people
            return from r in list
                   group r by new { r.AttractionID, AttractionName = r.AttractionName, r.AttractionPriority, r.DisneyApiParkID, r.Date, r.NumberOfPeople }
                   into grouped
                   select new GetAttractionsThatAreDueToBeCheckedModel { AttractionID = grouped.Key.AttractionID, AttractionName = grouped.Key.AttractionName, AttractionPriority = grouped.Key.AttractionPriority, DisneyApiParkID = grouped.Key.DisneyApiParkID, Date = grouped.Key.Date, NumberOfPeople = grouped.Key.NumberOfPeople };
        }

    }
}
