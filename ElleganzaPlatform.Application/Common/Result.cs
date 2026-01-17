namespace ElleganzaPlatform.Application.Common;

public class Result
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static Result Ok(string? message = null)
        => new Result { Success = true, Message = message };

    public static Result Fail(string error)
        => new Result { Success = false, Errors = new List<string> { error } };

    public static Result Fail(List<string> errors)
        => new Result { Success = false, Errors = errors };
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Ok(T data, string? message = null)
        => new Result<T> { Success = true, Data = data, Message = message };

    public static new Result<T> Fail(string error)
        => new Result<T> { Success = false, Errors = new List<string> { error } };

    public static new Result<T> Fail(List<string> errors)
        => new Result<T> { Success = false, Errors = errors };
}
