using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;

namespace API.MiddleWare
{
    // Middleware class to handle exceptions globally in the application
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next; // Delegate to invoke the next middleware in the pipeline
        private readonly ILogger<ExceptionMiddleware> _logger; // Logger to log exceptions
        private readonly IHostEnvironment _env; // To check the hosting environment (e.g., Development or Production)

        // Constructor to inject dependencies
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        // Method to handle the middleware's functionality
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Attempt to invoke the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError(ex, ex.Message);

                // Set the response type and status code for the error
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Create a custom error response based on the environment
                var response = _env.IsDevelopment()
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiException(context.Response.StatusCode, "An error occurred", "Internal server error");

                // Configure JSON serialization options (e.g., camelCase naming convention)
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                // Serialize the response object to JSON
                var json = JsonSerializer.Serialize(response, options);

                // Write the JSON response back to the client
                await context.Response.WriteAsync(json);
            }
        }
    }
}

// Explanation of Key Points:
// 1. Purpose of the Middleware:
// This middleware catches any unhandled exceptions in the application pipeline and returns a consistent error response to the client.
// It prevents sensitive error details from being exposed in production.

// 2 .Environment Check:
// The env.IsDevelopment() condition ensures detailed error messages (including the stack trace) are only shown in development, not in production.

// 3. Custom Error Object (ApiException):
// This represents the structured error response that the client will receive.

// 4. JSON Serialization Options:
// The JsonSerializerOptions with JsonNamingPolicy.CamelCase ensures property names in the JSON follow camelCase, which is a common convention in APIs.

// 5.Global Error Handling:
// Instead of writing try-catch blocks in individual controllers or services, this middleware centralizes error handling, simplifying debugging and maintaining consistency.