using Microsoft.AspNetCore.Mvc.Testing;

namespace UnityCentralizedTodoApi.Tests.Integration;

/// <summary>
/// Shared WebApplicationFactory fixture. All integration test classes that use
/// IClassFixture&lt;TodoApiFixture&gt; share one in-process host.
/// </summary>
public sealed class TodoApiFixture : WebApplicationFactory<Program>
{
    // No overrides needed for the default in-memory configuration.
    // Add WithWebHostBuilder() overrides here if tests need to swap services.
}
