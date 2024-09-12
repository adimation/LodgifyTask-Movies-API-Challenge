using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using ApiApplication.Exceptions;
using Newtonsoft.Json;
using ApiApplication.Constants;
using Microsoft.Extensions.Logging;
using ApiApplication.Exceptions.Interface;
using ApiApplication.DTOs.Abstract;
using ApiApplication.DTOs;
using FluentValidation;
using System.Linq;

public class CustomExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

    public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var errorCode = Constants.UNKNOWN_ERROR;
        var message = "An unexpected error occurred.";
        dynamic payload = null;

        if (exception is ICustomException customException) 
        {
            statusCode = customException.StatusCode;
            errorCode = customException.ErrorCode;
            message = ((Exception)customException).Message;
        }
        else if (exception is ArgumentNullException)
        {
            statusCode = HttpStatusCode.BadRequest;
            errorCode = Constants.ARGUMENT_NULL_ERROR;
            message = "A required argument was null.";
        }
        else if (exception is InvalidOperationException)
        {
            statusCode = HttpStatusCode.Conflict;
            errorCode = Constants.INVALID_OPERATION_ERROR;
            message = exception.Message;
        }
        else if (exception is ValidationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            errorCode = Constants.VALIDATION_FAILED;
            message = exception.Message;
            payload = ((ValidationException)exception).Errors.Select(e => e.ErrorMessage).ToList();
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        _logger.LogError(message, exception);

        var response = new ApiResponseDTO<object>() { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = message, Payload = payload };

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}