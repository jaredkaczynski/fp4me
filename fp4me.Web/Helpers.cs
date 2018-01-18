using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Authentication;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using fp4me.Web.Models;
using Newtonsoft.Json;

namespace fp4me.Web
{
    public static class Helpers
    {
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "acefghkrstuxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Used to remove all the spaces, special characters, and the leading "1" or "+1" in a phone number, leaving just the 10 digits.
        /// </summary>
        /// <param name="phoneNumber">Any phone number string, such as (800) 555-1212 or +19107773333</param>
        /// <returns>Returns just the 10 digits of a U.S. based phone number. For example, +1 (800) 555-1212 will return 8005551212.</returns>
        public static string CleanPhoneNumber(string phoneNumber)
        {
            var validPhoneNumberLength = "8005551212".Length;
            var phoneNumberWithJustTheNumbers = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d]", String.Empty);
            if (phoneNumberWithJustTheNumbers.Length > validPhoneNumberLength)
            {
                phoneNumberWithJustTheNumbers = phoneNumberWithJustTheNumbers.Substring(phoneNumberWithJustTheNumbers.Length - validPhoneNumberLength);
            }
            return phoneNumberWithJustTheNumbers;
        }

        public static string FormatPhoneNumber(string phoneNumber)
        {
            phoneNumber = Helpers.CleanPhoneNumber(phoneNumber);
            return String.Format("{0:(###) ###-####}", long.Parse(phoneNumber));
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static string ToRelativeDateString(this DateTime date)
        {
            return date.ToRelativeDateString(null);
        }
        
        public static string ToRelativeDateString(this DateTime date, string relativeDateFormat)
        {
            return date.ToRelativeDateString(relativeDateFormat, relativeDateFormat, 0);
        }

        public static string ToRelativeDateString(this DateTime date, int numberOfDaysDifferenceBeforeFullDateIsReturned, string fullDateFormat)
        {
            return date.ToRelativeDateString(null, null, 0, numberOfDaysDifferenceBeforeFullDateIsReturned, fullDateFormat);
        }

        public static string ToRelativeDateString(this DateTime date, string relativeDateFormat, string justNowDateFormat, int justNowSecondsTolerance)
        {
            return date.ToRelativeDateString(relativeDateFormat, justNowDateFormat, justNowSecondsTolerance, int.MaxValue, null);
        }

        public static string ToRelativeDateString(this DateTime date, string relativeDateFormat, string justNowDateFormat, int justNowSecondsTolerance, int numberOfDaysDifferenceBeforeFullDateIsReturned, string fullDateFormat)
        {
            var secondsAgo = Math.Abs(Convert.ToInt64((DateTime.UtcNow - date).TotalSeconds));

            relativeDateFormat = string.IsNullOrEmpty(relativeDateFormat) ? "{0}" : relativeDateFormat;
            justNowDateFormat = string.IsNullOrEmpty(justNowDateFormat) ? "{0}" : justNowDateFormat;

            numberOfDaysDifferenceBeforeFullDateIsReturned = string.IsNullOrEmpty(fullDateFormat) ? 364 : numberOfDaysDifferenceBeforeFullDateIsReturned;

            if (secondsAgo <= justNowSecondsTolerance)
                return String.Format(justNowDateFormat, String.Format("{0}{1}", secondsAgo, "s"));
            else if (secondsAgo < 60)
                return String.Format(relativeDateFormat, String.Format("{0}{1}", secondsAgo, "s"));
            else if (secondsAgo < 60 * 60)
                return String.Format(relativeDateFormat, String.Format("{0}{1}", Convert.ToInt32(secondsAgo / 60.0), "m"));
            else if (secondsAgo < 60 * 60 * 24)
                return String.Format(relativeDateFormat, String.Format("{0}{1}", Convert.ToInt32(secondsAgo / (60.0 * 60)), "h"));
            else if (secondsAgo < 60.0 * 60 * 24 * numberOfDaysDifferenceBeforeFullDateIsReturned)
                return String.Format(relativeDateFormat, String.Format("{0}{1}", Convert.ToInt32(secondsAgo / (60.0 * 60 * 24)), "d"));
            else if (string.IsNullOrEmpty(fullDateFormat))
                return String.Format(relativeDateFormat, String.Format("{0:0}{1}", Convert.ToInt32(secondsAgo / (60.0 * 60 * 24 * 365)), "y"));
            else
                return date.ToString(fullDateFormat);
        }

        public static string GetHash(string text)
        {
            // SHA512 is disposable by inheritance.  
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.  
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                // Get the hashed string.  
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static string SerialiseModelState(ModelStateDictionary modelState)
        {
            var errorList = modelState
                .Select(kvp => new ModelStateTransferValue
                {
                    Key = kvp.Key,
                    AttemptedValue = kvp.Value.AttemptedValue,
                    RawValue = kvp.Value.RawValue,
                    ErrorMessages = kvp.Value.Errors.Select(err => err.ErrorMessage).ToList(),
                });

            return JsonConvert.SerializeObject(errorList);
        }

        public static ModelStateDictionary DeserialiseModelState(string serialisedErrorList)
        {
            var errorList = JsonConvert.DeserializeObject<List<ModelStateTransferValue>>(serialisedErrorList);
            var modelState = new ModelStateDictionary();

            foreach (var item in errorList)
            {
                modelState.SetModelValue(item.Key, item.RawValue, item.AttemptedValue);
                foreach (var error in item.ErrorMessages)
                {
                    modelState.AddModelError(item.Key, error);
                }
            }
            return modelState;
        }

    }
}
