using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MetaCompta.BackOffice.Tests.Helpers;

/// <summary>
/// Provides helper methods for setting up controllers with mocked dependencies in unit tests.
/// </summary>
public static class ControllerTestHelper
{
    /// <summary>
    /// Creates a mock IHttpContextAccessor with an in-memory session containing a JWT token.
    /// </summary>
    public static Mock<IHttpContextAccessor> CreateMockContextAccessor(bool authenticated = true)
    {
        var session = new MockSession();
        if (authenticated)
        {
            session.SetString("JwtToken", "mock-jwt-token");
            session.SetString("UserName", "Test User");
            session.SetString("UserRole", "Manager");
            session.SetString("CabinetName", "Test Cabinet");
            session.SetString("CabinetId", "1");
        }

        var httpContext = new DefaultHttpContext { Session = session };
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(httpContext);
        return mock;
    }

    /// <summary>
    /// Configures a controller with HttpContext, TempData, and routing needed for unit testing.
    /// </summary>
    public static void SetupController(Controller controller, bool authenticated = true)
    {
        var session = new MockSession();
        if (authenticated)
        {
            session.SetString("JwtToken", "mock-jwt-token");
            session.SetString("UserName", "Test User");
            session.SetString("UserRole", "Manager");
            session.SetString("CabinetName", "Test Cabinet");
            session.SetString("CabinetId", "1");
            session.SetString("HasActiveContract", "True");
        }

        var httpContext = new DefaultHttpContext { Session = session };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
        };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    /// <summary>
    /// Creates a mock IReferenceDataFacade that returns empty dictionaries by default.
    /// </summary>
    public static Mock<IReferenceDataFacade> CreateMockReferenceData()
    {
        var mock = new Mock<IReferenceDataFacade>();
        mock.Setup(x => x.GetMultipleAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new Dictionary<string, List<ReferenceDataDto>>());
        return mock;
    }

    /// <summary>
    /// Creates a mock IReferenceDataFacade that returns data for the specified categories.
    /// </summary>
    public static Mock<IReferenceDataFacade> CreateMockReferenceDataWith(params string[] categories)
    {
        var mock = new Mock<IReferenceDataFacade>();
        var data = new Dictionary<string, List<ReferenceDataDto>>();
        foreach (var cat in categories)
        {
            data[cat] = new List<ReferenceDataDto>
            {
                new() { Id = 1, Category = cat, Code = $"{cat}_1", Label = $"{cat} Item 1", SortOrder = 1 },
                new() { Id = 2, Category = cat, Code = $"{cat}_2", Label = $"{cat} Item 2", SortOrder = 2 }
            };
        }
        mock.Setup(x => x.GetMultipleAsync(It.IsAny<string[]>()))
            .ReturnsAsync(data);
        return mock;
    }
}

/// <summary>
/// A simple in-memory ISession implementation for unit testing.
/// </summary>
public class MockSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();

    public string Id => Guid.NewGuid().ToString();
    public bool IsAvailable => true;
    public IEnumerable<string> Keys => _store.Keys;

    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);

    public void Set(string key, byte[] value) => _store[key] = value;

    public bool TryGetValue(string key, out byte[] value)
    {
        if (_store.TryGetValue(key, out var stored))
        {
            value = stored;
            return true;
        }
        value = Array.Empty<byte>();
        return false;
    }
}
