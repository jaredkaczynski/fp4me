using Disney;
using fp4me.Web.Data;
using fp4me.Web.Data.Models;
using fp4me.Web.Models;
using fp4me.Web.Models.UserViewModels;
using fp4me.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fp4me.Web.Controllers
{
    public class UserController : BaseController
    {
        private IViewRenderService _viewRenderService;
        private AppSettings _appSettings;

        public UserController(FacadeContext facade, IOptions<AppSettings> appSettings, IViewRenderService viewRenderService) : base(facade, appSettings)
        {
            _viewRenderService = viewRenderService;
            _appSettings = appSettings.Value;
        }

        [SimpleAuthorize]
        public IActionResult DeleteAttractionFastPassRequest(long AttractionFastPassRequestID)
        {
            facade.DeleteAttractionFastPassRequest(AttractionFastPassRequestID, this.HttpContext.AppUser().UserID);
            //TODO2: add a strategy for showing a "delete successful" message
            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult AddFPCheck()
        {
            return RedirectToAction(nameof(HomeController.Home), nameof(HomeController).Replace("Controller", ""),
                new { add = true });
        }

        [HttpGet]
        public IActionResult OptOutOfSMS(string phoneNumber, string optOutToken)
        {
            if (optOutToken != GetOptOutToken(phoneNumber))
                return RedirectToAction(nameof(HomeController.Home), nameof(HomeController).Replace("Controller", ""));

            var user = facade.GetUserBySMSNotificationPhoneNumber(phoneNumber);

            return View(new OptOutOfSMSViewModel { PhoneNumber = phoneNumber, OptOutToken = GetOptOutToken(phoneNumber) });
        }

        [HttpPost]
        public IActionResult OptOutOfSMS(OptOutOfSMSViewModel model)
        {
            if (model.OptOutToken == GetOptOutToken(model.PhoneNumber))
            {
                facade.SignalOptOut(model.PhoneNumber);
                return RedirectToAction(nameof(UserController.OptOutOfSMSSuccess));
            }
            return RedirectToAction(nameof(HomeController.Home), nameof(HomeController).Replace("Controller", ""));
        }

        public IActionResult OptOutOfSMSSuccess()
        {
            return View();
        }

        private string GetOptOutToken(string phoneNumber)
        {
            var user = facade.GetUserBySMSNotificationPhoneNumber(phoneNumber);
            return Helpers.GetHash(string.Format("{0}.{1}", user.SMSNotificationPhoneNumber, (user.UserID / Math.PI).ToString())).Substring(0, 7);
        }

        [SimpleAuthorize]
        [HttpGet]
        public IActionResult Dashboard(long? newAttractionFastPassRequestID)
        {
            var requests = facade.GetAttractionFastPassRequestsWithStatistics(this.HttpContext.AppUser().UserID);

            var user = facade.GetUserWithStatistics(this.HttpContext.AppUser().UserID);

            return View(new DashboardViewModel
            {
                UserPlan = this.HttpContext.AppUser().UserPlan,
                AttractionFastPassRequestsWithStatistics = requests,
                NewAttractionFastPassRequestID = newAttractionFastPassRequestID,
                IsFirstSavedAttractionFastPassRequestForUser = newAttractionFastPassRequestID.HasValue && user.NumberOfAttractionFastPassRequests.Equals(1),
                RecentChecksForAttractionFastPassRequests = facade.GetMostRecentChecksForAttractionFastRequests(this.HttpContext.AppUser().UserID, 3),
                HowOftenTheSystemChecksAndProcessesAttractionFastPassRequestsInMinutes = _appSettings.HowOftenTheSystemChecksAndProcessesAttractionFastPassRequestsInMinutes
            });
        }

        [HttpGet]
        public IActionResult SignIn(string phoneNumber, string waitlistCode)
        {
            return View(new SignInViewModel { PhoneNumber = phoneNumber, WaitlistCode = waitlistCode });
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(Models.UserViewModels.SignInViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = facade.GetUserBySMSNotificationPhoneNumber(model.PhoneNumber);
            var userExists = user != null;  // for code readability

            if (userExists)
            {
                return RedirectToAction(nameof(UserController.SignInWithCode), new SignInWithCodeViewModel { PhoneNumber = user.SMSNotificationPhoneNumber });
            }

            // If we're here, the user trying to sign in doesn't exist.
            //
            // So...
            //
            // Check if we're allowing new users:
            var allowNewUserToSignUp = _appSettings.AcceptingNewUsers;
            WaitlistCode waitlistCode = null;
            // If we're not allowing new users, check if the sign in form had a waitlist code.
            // A waitlist code can be used to bypass the check that stops new users. This lets us send a user
            // a sign in link like /user/signin?waitlistcode=12345. If a WaitlistCode exists in the database
            // and it hasn't been "used", then allow this new user to sign up
            if (!allowNewUserToSignUp && !string.IsNullOrEmpty(model.WaitlistCode))
            {
                // try to get the wait list code by searching for the code and IS NOT USED
                waitlistCode = facade.db.WaitlistCodes.FirstOrDefault(p => p.Code == model.WaitlistCode && !p.Used);
                // allow the new user if we found a valid unused waitlist code
                allowNewUserToSignUp = waitlistCode != null;
            }

            if (allowNewUserToSignUp)
            // if a user doesn't exist for the specified phone number AND we are accepting new users, create the new user 
            {
                facade.AddUser(new Data.Models.User { SMSNotificationPhoneNumber = model.PhoneNumber });
                // if we have a waitlist code in context, mark it as "used"
                if (waitlistCode != null)
                    waitlistCode.Used = true;
                facade.db.SaveChanges();
                var signInLink = GenerateSignInLink(model.PhoneNumber, true);
                await SendSignInLink(Smart.Format(Messages.SIGN_IN_LINK_WELCOME_NEW_USER, new { code = signInLink.AccessToken, url = signInLink.Url.AbsoluteUri }), model.PhoneNumber);
                return RedirectToAction(nameof(UserController.SignInLinkSent), new SignInLinkSentViewModel(model.PhoneNumber, true, false, false));
            }
            else
            {
                ViewData["HighlightWaitlistMessage"] = true;
                return View(model);
            }
        }

        private async Task SendSignInLink(string message, string phoneNumber)
        {
            await facade.SendTextMessageAsync(phoneNumber, message);
            facade.SignalSignInLinkSent(phoneNumber);
        }

        private SignInLink GenerateSignInLink(string userSMSNotificationPhoneNumber, bool generateNewAccessToken)
        {
            var user = facade.GetUserBySMSNotificationPhoneNumber(userSMSNotificationPhoneNumber);

            if (user == null)
                return null;

            if (generateNewAccessToken || string.IsNullOrEmpty(user.AccessToken))
            {
                user.AccessToken = GenerateNewAccessToken();
                facade.db.SaveChanges();
            }

            return new SignInLink
            {
                Url = new Uri(Url.AbsoluteAction(nameof(UserController.Authenticate), nameof(UserController).Replace("Controller", ""), new AuthenticateViewModel(user.AccessToken))),
                AccessToken = user.AccessToken
            };
        }

        private string GenerateNewAccessToken()
        {
            return Helpers.RandomString(6).ToLowerInvariant();
        }

        [SimpleAuthorize]
        public async Task<ActionResult> DashboardAttractionFastPassRequestCardAsync(long[] attractionFastPassRequestIDs)
        {
            var requestsInfo = facade.GetAttractionFastPassRequestsWithStatistics(this.HttpContext.AppUser().UserID);
            var recentChecksForAttractionFastPassRequests = facade.GetMostRecentChecksForAttractionFastRequests(this.HttpContext.AppUser().UserID, 3);
            List<Models.KeyValuePair> requestResults = new List<Models.KeyValuePair>();
            ((ViewRenderServiceUsingHttpContext)_viewRenderService).HttpContext = this.HttpContext;

            foreach (long id in attractionFastPassRequestIDs)
            {
                string html = null;
                var request = requestsInfo.FirstOrDefault(p => p.AttractionFastPassRequestID == id);
                if (request != null)
                {        
                    html = await _viewRenderService.RenderToStringAsync("DashboardAttractionFastPassRequestCard", new DashboardAttractionFastPassRequestCardViewModel
                    {
                        GetAttractionFastPassRequestsWithStatistics = request,
                        UserPlan = this.HttpContext.AppUser().UserPlan,
                        RecentChecksForAttractionFastPassRequests = recentChecksForAttractionFastPassRequests.Where(p => p.AttractionFastPassRequestID == id),
                        HowOftenTheSystemChecksAndProcessesAttractionFastPassRequestsInMinutes = _appSettings.HowOftenTheSystemChecksAndProcessesAttractionFastPassRequestsInMinutes
                    });
                    requestResults.Add(new Models.KeyValuePair { key = id.ToString(), value = html });
                }
            }
            return Ok(requestResults);

        }

        [SimpleAuthorize]
        public async Task<IActionResult> ProcessAttractionFastPassRequests(long[] attractionFastPassRequestIDs)
        {
            var services = new fp4meServices(this.facade, (IOptions<AppSettings>)this.HttpContext.RequestServices.GetService(typeof(IOptions<AppSettings>)), (IDisneyAPI)this.HttpContext.RequestServices.GetService(typeof(IDisneyAPI)), (IOptions<DisneyAPISettings>)this.HttpContext.RequestServices.GetService(typeof(IOptions<DisneyAPISettings>)));
            var processingResults = await services.ProcessAttractionFastPassRequestsThatAreDueAsync(attractionFastPassRequestIDs, this.HttpContext.AppUser().UserID);
            return Ok();
        }

        public async Task<IActionResult> SendSignInLinkForNewUserAfterCheckCreated(Models.UserViewModels.SignInViewModel model)
        {
            var signInLink = GenerateSignInLink(model.PhoneNumber, true);
            await SendSignInLink(Smart.Format(Messages.SIGN_IN_LINK_WELCOME_NEW_USER_AND_CHECK_CREATED, new { code = signInLink.AccessToken, url = signInLink.Url.AbsoluteUri }), model.PhoneNumber);
            return RedirectToAction(nameof(UserController.SignInLinkSent), new SignInLinkSentViewModel(model.PhoneNumber, true, false, true));
        }

        public async Task<IActionResult> SendNewSignInLink(string phoneNumber)
        {
            var user = facade.GetUserBySMSNotificationPhoneNumber(phoneNumber);
            if (user == null)
            {
                ModelState.AddModelError(nameof(SignInViewModel.PhoneNumber), string.Format(Messages.USER_BY_PHONE_NUMBER_DOESNT_EXIST, phoneNumber));
                return RedirectToAction(nameof(UserController.SignIn));
            }
            else
            {
                if (user.NumberOfSuccessiveSignInLinkAttempts >= 3)
                {
                    return RedirectToAction(nameof(UserController.SignInLinkMaximumReached), new { phoneNumber = phoneNumber });
                }
                else
                {
                    var signInLink = GenerateSignInLink(user.SMSNotificationPhoneNumber, true);
                    if (user.IsActivated)
                    {
                        await SendSignInLink(Smart.Format(Messages.SIGN_IN_LINK_MESSAGE_FOR_ACTIVE_USER, new { code = signInLink.AccessToken, url = signInLink.Url.AbsoluteUri }), phoneNumber);
                    }
                    else
                    {
                        await SendSignInLink(Smart.Format(Messages.SIGN_IN_LINK_MESSAGE_FOR_INACTIVE_USER, new { code = signInLink.AccessToken, url = signInLink.Url.AbsoluteUri }), phoneNumber);
                        if (user.NumberOfSuccessiveSignInLinkAttempts == 2)
                        {
                            await facade.SendTextMessageAsync(phoneNumber, String.Format(Messages.OPT_OUT_MESSAGE, Url.AbsoluteAction(nameof(UserController.OptOutOfSMS), nameof(UserController).Replace("Controller", ""), new OptOutOfSMSViewModel { PhoneNumber = phoneNumber, OptOutToken = GetOptOutToken(phoneNumber) })));
                        }
                    }
                    return RedirectToAction(nameof(UserController.SignInLinkSent), new SignInLinkSentViewModel(phoneNumber, false, user.IsActivated, false));

                }

            }
        }

        public IActionResult SignInLinkMaximumReached(string phoneNumber)
        {
            return View((object)phoneNumber);
        }

        public IActionResult SignInLinkSent(Models.UserViewModels.SignInLinkSentViewModel model)
        {
            model.AccessToken = facade.GetUserBySMSNotificationPhoneNumber(model.SMSNotificationPhoneNumber).AccessToken;
            return View(model);
        }

        public IActionResult Activated()
        {
            var user = facade.GetUserWithStatisticsBySMSNotificationPhoneNumber(this.HttpContext.AppUser().SMSNotificationPhoneNumber);
            return View(user);
        }

        public IActionResult SignInWithCode(SignInWithCodeViewModel model)
        {
            var user = facade.GetUserBySMSNotificationPhoneNumber(model.PhoneNumber);

            if (user == null)
                return RedirectToAction(nameof(SignIn));

            model.IsActivated = user.IsActivated;

            return View(model);
        }

        public IActionResult Authenticate(AuthenticateViewModel model)
        {
            var user = facade.db.Users.FirstOrDefault(p => p.AccessToken == model.AccessToken);
            var authenticationWasSuccessful = (user != null); // for code readability

            if (authenticationWasSuccessful)
            {
                // add the cookie to keep the user signed in
                this.HttpContext.Response.Cookies.Append("bearer", user.AccessToken);

                // determine if this is the first sign in
                var isFirstSignInForUser = !user.IsActivated;

                // apply whatever rules need to be applied for sign ins
                facade.SignalSignIn(user.UserID);

                if (isFirstSignInForUser)
                {
                    return RedirectToAction(nameof(UserController.Activated));
                }
                else
                {
                    // on successful sign in, if the user has some checks, redirect to show them; otherwise redirect to home
                    if (facade.db.AttractionFastPassRequests.Any(p => p.User.UserID == user.UserID && p.Status == AttractionFastPassRequestStatusEnum.Active))
                        return RedirectToAction(nameof(UserController.Dashboard));
                    else
                        return RedirectToAction(nameof(HomeController.Home), nameof(HomeController).Replace("Controller", ""));
                }
            }
            else
            {
                ModelState.Clear();
                ModelState.AddModelError("AccessToken", Messages.INVALID_AUTH_LINK);
                return RedirectToAction(nameof(UserController.SignIn));
            }
        }

        public IActionResult SignOut()
        {
            this.HttpContext.Response.Cookies.Delete("bearer");
            return RedirectToAction(nameof(HomeController.Home), nameof(HomeController).Replace("Controller", ""));
        }
    }
}
