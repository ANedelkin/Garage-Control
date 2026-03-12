using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace GarageControl.Core.Helpers
{
    public static class IdentityResultHelper
    {
        public static Dictionary<string, List<string>> ProcessIdentityResult(IdentityResult result)
        {
            var errorsDictionary = new Dictionary<string, List<string>>();

            foreach (var error in result.Errors)
            {
                string field = "General";

                if (error.Code.Contains("Password"))
                {
                    field = "Password";
                }
                else if (error.Code.Contains("UserName"))
                {
                    field = "Username";
                }
                else if (error.Code.Contains("Email"))
                {
                    field = "Email";
                }

                if (!errorsDictionary.ContainsKey(field))
                {
                    errorsDictionary[field] = new List<string>();
                }

                errorsDictionary[field].Add(error.Description);
            }

            return errorsDictionary;
        }
    }
}