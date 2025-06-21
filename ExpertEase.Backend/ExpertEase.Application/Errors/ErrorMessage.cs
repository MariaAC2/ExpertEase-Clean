using System.Net;
using System.Text.Json.Serialization;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.Errors;

/// <summary>
/// This is a simple class to transmit the error information to the client.
/// It includes the message, custom error code to identify te specific error and the HTTP status code to be set on the HTTP response.
/// </summary>
public class ErrorMessage(HttpStatusCode status, string message, ErrorCodes code = ErrorCodes.Unknown)
{
    public string Message { get; } = message;
    public ErrorCodes Code { get; } = code;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HttpStatusCode Status { get; } = status;

    public static ErrorMessage FromException(ServerException exception) => new(exception.Status, exception.Message);
}
