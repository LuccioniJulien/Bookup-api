using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BaseApi.Helper
{
    public static class Format
    {
        public static object ToBadRequest(this ModelStateDictionary modelState)
        {
            List<string> errors = modelState.Keys
                .SelectMany(key => modelState[key].Errors.Select(x => x.ErrorMessage))
                .ToList();

            var response = new
            {
                meta = new
                {
                    errors
                }
            };
            
            return response;
        }

        public static object ToBadRequest(this string message)
        {
            string[] errors = { message };
            
            var response = new
            {
                meta = new
                {
                    errors
                }
            };

            return response;
        }
    }
}