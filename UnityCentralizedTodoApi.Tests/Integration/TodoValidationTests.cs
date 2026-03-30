using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UnityCentralizedTodoApi.Contracts;
using UnityCentralizedTodoApi.Tests.Helpers;

namespace UnityCentralizedTodoApi.Tests.Integration;

public class TodoValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string Project = "validation-tests";
    private const string Base = $"/projects/{Project}/todos";

    public TodoValidationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // -----------------------------------------------------------------------
    // Name
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_EmptyName_Returns400NameRequired(string name)
    {
        var response = await _client.PostAsJsonAsync(Base, new
        {
            name,
            severity = "low",
            dueDate = "2026-06-01T00:00:00Z",
            isComplete = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>(TestJsonOptions.Api);
        Assert.Equal("validation.name_required", error!.Code);
        Assert.Equal("Name", error.Field);
    }

    // -----------------------------------------------------------------------
    // Due date
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Post_DueDateInPast_Returns400DueDatePast()
    {
        var response = await _client.PostAsJsonAsync(Base, new
        {
            name = "Past date test",
            severity = "low",
            dueDate = "2020-01-01T00:00:00Z",
            isComplete = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>(TestJsonOptions.Api);
        Assert.Equal("validation.due_date_past", error!.Code);
        Assert.Equal("DueDate", error.Field);
    }

    // -----------------------------------------------------------------------
    // Severity
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("urgent")]
    [InlineData("blocker")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_InvalidSeverity_Returns400SeverityInvalid(string severity)
    {
        var response = await _client.PostAsJsonAsync(Base, new
        {
            name = "Severity test",
            severity,
            dueDate = "2026-06-01T00:00:00Z",
            isComplete = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>(TestJsonOptions.Api);
        Assert.Equal("validation.severity_invalid", error!.Code);
        Assert.Equal("Severity", error.Field);
    }

    // -----------------------------------------------------------------------
    // ID
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99)]
    public async Task GetById_NonPositiveId_Returns400IdNonPositive(int id)
    {
        var response = await _client.GetAsync($"{Base}/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>(TestJsonOptions.Api);
        Assert.Equal("validation.id_non_positive", error!.Code);
        Assert.Equal("Id", error.Field);
    }

    // -----------------------------------------------------------------------
    // Project key
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("invalid!")]
    [InlineData("has space")]
    [InlineData("has!special")]
    public async Task Get_InvalidProjectKey_Returns400ProjectKeyInvalid(string projectKey)
    {
        var response = await _client.GetAsync($"/projects/{Uri.EscapeDataString(projectKey)}/todos");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>(TestJsonOptions.Api);
        Assert.Equal("validation.project_key_invalid", error!.Code);
        Assert.Equal("ProjectKey", error.Field);
    }

    // -----------------------------------------------------------------------
    // Query filters
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Get_InvalidSeverityFilter_Returns400SeverityInvalid()
    {
        var response = await _client.GetAsync($"{Base}?severity=urgent");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>(TestJsonOptions.Api);
        Assert.Equal("validation.severity_invalid", error!.Code);
    }

    [Fact]
    public async Task Get_InvalidSortBy_Returns400SortByInvalid()
    {
        var response = await _client.GetAsync($"{Base}?sortBy=random");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>(TestJsonOptions.Api);
        Assert.Equal("validation.sort_by_invalid", error!.Code);
    }

    [Fact]
    public async Task Get_DueFromAfterDueTo_Returns400DueRangeInvalid()
    {
        var response = await _client.GetAsync($"{Base}?dueFrom=2026-05-02T00:00:00Z&dueTo=2026-05-01T00:00:00Z");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>(TestJsonOptions.Api);
        Assert.Equal("validation.due_range_invalid", error!.Code);
    }

    // -----------------------------------------------------------------------
    // Error shape
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ErrorResponse_ContainsAllExpectedFields()
    {
        var response = await _client.GetAsync($"{Base}/0");

        var error = await response.Content.ReadFromJsonAsync<ApiError>(TestJsonOptions.Api);
        Assert.NotNull(error);
        Assert.NotNull(error!.Code);
        Assert.NotNull(error.Message);
        Assert.NotNull(error.Field);
        Assert.Equal((int)HttpStatusCode.BadRequest, error.Status);
        Assert.NotNull(error.TraceId);
    }
}
