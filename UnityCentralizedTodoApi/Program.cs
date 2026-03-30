using System.Text.Json;
using System.Text.Json.Serialization;
using UnityCentralizedTodoApi.Data;
using UnityCentralizedTodoApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
});
builder.Services.AddSingleton<ITodoRepository, TodoStore>();

var app = builder.Build();

app.Use(SimpleMiddlewareTest());

app.MapTodoEndpoints();

app.MapGet("/", () => "Use /projects or /projects/{projectKey}/todos.");

app.Run();

static Func<HttpContext, RequestDelegate, Task> SimpleMiddlewareTest()
{
    return async (context, next) =>
    {
        Console.WriteLine($"Received {context.Request.Method} request for {context.Request.Path}");
        await next(context);
        Console.WriteLine($"Responded with status code {context.Response.StatusCode}");
    };
}