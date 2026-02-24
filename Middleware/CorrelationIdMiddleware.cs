namespace BauFlow.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId;

            if (context.Request.Headers.ContainsKey(HeaderName))
            {
                correlationId = context.Request.Headers[HeaderName];
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // verfügbar für gesamte Request Pipeline
            context.Items[HeaderName] = correlationId;

            // Response Header setzen
            context.Response.Headers[HeaderName] = correlationId;

            using (_logger.BeginScope("{CorrelationId}", correlationId))
            {
                await _next(context);
            }
        }
    }
}
