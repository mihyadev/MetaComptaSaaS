namespace MetaCompta.BackOffice.Tests.DTOs;

/// <summary>
/// Tests for ApiResponse factory methods and PagedResult computed properties.
/// </summary>
public class ApiResponseTests
{
    [Fact]
    public void Ok_SetsSuccessAndData()
    {
        var response = ApiResponse<string>.Ok("test", "Done");

        response.Success.Should().BeTrue();
        response.Data.Should().Be("test");
        response.Message.Should().Be("Done");
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Fail_SetsSuccessFalseAndMessage()
    {
        var errors = new List<string> { "Field1 required", "Field2 invalid" };
        var response = ApiResponse<string>.Fail("Validation failed", errors);

        response.Success.Should().BeFalse();
        response.Message.Should().Be("Validation failed");
        response.Data.Should().BeNull();
        response.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Ok_WithoutMessage_HasNullMessage()
    {
        var response = ApiResponse<int>.Ok(42);

        response.Success.Should().BeTrue();
        response.Data.Should().Be(42);
        response.Message.Should().BeNull();
    }
}

public class PagedResultTests
{
    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        var result = new PagedResult<string>
        {
            Items = new List<string> { "a", "b" },
            TotalCount = 25,
            Page = 1,
            PageSize = 10
        };

        result.TotalPages.Should().Be(3); // ceil(25/10) = 3
    }

    [Fact]
    public void TotalPages_ExactDivision()
    {
        var result = new PagedResult<string>
        {
            TotalCount = 20,
            PageSize = 10
        };

        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public void TotalPages_ZeroItems()
    {
        var result = new PagedResult<string>
        {
            TotalCount = 0,
            PageSize = 10
        };

        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void TotalPages_SingleItem()
    {
        var result = new PagedResult<string>
        {
            TotalCount = 1,
            PageSize = 10
        };

        result.TotalPages.Should().Be(1);
    }
}
