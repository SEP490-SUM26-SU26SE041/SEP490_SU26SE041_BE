namespace SmartFarmSEP490.API.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Thành công") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Created(T data, string message = "Tạo thành công") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Error(string message) =>
        new() { Success = false, Message = message, Data = default };
}

public class ApiResponse : ApiResponse<object>
{
    public new static ApiResponse Error(string message) =>
        new() { Success = false, Message = message, Data = null };

    public new static ApiResponse Ok(string message = "Thành công") =>
        new() { Success = true, Message = message, Data = null };

    public new static ApiResponse Created(string message = "Tạo thành công") =>
        new() { Success = true, Message = message, Data = null };
}
