using JobTracker.Domain.Common;

namespace JobTracker.Domain.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_GivenSuccessResult_ThenIsSuccessTrue()
    {
        var result = Result<int>.Success(1);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Value_GivenSuccessResult_ThenReturnsValue()
    {
        var result = Result<string>.Success("hello");

        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Error_GivenSuccessResult_ThenThrows()
    {
        var result = Result<int>.Success(1);

        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void Kind_GivenSuccessResult_ThenThrows()
    {
        var result = Result<int>.Success(1);

        Assert.Throws<InvalidOperationException>(() => _ = result.Kind);
    }

    [Fact]
    public void Failure_GivenFailureResult_ThenIsFailureTrue()
    {
        var result = Result<int>.Failure("bad");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Error_GivenFailureResult_ThenReturnsMessage()
    {
        var result = Result<int>.Failure("oops");

        Assert.Equal("oops", result.Error);
    }

    [Fact]
    public void Kind_GivenFailureResult_ThenReturnsKind()
    {
        var result = Result<int>.Failure("gone", ErrorKind.NotFound);

        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public void Value_GivenFailureResult_ThenThrows()
    {
        var result = Result<int>.Failure("bad");

        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Failure_GivenNoKindSpecified_ThenDefaultKindIsValidation()
    {
        var result = Result<int>.Failure("bad");

        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public void Failure_GivenExplicitKind_ThenKindPreserved()
    {
        var result = Result<int>.Failure("conflict", ErrorKind.Conflict);

        Assert.Equal(ErrorKind.Conflict, result.Kind);
    }
}
