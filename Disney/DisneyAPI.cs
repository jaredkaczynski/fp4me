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
            string PHPSESSID = "";
            string pep_oauth_token = "";

            using (var httpClientHandler = new HttpClientHandler())
            {
                // request the login page from Disney to start a new Disney session
                var httpClient = new HttpClient(httpClientHandler);
                HttpResponseMessage response = await httpClient.GetAsync(url);
                string html = await response.Content.ReadAsStringAsync();
                PHPSESSID = httpClientHandler.CookieContainer.GetCookies(new Uri("https://disneyworld.disney.go.com"))["PHPSESSID"].Value;

                // from the source of the login page, extract the pep_csrf value; this is needed to perform the login
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                pep_csrf = doc.DocumentNode.SelectNodes("//input[@name='pep_csrf']")[0].Attributes["value"].Value;
            }

            // prepare the request body that will be used to login
            string content = String.Format("pep_csrf={0}&returnUrl=&username={1}&password={2}&rememberMe=0&submit=", pep_csrf, System.Uri.EscapeDataString(username), System.Uri.EscapeDataString(password));
            
            using (var httpClientHandler = new HttpClientHandler())
            {
                // prepare the client handler to make the login POST request
                // 1) add the session id cookie
                // 2) configure it to NOT follow redirects so that we can pull the oauth token from the response before the redirect happens
                httpClientHandler.CookieContainer = new CookieContainer();
                httpClientHandler.CookieContainer.Add(new Uri("http://disneyworld.disney.go.com"), new Cookie("PHPSESSID", PHPSESSID) { Domain = "disneyworld.disney.go.com" });
                httpClientHandler.AllowAutoRedirect = false;
                // create the login POST client
                var httpClient = new HttpClient(httpClientHandler);
                // add the required form data to the request; see above for how this is prepared - it includes the username and the password
                var httpContent = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");
                // POST to the login page
                HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                string html = await response.Content.ReadAsStringAsync();
                // extract the oauth token from the response cookies
                pep_oauth_token = httpClientHandler.CookieContainer.GetCookies(new Uri("https://disneyworld.disney.go.com"))["pep_oauth_token"].Value; 
            }

            PEP_OAUTH_TOKEN = pep_oauth_token;
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
