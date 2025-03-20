using Market.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Market.SharedLibrary.Middleware;

public class GlobalException(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string message = "Internal server error ocurred. Try again later!";
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string title = "Error";

        try
        {
            await next(context);

            if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
            {
                title = "Warning";
                message = "Too many requests were made.";
                statusCode = (int)StatusCodes.Status429TooManyRequests;
                await ModifyHeader(context, title, message, statusCode);
            }

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                title = "Alert";
                message = "You are no authorized to acess.";
                statusCode = (int)StatusCodes.Status401Unauthorized;
                await ModifyHeader(context, title, message, statusCode);
            }

            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                title = "Out of Access";
                message = "You are not allowed/required to access.";
                statusCode = (int)StatusCodes.Status403Forbidden;
                await ModifyHeader(context, title, message, statusCode);
            }
        }
        catch (Exception ex)
        {
            LogException.LogExceptions(ex);

            if(ex is TaskCanceledException || ex is TimeoutException)
            {
                title = "Timeout";
                message = "Request timeout, try again!";
                statusCode = (int)StatusCodes.Status408RequestTimeout;
            }
            await ModifyHeader(context, title, message, statusCode);
        }
    }
    private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
        {
            Detail = message,
            Status = statusCode,
            Title = title
        }),CancellationToken.None);
        return;
    }
}
