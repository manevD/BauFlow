namespace BauFlow.Middleware
{
    using System.Net;
    using System.Text.Json;

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex,
                       "Unhandled Exception for {Path} User {User}",
                       context.Request.Path,
                       context.User?.Identity?.Name);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var isApi = context.Request.Path.StartsWithSegments("/api");

            var statusCode = ex switch
            {
                UnauthorizedAccessException => HttpStatusCode.Forbidden,
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            if (!isApi)
            {
                context.Response.Redirect("/Home/Error?code=" + (int)statusCode);
                return Task.CompletedTask;
            }

            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var isDev = env.IsDevelopment();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                message = isDev
                    ? ex.Message
                    : "Ein Fehler ist aufgetreten."
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
