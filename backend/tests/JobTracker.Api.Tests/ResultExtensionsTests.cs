using JobTracker.Api;
using JobTracker.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JobTracker.Api.Tests;

public sealed class ResultExtensionsTests
{
    [Fact]
    public void ToHttpResult_GivenSuccessResult_ThenReturns200WithValue()
    {
        var result = Result<string>.Success("hello").ToHttpResult();

        var ok = Assert.IsType<Ok<string>>(result);
        Assert.Equal("hello", ok.Value);
    }

    [Fact]
    public void ToHttpResult_GivenValidationFailure_ThenReturns422()
    {
        var result = Result<string>.Failure("invalid", ErrorKind.Validation).ToHttpResult();

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(422, problem.ProblemDetails.Status);
        Assert.Equal("invalid", problem.ProblemDetails.Detail);
    }

    [Fact]
    public void ToHttpResult_GivenNotFoundFailure_ThenReturns404()
    {
        var result = Result<string>.Failure("not found", ErrorKind.NotFound).ToHttpResult();

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(404, problem.ProblemDetails.Status);
    }

    [Fact]
    public void ToHttpResult_GivenConflictFailure_ThenReturns409()
    {
        var result = Result<string>.Failure("conflict", ErrorKind.Conflict).ToHttpResult();

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(409, problem.ProblemDetails.Status);
    }

    [Fact]
    public void ToCreatedResult_GivenSuccessResult_ThenReturns201()
    {
        var result = Result<string>.Success("hello").ToCreatedResult(v => $"/api/items/{v}");

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(201, statusResult.StatusCode);
    }

    [Fact]
    public void ToCreatedResult_GivenFailure_ThenReturnsProblem()
    {
        var result = Result<string>.Failure("not found", ErrorKind.NotFound).ToCreatedResult(v => $"/api/items/{v}");

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(404, problem.ProblemDetails.Status);
    }
}
