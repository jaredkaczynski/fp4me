using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

using Amazon.SimpleNotificationService;
using System.Collections.Generic;
using Helpers.Services;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Reflection;

namespace Disney
{
    public class DisneyAPI : IDisneyAPI
    {
        private string PEP_OAUTH_TOKEN = "";
        private IEmailSender _emailSender;
        private DisneyAPISettings _settings;

        public DisneyAPI(IEmailSender emailSender, IOptions<DisneyAPISettings> disneyAPISettings)
        {
            _emailSender = emailSender;
            _settings = disneyAPISettings.Value;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var url = "https://disneyworld.disney.go.com/login";

            string pep_csrf = "";
            string access_token = "";
            string PHPSESSID = "";
            string pep_oauth_token = "";
            CookieContainer cookieContainer = new CookieContainer();
            System.Net.Http.Headers.HttpResponseHeaders headers;
            string secure_access_token;

            using (var httpClientHandler = new HttpClientHandler())
            {
                // request the login page from Disney to start a new Disney session
                var httpClient = new HttpClient(httpClientHandler);
                httpClientHandler.CookieContainer = cookieContainer;
                HttpResponseMessage response = await httpClient.PostAsync("https://authorization.go.com/token?grant_type=assertion&assertion_type=public&client_id=TPR-WDW.AND-PROD",null);
                string html = await response.Content.ReadAsStringAsync();
                headers = response.Headers;
       
                // from the source of the login page, extract the pep_csrf value; this is needed to perform the login
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                ///Get access token aka Bearer token
                access_token = html.Substring(17,32);
                httpClient.DefaultRequestHeaders.Add("Authorization", "BEARER " + access_token);
                // request the login page from Disney to start a new Disney session

                //PHASE 1 Complete

                response = await httpClient.PostAsync("https://api.wdpro.disney.go.com/profile-service/v4/clients/TPR-WDW.AND-PROD/api-key", null);
                html = await response.Content.ReadAsStringAsync();
                headers = response.Headers;


                IEnumerable<string> values;
                string session = "";
                if (headers.TryGetValues("Api-Key", out values))
                {
                    session = values.First();
                }
                string session2 = "";

                httpClient.DefaultRequestHeaders.Add("x-authorization-gc", "APIKEY " + session);

                if (headers.TryGetValues("X-Conversation-Id", out values))
                {
                    session2 = values.First();
                }

                httpClient.DefaultRequestHeaders.Add("X-Conversation-Id", "X-Conversation-Id " + session2);
                string jsonStr = "{\"loginValue\":\"" + username + "\",\"password\":\"" + password + "\"}";
                var content2 = new StringContent(jsonStr, Encoding.UTF8, "application/json");

                response = await httpClient.PostAsync("https://api.wdpro.disney.go.com/profile-service/v4/clients/TPR-WDW.AND-PROD/guests/login", content2);
                html = await response.Content.ReadAsStringAsync();

                // from the source of the login page, extract the pep_csrf value; this is needed to perform the login
                doc = new HtmlDocument();
                doc.LoadHtml(html);
                secure_access_token = html.Split("access_token")[1].Substring(3, 32);
                //pep_csrf = doc.DocumentNode.SelectNodes("//input[@name='access_token']");


            }

            PEP_OAUTH_TOKEN = secure_access_token;
            return pep_oauth_token;
        }
        public async System.Threading.Tasks.Task<List<ExperienceFastpassAvailability>> GetFastpassStatus(DateTime date, string startTime, string endTime, string experienceParkID, int numberOfGuests)
        {
            // scott, heather, andrew, hannah:
            var guestIDs = _settings.GuestIDs;
            var guests = string.Join(",", (guestIDs.Split(',')).Take(numberOfGuests));
            
            var url = string.Format("https://disneyworld.disney.go.com/wdpro-wam-fastpassplus/api/v1/fastpass/orchestration/park/{4}/{2}/offers;start-time={0};end-time={1};guest-xids={3}/",
                startTime, endTime, date.ToString("yyyy-MM-dd"), guests, experienceParkID);
            
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("BEARER {0}", PEP_OAUTH_TOKEN));
            HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));
            string json = await response.Content.ReadAsStringAsync();
            
            var results = (new Helpers()).ExtractFastpassStatus(json);

            results.ForEach(p => p.Timestamp = DateTime.UtcNow);
            results.ForEach(p => p.NumberOfGuests = numberOfGuests);
            results.ForEach(p => p.ParkID = experienceParkID);
            results.ForEach(p => p.Date = date);

            return results;
        }
    }
}
