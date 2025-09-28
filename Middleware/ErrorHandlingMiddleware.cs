using System.Net;            //Convert Exceptions to problem details. 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dealership.Api.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> log)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (DbUpdateException ex)
        {
            log.LogError(ex, "Database error");
            await WriteProblem(ctx, "Database error", 409, "https://httpstatuses.io/409");
        }
        catch (UnauthorizedAccessException ex)
        {
            log.LogWarning(ex, "Unauthorized");
            await WriteProblem(ctx, "Unauthorized", 401, "https://httpstatuses.io/401");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unhandled error");
            await WriteProblem(ctx, "Internal Server Error", 500, "https://httpstatuses.io/500");
        }
    }

    private static async Task WriteProblem(HttpContext ctx, string title, int status, string type)
    {
        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Title = title,
            Status = status,
            Type = type,
            Instance = ctx.Request.Path
        };

        await ctx.Response.WriteAsJsonAsync(problem);
    }
}
