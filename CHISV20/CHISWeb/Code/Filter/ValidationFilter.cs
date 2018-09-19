using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.Filter
{
    public class ValidationFilter : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = new Dictionary<string,string>();
                foreach (var pair in context.ModelState)
                {
                    var errorMessages = pair.Value.Errors.Select(x => x.ErrorMessage);

                    var pairErrors = errorMessages.Aggregate("", (current, error) => current + error + " ");

                    errors.Add(pair.Key, pairErrors);
                }

                context.Result = new OkObjectResult(new
                {
                    rlt = false,
                    msg = errors
                });
                return;
            }
        }
    }
}
