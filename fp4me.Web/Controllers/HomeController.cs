using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using fp4me.Web.Models;
using fp4me.Web.Data;
using fp4me.Web.Data.Models;
using fp4me.Web.Models.UserViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using fp4me.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using fp4me.Web.Models.HomeViewModels;
using Helpers.Services;

namespace fp4me.Web.Controllers
{
    public class HomeController : BaseController
    {
        private IEmailSender _emailSender;

        public HomeController(FacadeContext facade, IEmailSender emailSender, IOptions<AppSettings> appSettings) : base(facade, appSettings) {
            _emailSender = emailSender;
        }

        [SimpleAuthorize(UserMustBeAdmin = true)]
        public IActionResult TestBitFlip()
        {
            var user = facade.db.Users.Single(p => p.UserID == this.HttpContext.AppUser().UserID);
            user.IsActivated = !user.IsActivated;
            facade.db.SaveChanges();
            return RedirectToAction(nameof(HomeController.Test));
        }

        [SimpleAuthorize(UserMustBeAdmin = true)]
        public IActionResult Test()
        {
            return View();
        }

        public IActionResult Plans()
        {
            var userPlans = facade.db.UserPlans.Where(p => p.IsPublic).ToList();
            if (this.HttpContext.AppUser().IsAuthenticated)
                if (!userPlans.Any(p => p.UserPlanID == this.HttpContext.AppUser().UserPlan.UserPlanID))
                    userPlans.Add(this.HttpContext.AppUser().UserPlan);

            return View(userPlans);
        }

        [SimpleAuthorize]
        [HttpGet]
        [ThisPageIsUsedForCreatingNewFastPassChecks]
        public IActionResult Home(bool? add)
        {
            if (HttpContext.AppUser().IsAuthenticated && !add.GetValueOrDefault(false))
                return RedirectToAction(nameof(UserController.Dashboard), nameof(UserController).Replace("Controller", ""));

            var model = new HomeViewModel();

            model.Attractions = facade.db.Attractions
                .OrderBy(p => new {  p.Park.Abbreviation, p.Name })
                .Select(p => new { Name = String.Format("{0} - {1}", p.Park.Abbreviation, p.Name), p.AttractionID })
                .ToDictionary(p => p.AttractionID, p => p.Name);

            model.PriorityAttractions = facade.db.Attractions
                .Where(p => p.Priority.GetValueOrDefault(0) > 0)
                .OrderBy(p => new { p.Priority, p.Park.Abbreviation, p.Name })
                .Select(p => Tuple.Create<long, long, string>(p.Priority.Value, p.AttractionID, String.Format("{0} - {1}", p.Park.Abbreviation, p.Name)))
                .ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendContactMessage(ContactViewModel model)
        {
            await _emailSender.SendEmailAsync(settings.ContactEmailFromAddress, settings.ContactEmailToAddress, String.Format("Message from Contact Us Page - {0}", System.Guid.NewGuid().ToString().Substring(0, 6)), String.Format("from: {0}{1}{1}{2}", model.EmailAddress, System.Environment.NewLine, model.Message));
            return Ok();
        }

        [HttpPost]
        [ThisPageIsUsedForCreatingNewFastPassChecks]
        public IActionResult Home(HomeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // get the user from the database, and determine if we're dealing with an existing user or a new one
                var user = facade.GetUserWithStatisticsBySMSNotificationPhoneNumber(model.SMSNotificationPhoneNumber);
                bool isNewUser = (user == null); // for code readability
                bool isExistingUser = !isNewUser; // for code readability

                // if this is an existing user and it is NOT the signed in user...
                if (isExistingUser && this.HttpContext.AppUser().IsAuthenticated && user.User.UserID != this.HttpContext.AppUser().UserID)
                {
                    // redirect to the home page
                    return RedirectToAction(nameof(HomeController.Home));
                }

                // if this is an existing user and the user is NOT signed in, redirect to a sign in page with explanation
                if (isExistingUser && !this.HttpContext.AppUser().IsAuthenticated)
                {
                    ModelState.AddModelError(nameof(SignInViewModel.PhoneNumber), string.Format(Messages.USER_BY_PHONE_NUMBER_ALREADY_EXISTS, Helpers.FormatPhoneNumber(model.SMSNotificationPhoneNumber)));
                    return RedirectToAction(nameof(UserController.SignIn), nameof(UserController).Replace("Controller", ""), new SignInViewModel { PhoneNumber = Helpers.FormatPhoneNumber(model.SMSNotificationPhoneNumber) });
                }

                // if this is a new user, add the user to the database
                if (isNewUser)
                {
                    facade.AddUser(new User { SMSNotificationPhoneNumber = model.SMSNotificationPhoneNumber });
                    facade.db.SaveChanges();
                    user = facade.GetUserWithStatisticsBySMSNotificationPhoneNumber(model.SMSNotificationPhoneNumber);
                }

                // add the fastpass check
                var check = new AttractionFastPassRequest
                {
                    Date = model.Date,
                    Attraction = facade.db.Attractions.First(p => p.AttractionID == model.AttractionID),
                    NumberOfPeople = model.NumberOfPeople,
                    UserID = user.User.UserID,
                    Status = AttractionFastPassRequestStatusEnum.Active,
                    CreatedOn = DateTime.UtcNow
                };
                facade.db.AttractionFastPassRequests.Add(check);
                facade.db.SaveChanges();

                // if this is an existing user and they are signed in redirect to the saved-successful page
                if (isExistingUser && this.HttpContext.AppUser().IsAuthenticated)
                {
                    return RedirectToAction(nameof(UserController.Dashboard), nameof(UserController).Replace("Controller", ""), new DashboardViewModel { NewAttractionFastPassRequestID = check.AttractionFastPassRequestID });
                }
                // if this was a new user, send the sign in code & link and redirect to explanation page
                else
                {
                    return RedirectToAction(nameof(UserController.SendSignInLinkForNewUserAfterCheckCreated), nameof(UserController).Replace("Controller", ""), new SignInViewModel { PhoneNumber = model.SMSNotificationPhoneNumber });
                }

            }
            return View(model);
        }
        
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult ContactForWaitingListSignup()
        {
            string message = "Hi fp4me,##Please add my email address to the waitlist for new signups.##Thanks,#A fan";
            message = message.Replace("#", System.Environment.NewLine);
            return RedirectToAction(nameof(HomeController.Contact), nameof(HomeController).Replace("Controller", ""), new ContactViewModel { Message = message, Focus = nameof(ContactViewModel.EmailAddress) });
        }

        public IActionResult Contact(ContactViewModel model)
        {
            if (String.IsNullOrEmpty(model.Message))
            {
                model.Message = "Hi fp4me,##Love your app! Here are some ideas to make it even better.##Thanks!#A fan";
                model.Message = model.Message.Replace("#", System.Environment.NewLine);
            }
            ModelState.Clear();
            return View(model);
        }

        public IActionResult Error()
        {
            return View(new Models.SharedViewModels.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
