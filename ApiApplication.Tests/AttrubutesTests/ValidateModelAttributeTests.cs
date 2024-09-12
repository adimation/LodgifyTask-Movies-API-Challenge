using ApiApplication.Attributes;
using ApiApplication.DTOs.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Xunit;

namespace ApiApplication.Tests.AttrubutesTests
{
    public class ValidateModelAttributeTests
    {
        [Fact]
        public void OnActionExecuting_ShouldSetBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Field1", "Field1 is required.");
            modelState.AddModelError("Field2", "Field2 must be a valid email.");

            var httpContext = new DefaultHttpContext();
            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            var actionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor();

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor, modelState);

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new object()
            );

            var attribute = new ValidateModelAttribute();

            // Act
            attribute.OnActionExecuting(actionExecutingContext);

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            var response = Assert.IsType<ApiResponseDTO<object>>(result.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(Constants.Constants.VALIDATION_FAILED, response.ErrorCode);
            Assert.Contains("Field1 is required.", response.Payload as List<string>);
            Assert.Contains("Field2 must be a valid email.", response.Payload as List<string>);
        }

        [Fact]
        public void OnActionExecuting_ShouldNotSetResult_WhenModelStateIsValid()
        {
            // Arrange
            var modelState = new ModelStateDictionary();

            var httpContext = new DefaultHttpContext();
            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            var actionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor();

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor, modelState);

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new object()
            );

            var attribute = new ValidateModelAttribute();

            // Act
            attribute.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.Null(actionExecutingContext.Result);
        }
    }
}
