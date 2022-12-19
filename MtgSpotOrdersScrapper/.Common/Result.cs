namespace MtgSpotOrdersScrapper.Common;

public interface IResult
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    string? ErrorMessage { get; }
}

public interface IResult<out TData> : IResult
{
    public TData? Data { get; }
}

public class Result<TData> : IResult<TData> where TData : class
{
    public TData? Data { get; }
    public string? ErrorMessage { get; }

    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    public bool IsFailure => !IsSuccess;

    private Result(TData? data, string? errorMessage)
    {
        Data = data;
        ErrorMessage = errorMessage;
    }

    public static IResult<TData> Success(TData data)
    {
        return new Result<TData>(data, null);
    }

    public static IResult<TData> Failure(string errorMessage)
    {
        return new Result<TData>(null, errorMessage);
    }

    public static IResult<Empty> Success()
    {
        return new Result<Empty>(new Empty(),null);
    }
}