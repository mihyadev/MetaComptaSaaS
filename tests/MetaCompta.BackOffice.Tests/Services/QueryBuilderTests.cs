namespace MetaCompta.BackOffice.Tests.Services;

/// <summary>
/// Tests for the fluent QueryBuilder utility.
/// </summary>
public class QueryBuilderTests
{
    [Fact]
    public void For_ReturnsQueryBuilder()
    {
        var builder = QueryBuilder.For("api/clients");
        builder.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithNoParams_ReturnsBaseUrl()
    {
        var url = QueryBuilder.For("api/clients").Build();
        url.Should().Be("api/clients");
    }

    [Fact]
    public void Build_WithSingleParam_AppendsQueryString()
    {
        var url = QueryBuilder.For("api/clients")
            .Add("page", 1)
            .Build();

        url.Should().Be("api/clients?page=1");
    }

    [Fact]
    public void Build_WithMultipleParams_JoinsWithAmpersand()
    {
        var url = QueryBuilder.For("api/clients")
            .Add("page", 1)
            .Add("search", "test")
            .Add("statut", "Active")
            .Build();

        url.Should().Be("api/clients?page=1&search=test&statut=Active");
    }

    [Fact]
    public void Build_SkipsNullStringParam()
    {
        var url = QueryBuilder.For("api/clients")
            .Add("page", 1)
            .Add("search", (string?)null)
            .Build();

        url.Should().Be("api/clients?page=1");
    }

    [Fact]
    public void Build_SkipsEmptyStringParam()
    {
        var url = QueryBuilder.For("api/employees")
            .Add("page", 1)
            .Add("search", "")
            .Build();

        url.Should().Be("api/employees?page=1");
    }

    [Fact]
    public void Build_SkipsWhitespaceStringParam()
    {
        var url = QueryBuilder.For("api/employees")
            .Add("page", 1)
            .Add("search", "   ")
            .Build();

        url.Should().Be("api/employees?page=1");
    }

    [Fact]
    public void Build_SkipsNullIntParam()
    {
        var url = QueryBuilder.For("api/employees")
            .Add("page", 1)
            .Add("clientId", (int?)null)
            .Build();

        url.Should().Be("api/employees?page=1");
    }

    [Fact]
    public void Build_IncludesIntParam_WhenHasValue()
    {
        var url = QueryBuilder.For("api/employees")
            .Add("clientId", (int?)42)
            .Build();

        url.Should().Be("api/employees?clientId=42");
    }

    [Fact]
    public void Build_BoolTrue_IncludesParam()
    {
        var url = QueryBuilder.For("api/reports")
            .Add("includeArchived", true)
            .Build();

        url.Should().Be("api/reports?includeArchived=true");
    }

    [Fact]
    public void Build_BoolFalse_SkipsParam()
    {
        var url = QueryBuilder.For("api/reports")
            .Add("includeArchived", false)
            .Build();

        url.Should().Be("api/reports");
    }

    [Fact]
    public void Build_UrlEncodesSpecialCharacters()
    {
        var url = QueryBuilder.For("api/clients")
            .Add("search", "café & thé")
            .Build();

        url.Should().Contain("search=caf%c3%a9+%26+th%c3%a9");
    }

    [Fact]
    public void ImplicitConversion_ReturnsBuiltUrl()
    {
        string url = QueryBuilder.For("api/test").Add("page", 1);
        url.Should().Be("api/test?page=1");
    }
}
