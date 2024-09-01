namespace TNRD.Zeepkist.GTR.Backend.Middleware;

public class LogBadRequestMiddleware
{
    private readonly RequestDelegate _next;

    public LogBadRequestMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Post)
        {
            // Enabling seeking to read the stream multiple times
            context.Request.EnableBuffering();

            // Save the original response body stream
            Stream originalBodyStream = context.Response.Body;
            await using MemoryStream responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                // Continue down the Middleware pipeline, eventually returning here
                await _next(context);

                // Check if the status code is BadRequest (400)
                if (context.Response.StatusCode == 400)
                {
                    // Rewind the request body to the beginning
                    context.Request.Body.Position = 0;

                    // Read the request body
                    string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                    // Log the request body here
                    Console.WriteLine($"BadRequest encountered with body: {requestBody}");
                }

                // Reset the request body position for further use
                context.Request.Body.Position = 0;

                // Copy the modified response stream back to the original stream
                responseBody.Position = 0;
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                // Restore the original response body stream
                context.Response.Body = originalBodyStream;
            }
        }
        else
        {
            await _next(context);
        }
    }
}
