using JobTracker.Domain.Common;

namespace JobTracker.Api;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return TypedResults.Ok(result.Value);

        return result.Kind switch
        {
            ErrorKind.NotFound => Results.Problem(detail: result.Error, statusCode: 404),
            ErrorKind.Conflict => Results.Problem(detail: result.Error, statusCode: 409),
            _ => Results.Problem(detail: result.Error, statusCode: 422),
        };
    }

    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> locationFn)
    {
        if (result.IsSuccess)
            return Results.Created(locationFn(result.Value), result.Value);

        return result.Kind switch
        {
            ErrorKind.NotFound => Results.Problem(detail: result.Error, statusCode: 404),
            ErrorKind.Conflict => Results.Problem(detail: result.Error, statusCode: 409),
            _ => Results.Problem(detail: result.Error, statusCode: 422),
        };
    }
}
