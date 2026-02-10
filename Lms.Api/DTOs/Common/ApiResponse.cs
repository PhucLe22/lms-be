namespace Lms.Api.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ApiResponse<T> Ok(T data, string message = "OK") =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Data = default, Message = message };
}

public class ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ApiResponse Ok(string message = "OK") =>
        new() { Success = true, Message = message };

    public static ApiResponse Fail(string message) =>
        new() { Success = false, Message = message };
}
