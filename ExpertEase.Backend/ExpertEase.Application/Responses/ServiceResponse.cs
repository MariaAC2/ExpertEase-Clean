using ExpertEase.Application.Errors;

namespace ExpertEase.Application.Responses;

public class ServiceResponse
{
    public ErrorMessage? Error { get; private init; }
    public bool IsOk => Error == null;

    public static ServiceResponse CreateErrorResponse(ErrorMessage? error) => new() { Error = error };
    public static ServiceResponse<T> CreateErrorResponse<T>(ErrorMessage? error) => new() { Error = error };
    public static ServiceResponse CreateSuccessResponse() => new();
    public static ServiceResponse<T> CreateSuccessResponse<T>(T data) => new() { Result = data };
    public ServiceResponse ToResponse<T>(T result) => Error == null ? CreateSuccessResponse(result) : CreateErrorResponse(Error);

    protected ServiceResponse() { }
}

public class ServiceResponse<T> : ServiceResponse
{
    public T? Result { get; init; }
    public ServiceResponse ToResponse() => Error == null ? CreateSuccessResponse() : CreateErrorResponse(Error);

    protected internal ServiceResponse() { }
}