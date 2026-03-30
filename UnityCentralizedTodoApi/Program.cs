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

app.MapTodoEndpoints();

app.MapGet("/", () => "Use /projects or /projects/{projectKey}/todos.");

app.Run();

// Needed so WebApplicationFactory<Program> in the test project can reference this assembly.
public partial class Program { }