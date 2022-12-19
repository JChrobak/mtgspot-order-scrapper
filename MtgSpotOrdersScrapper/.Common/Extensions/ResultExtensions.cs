namespace MtgSpotOrdersScrapper.Common;

public static class ResultExtensions
{
    public static IResult<TData> MapError<TData>(this IResult result) where TData : class
    {
        return Result<TData>.Failure(result.ErrorMessage!);
    }

    public static IResult<TData> Success<TData>(this TData data) where TData : class
    {
        return Result<TData>.Success(data);
    }

    public static IResult<TData> Failure<TData>(this string errorMessage) where TData : class
    {
        return Result<TData>.Failure(errorMessage);
    }
}