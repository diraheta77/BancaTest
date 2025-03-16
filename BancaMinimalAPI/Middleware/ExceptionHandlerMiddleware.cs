using BancaMinimalAPI.Common.Exceptions;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text.Json;

namespace BancaMinimalAPI.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                
                var errorResponse = new
                {
                    Message = error.Message,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };

                switch (error)
                {
                    case BusinessException e:
                        response.StatusCode = e.StatusCode;
                        errorResponse = new { Message = e.Message, StatusCode = e.StatusCode };
                        break;

                    case SqlException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorResponse = new { Message = "Error en la base de datos", StatusCode = 400 };
                        _logger.LogError(e, "SQL Error: {Message}", e.Message);
                        break;

                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        _logger.LogError(error, "Server Error: {Message}", error.Message);
                        break;
                }

                var result = JsonSerializer.Serialize(errorResponse);
                await response.WriteAsync(result);
            }
        }
    }
}