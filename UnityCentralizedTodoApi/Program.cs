using Microsoft.AspNetCore.Rewrite;
using UnityCentralizedTodoApi.Data;
using UnityCentralizedTodoApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TodoStore>();

var app = builder.Build();

// redirect /tasks to /todos for better UX when users mistype the URL
SetupRewriteMiddleware(app);
app.Use(SimpleMiddlewareTest());

app.MapTodoEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();

static void SetupRewriteMiddleware(WebApplication app)
{
    var rewriteOptions = new RewriteOptions()
        .AddRedirect("tasks", "todos")
        .AddRedirect("tasks/(.*)", "todos/$1");
    app.UseRewriter(rewriteOptions);
}

static Func<HttpContext, RequestDelegate, Task> SimpleMiddlewareTest()
{
    return async (context, next) =>
    {
        Console.WriteLine($"Received {context.Request.Method} request for {context.Request.Path}");
        await next(context);
        Console.WriteLine($"Responded with status code {context.Response.StatusCode}");
    };
}