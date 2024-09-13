using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.WebApi
{
    public class GlobalExceptionHandlingMidlleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMidlleware> _logger;

       public GlobalExceptionHandlingMidlleware(RequestDelegate next,
                ILogger<GlobalExceptionHandlingMidlleware> logger)
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
            catch (CustomException customException)
            {
                _logger.LogError(customException, "Custom exception: {Message}", customException.Message);
                await HandleExceptionAsync(context, customException.StatusCode, customException);
            }
            catch (ValidationException validationException)
            {
                _logger.LogError(validationException, "Validation exception: {Message}", validationException.Message);
                await HandleValidationExceptionAsync(context, validationException);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
                await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, Exception exception)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = "Error",
                Detail = exception.Message
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest; 
            context.Response.ContentType = "application/json";

            var errors = validationException.Errors.Select(error => new
            {
                Field = error.PropertyName,
                Error = error.ErrorMessage
            });

            var validationProblemDetails = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Validation errors occurred",
            };

            foreach (var error in errors)
            {
                validationProblemDetails.Errors.Add(error.Field, new[] { error.Error });
            }

            await context.Response.WriteAsJsonAsync(validationProblemDetails);
        }
    }
}

