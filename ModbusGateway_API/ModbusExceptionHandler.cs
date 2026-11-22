using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Handles exception thrown while processing HTTP requests
/// and creates HTTP responses that the front end can show.
/// </summary>
public sealed class ModbusExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ModbusExceptionHandler> _logger;

    public ModbusExceptionHandler(
        ILogger<ModbusExceptionHandler> logger)
    {  
        _logger = logger; 
    }

    /// <summary>
    /// Handles exceptions of different types and creates the corresponding
    /// error response.
    /// </summary>
    /// <param name="context">Current HTTP request and response
    /// </param>
    /// <param name="exception">The unhandled exception
    /// </param>
    /// <param name="cancellationToken">A token that can cancel 
    /// the response operation
    /// </param>
    /// <returns>A boolean value that indicates if the excception was handled
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode;
        string title;
        string detail;

        //A Modbus device returned an exception response
        //We format it using the modbus exception formatter which provides us
        //details about what went wrong in compliance with Modbus' exception specification
        if (exception is NModbus.SlaveException ex)
        {
            ModbusExceptionResponse error =
                ModbusExceptionFormatter.Format(ex);

            context.Response.StatusCode =
                StatusCodes.Status502BadGateway;

            await context.Response.WriteAsJsonAsync(
                error, 
                cancellationToken);

            return true;
        }
        //The device did not respond before the timeout (2 seconds)
        else if (exception is TimeoutException)
        {
            statusCode = StatusCodes.Status504GatewayTimeout;
            title = "Modbus response timeout";
            detail = "The Modbus device did not respond in time.";

            _logger.LogWarning(
                exception,
                "Modbus operation timed out: {Path}",
                context.Request.Path);
        }
        //The request cannot be completed in the current connection state
        else if(exception is InvalidOperationException)
        {
            statusCode = StatusCodes.Status409Conflict;
            title = "Invalid connection state";
            detail = "The Modbus connection is not available.";

            _logger.LogWarning(
                exception,
                "Invalid connection state: {Path}",
                context.Request.Path);

        }
        //The most likely cause for this exception is a failure to open the serial port
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = StatusCodes.Status409Conflict;
            title = "Serial port unavailable.";
            detail = "The requested serial port is already in use or access is denied.";

            _logger.LogError(
                exception,
                "Unhandled exception while processing {Path}",
                context.Request.Path);
        }
        else if (exception is BadHttpRequestException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            title  = "Invalid Request.";
            detail = "One or more request parameters have an invalid format or value.";

            _logger.LogError(
                exception,
                "Unhandled exception while processing {Path}",
                context.Request.Path);
        }
        else if(exception is FileNotFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            title = "COM Port Not found.";
            detail = "The requested COM port was not found.";

            _logger.LogError(
                exception,
                "Unhandled exception while processing {Path}",
                context.Request.Path);
        }
        //Handle unexpected exceptions
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            title = "Internal server error";
            detail = "An unexpected error occurred.";

            _logger.LogError(
                exception,
                "Unhandled exception while processing {Path}",
                context.Request.Path);
        }

        //Create an HTTP error response for non Modbus exceptions
        ProblemDetails problem = new()
        { 
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
