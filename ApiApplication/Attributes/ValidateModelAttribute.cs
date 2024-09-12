using ApiApplication.DTOs.Abstract;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ApiApplication.Attributes
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.SelectMany(v => v.Errors)
                                                      .Select(e => e.ErrorMessage)
                                                      .ToList();

                context.Result = new BadRequestObjectResult(new ApiResponseDTO<object>()
                {
                    IsSuccess = false,
                    ErrorCode = Constants.Constants.VALIDATION_FAILED,
                    Payload = errors
                });
            }
        }
    }
}
