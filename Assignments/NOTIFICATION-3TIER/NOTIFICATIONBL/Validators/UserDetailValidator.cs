using System.Text.RegularExpressions;
using NotificationModels.Exceptions;

namespace NotificationBl.Validators
{
    // class containing extension methods pertaining to user details
 public static class UserDetailValidator
    {

        // validate email
        public static void ValidateEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidUserDetailsException("Empty Email");
            }
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                throw new InvalidUserDetailsException("Email is not of proper format");
            }
        }
        // validate phone
        public static void ValidatePhone(this string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new InvalidUserDetailsException("Empty PhoneNumber");
                
            }
            if (phone.Contains('+'))
            {
                throw new InvalidUserDetailsException("Country code not to be included");

            }
            if (phone.Length != 10)
            {
                throw new InvalidUserDetailsException("Phone number doesn't match 10 digits");
                
            }
            
        }
    }
}