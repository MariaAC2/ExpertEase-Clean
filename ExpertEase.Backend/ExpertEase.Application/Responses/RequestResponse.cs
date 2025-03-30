using ExpertEase.Application.Errors;

namespace ExpertEase.Application.Responses;

public class RequestResponse<T>
{
    public T? Response { get; private init; }
    public ErrorMessage? ErrorMessage { get; private init; }

    protected RequestResponse() { }

    public static RequestResponse CreateErrorResponse(ErrorMessage? error)
    {
        return error != null
            ? new RequestResponse
            {
                ErrorMessage = error
            }
            : new()
            {
                Response = "Ok"
            };
    }

    public static RequestResponse<string> CreateRequestResponseFromServiceResponse(ServiceResponse serviceResponse)
    {
        return CreateErrorResponse(serviceResponse.Error);
    }

    public static RequestResponse<T> CreateRequestResponseFromServiceResponse(ServiceResponse<T> serviceResponse)
    {
        return serviceResponse.Error != null
            ? new RequestResponse<T>
            {
                ErrorMessage = serviceResponse.Error
            }
            : new()
            {
                Response = serviceResponse.Result
            };
    }
}

public class RequestResponse : RequestResponse<string>
{
    public static RequestResponse OkRequestResponse => CreateErrorResponse(null);
}